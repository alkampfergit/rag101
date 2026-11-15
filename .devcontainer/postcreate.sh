#!/bin/bash
# Post-creation setup script for the development container
set -e

# Every downloader below runs over HTTPS only, TLS 1.2 or better. Kept in one
# array so the policy is stated once rather than repeated per `curl`.
readonly CURL_TLS_ARGS=(--proto '=https' --tlsv1.2)

# Run from the repository root whatever the caller's working directory is.
REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$REPO_ROOT"

# The installers below (Claude Code, uv, beads) put binaries in ~/.local/bin,
# which may not exist yet when this shell starts.
export PATH="$HOME/.local/bin:$PATH"

echo "=========================================="
echo "Starting devcontainer post-creation setup"
echo "=========================================="

# Fix apt sources issue with yarn (copied from reference container)
echo "Cleaning up apt sources..."
sudo rm -f /etc/apt/sources.list.d/yarn.list

# Update apt and install utilities
echo "Installing system dependencies..."
sudo apt-get update
sudo apt-get install -y git-flow

# Setup git aliases
echo "Configuring git aliases..."
bash .devcontainer/setup-git-aliases.sh

# ---------------------------------------------------------------------------
# rag101 project setup (replaces azdo-cli's Node-only steps).
# It runs before the agent tooling so that a failed download further down
# (this script uses "set -e") still leaves a restored and built project.
# Every step here warns instead of aborting, for the opposite reason: a failed
# restore or build must not leave the container without its tooling.
# ---------------------------------------------------------------------------
PROJECT_WARNINGS=()
warn() {
    echo "WARNING: $1"
    PROJECT_WARNINGS+=("$1")
}

echo "Setting up the rag101 project..."
dotnet --version || warn "dotnet is not on PATH; check the dotnet feature in devcontainer.json"
node --version || warn "node is not on PATH"

# web/node_modules is a named volume (see docker-compose.yml) and is created
# owned by root; hand it to the current user before npm writes into it.
if [[ -d web/node_modules ]]; then
    sudo chown "$(id -u):$(id -g)" web/node_modules || warn "could not chown web/node_modules"
fi

# The MSBuild target in Rag101.Api.csproj runs "npm ci" only when
# web/node_modules is missing. Here it always exists (it is a mount point), so
# the packages must be installed explicitly.
echo "Installing frontend packages (npm ci in web/)..."
(cd web && npm ci) || warn "npm ci failed in web/"

echo "Restoring .NET packages..."
dotnet restore Rag101.slnx || warn "dotnet restore failed"

# Also builds the frontend into src/Rag101.Api/wwwroot.
echo "Building the solution..."
dotnet build Rag101.slnx --no-restore || warn "dotnet build failed"

# MongoDB and Elasticsearch come from docker-compose.yml. Report whether they
# answer; Elasticsearch can take a minute to start, so wait a little for it.
host_port_open() {
    timeout 3 bash -c "exec 3<>/dev/tcp/$1/$2" 2>/dev/null
}

echo "Checking MongoDB at mongo:27017..."
if host_port_open mongo 27017; then
    echo "  MongoDB is reachable (RAG_MONGODB_CONNECTION=${RAG_MONGODB_CONNECTION:-<unset>})."
else
    warn "MongoDB is not reachable at mongo:27017"
fi

ES_URL="${RAG_TEST_ELASTICSEARCH_URL:-http://elasticsearch:9200}"
echo "Checking Elasticsearch at ${ES_URL}..."
es_ready=""
for _ in $(seq 1 30); do
    if curl -fsS --max-time 3 "$ES_URL" >/dev/null 2>&1; then
        es_ready="yes"
        break
    fi
    sleep 2
done
if [[ -n "$es_ready" ]]; then
    echo "  Elasticsearch is reachable."
else
    warn "Elasticsearch is not reachable at ${ES_URL} (it may still be starting; check 'docker compose logs elasticsearch' on the host)"
fi


# Install Claude Code CLI via official native installer (auto-updates)
# See: https://docs.anthropic.com/en/docs/claude-code/overview
echo "Installing Claude Code CLI..."
curl -fsSL "${CURL_TLS_ARGS[@]}" https://claude.ai/install.sh | bash || true

# Install CLI tools that are distributed via npm
if command -v npm >/dev/null 2>&1; then
    echo "Installing OpenAI Codex..."
    npm install -g @openai/codex --ignore-scripts || true
    echo "Installing automata-cli..."
    npm install -g automata-cli --ignore-scripts || true
else
    echo "npm not available, skipping npm-based CLI installs."
fi

# Install beads
echo "Installing beads..."
curl -fsSL "${CURL_TLS_ARGS[@]}" https://raw.githubusercontent.com/steveyegge/beads/v0.49.6/scripts/install.sh | bash

# Install uv (Astral) and GitHub spec-kit via uv tool
# uv provides a universal version manager; we install via official script
if ! command -v uv >/dev/null 2>&1; then
    echo "Installing uv..."
    curl -LsSf "${CURL_TLS_ARGS[@]}" https://astral.sh/uv/install.sh | sh
else
    echo "uv already installed, skipping."
fi

# use uv to install github spec-kit command-line tool
if command -v uv >/dev/null 2>&1; then
    echo "Installing github spec-kit via uv..."
    uv tool install specify-cli --no-build || true
