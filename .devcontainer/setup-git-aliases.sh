#!/bin/bash
# Setup git aliases for the development container

echo "Setting up git aliases..."

# The azdo-cli original hardcodes origin/develop (GitFlow). rag101 has no
# develop branch: its trunk is master, so the aliases that compare or rebase
# against the base branch (bstatus, rod, rfi) use this variable instead.
# OWNER ATTENTION: "rod" keeps its name (rebase on develop) for muscle memory;
# set GIT_ALIAS_BASE_BRANCH if the base branch ever changes.
BASE_BRANCH="${GIT_ALIAS_BASE_BRANCH:-master}"

# Log aliases
git config --global alias.logf 'log --graph --oneline --all --decorate'
git config --global alias.logr 'log --graph --oneline --decorate'

# Branch status alias
git config --global alias.bstatus '!f() { git log HEAD...origin/'"$BASE_BRANCH"' --oneline --left-right | awk '\''{ print substr($0, 1, 1)}'\'' | sort -n | uniq -c; }; f'

# Rebase and fetch aliases
git config --global alias.rod '!git fetch --prune && git rebase origin/'"$BASE_BRANCH"
git config --global alias.fpull '!git fetch --prune && git pull --rebase'
git config --global alias.rfi '!git rebase -i $(git merge-base HEAD origin/'"$BASE_BRANCH"')'
git config --global alias.rc 'rebase --continue'

# Commit aliases
git config --global alias.amen '!git commit -a --amend -C HEAD'
git config --global alias.pamen '!git commit -a --amend -C HEAD && git push --force-with-lease'

# Push aliases
git config --global alias.pushf 'push --force-with-lease'
