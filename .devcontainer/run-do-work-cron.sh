#!/usr/bin/env bash
set -u -o pipefail

cd /workspaces/rag101 || exit 1

# OWNER ATTENTION: rag101 has no .automata/ directory yet (azdo-cli has
# config.json, claude-system-prompt.md, do-work-issue-discuss.md,
# do-work-pr-work.md and sonar-prompt.md there). Until its config exists the
# tick is a no-op instead of an error in the log every 5 minutes.
if [[ ! -f .automata/config.json ]]; then
    printf '%s automata do-work skipped: .automata/config.json not found\n' "$(date -u +%FT%TZ)"
    exit 0
fi

# cron starts jobs with an almost empty environment: neither the variables set
# in docker-compose.yml nor the PATH/DOTNET_ROOT added by the dotnet feature
# reach this script, so restore what the agent needs to build, run and test.
# Keep the two defaults in sync with docker-compose.yml.
export RAG_MONGODB_CONNECTION="${RAG_MONGODB_CONNECTION:-mongodb://mongo:27017}"
export RAG_TEST_ELASTICSEARCH_URL="${RAG_TEST_ELASTICSEARCH_URL:-http://elasticsearch:9200}"
if [[ -d /usr/share/dotnet ]]; then
    export DOTNET_ROOT="${DOTNET_ROOT:-/usr/share/dotnet}"
    export PATH="${PATH}:${DOTNET_ROOT}:${HOME}/.dotnet/tools"
fi

# Run through a login shell so PATH additions from postcreate (npm globals,
# ~/.local/bin, brew) are available to the tick.
if [[ -x /usr/bin/zsh ]]; then
    exec /usr/bin/zsh -lic 'exec automata do-work --silent'
fi

exec /bin/bash -lc 'exec automata do-work --silent'