else
    echo "uv not available, cannot install spec-kit."
fi

# OWNER ATTENTION: not ported from azdo-cli. There, the next two steps install
# GNOME Keyring for the credential-store integration tests and initialise it
# with scripts/setup-keyring.sh. rag101 has no credential store on Linux (the
# Bootstrap connection comes only from RAG_MONGODB_CONNECTION) and no
# scripts/setup-keyring.sh, so they are kept here disabled. Delete them, or
# uncomment them if a Linux credential store is ever added.
# sudo apt-get install -y gnome-keyring libsecret-tools xvfb xdotool python3-dbus python3-gi
# [[ -f scripts/setup-keyring.sh ]] && bash scripts/setup-keyring.sh || true

# tokensave: semantic code intelligence for Claude Code and Codex CLI.
echo "Installing tokensave..."
TOKENSAVE_TAG=$(curl -sI https://github.com/aovestdipaperino/tokensave/releases/latest | grep -i '^location:' | sed 's|.*/tag/||;s/\r//')
TOKENSAVE_VERSION="${TOKENSAVE_TAG#v}"
ARCH=$(uname -m)
if [[ "$ARCH" = "aarch64" ]] || [[ "$ARCH" = "arm64" ]]; then
    TOKENSAVE_ARCH="aarch64-linux"
else
    TOKENSAVE_ARCH="x86_64-linux"
fi
TOKENSAVE_URL="https://github.com/aovestdipaperino/tokensave/releases/download/${TOKENSAVE_TAG}/tokensave-${TOKENSAVE_TAG}-${TOKENSAVE_ARCH}.tar.gz"
echo "  Downloading tokensave ${TOKENSAVE_VERSION} (${TOKENSAVE_ARCH})..."
curl -sL "${CURL_TLS_ARGS[@]}" "$TOKENSAVE_URL" -o /tmp/tokensave.tar.gz
tar xzf /tmp/tokensave.tar.gz -C /tmp
sudo mv /tmp/tokensave /usr/local/bin/tokensave
rm -f /tmp/tokensave.tar.gz
echo "  tokensave $(tokensave --version) installed."

echo "  Configuring tokensave for Claude Code..."
tokensave install --agent claude || true
echo "  Configuring tokensave for Codex CLI..."
tokensave install --agent codex || true
tokensave enable-upload-counter || true

echo "  Indexing repository..."
tokensave sync || true
echo "  tokensave setup complete."

# Install Homebrew and rtk.
append_if_missing() {
    local line="$1"
    local file="$2"

    touch "$file"
    if ! grep -Fqx "$line" "$file"; then
        echo "$line" >>"$file"
    fi
}

if ! command -v brew >/dev/null 2>&1; then
    echo "Installing Homebrew..."
    NONINTERACTIVE=1 bash -c "$(curl -fsSL "${CURL_TLS_ARGS[@]}" https://raw.githubusercontent.com/Homebrew/install/HEAD/install.sh)"
else
    echo "Homebrew already installed, skipping."
fi

if [[ -x /home/linuxbrew/.linuxbrew/bin/brew ]]; then
    BREW_BIN="/home/linuxbrew/.linuxbrew/bin/brew"
elif [[ -x /opt/homebrew/bin/brew ]]; then
    BREW_BIN="/opt/homebrew/bin/brew"
else
    BREW_BIN=""
fi

if [[ -n "$BREW_BIN" ]]; then
    BREW_SHELLENV_LINE="eval \"\$($BREW_BIN shellenv)\""
    append_if_missing "$BREW_SHELLENV_LINE" "$HOME/.zprofile"
    append_if_missing "$BREW_SHELLENV_LINE" "$HOME/.zshrc"
    append_if_missing "$BREW_SHELLENV_LINE" "$HOME/.bashrc"
    eval "$("$BREW_BIN" shellenv)"

    if brew list --formula rtk >/dev/null 2>&1; then
        echo "rtk already installed, skipping."
    else
        echo "Installing rtk via Homebrew..."
        brew install rtk
    fi

    echo "Configuring rtk for Claude Code..."
    rtk init --global --auto-patch || true
    echo "Configuring rtk for Codex CLI..."
    rtk init --global --codex --auto-patch || true
else
    echo "Homebrew install did not expose brew on a known path."
    exit 1
fi

# Expose .agents/skills/ to Claude Code by mirroring them under .claude/skills/
# (Claude Code's auto-discovery path). Idempotent.
# OWNER ATTENTION: azdo-cli keeps its skills in .agents/skills/ and links them
# with scripts/link-skills.sh. rag101 has neither: its Spec Kit skills are
# committed directly under .claude/skills/, so this is a no-op until both exist.
if [[ -f scripts/link-skills.sh ]]; then
    echo "Linking skills into .claude/skills/..."
    bash scripts/link-skills.sh
else
    echo "No scripts/link-skills.sh in this repository, skipping skill linking."
fi

echo "=========================================="
if [[ ${#PROJECT_WARNINGS[@]} -eq 0 ]]; then
    echo "Post-creation setup complete."
else
    echo "Post-creation setup complete with ${#PROJECT_WARNINGS[@]} warning(s):"
    printf '  - %s\n' "${PROJECT_WARNINGS[@]}"
fi
echo "Run the application with: dotnet run --project src/Rag101.Api"
echo "then open http://localhost:5101"
echo "=========================================="
