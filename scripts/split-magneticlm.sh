#!/usr/bin/env bash
#
# split-magneticlm.sh
#
# Splits the MagneticLM research project out of the current monorepo into
# a fresh /tmp/magneticlm working tree with full git history.
#
# Usage:
#   bash scripts/split-magneticlm.sh <your-github-username-or-org>

set -euo pipefail

ACCOUNT="${1:-}"
if [ -z "$ACCOUNT" ]; then
    echo "usage: $0 <github-account-or-org>" >&2
    exit 1
fi

REPO_NAME="magneticlm"
DEST="/tmp/$REPO_NAME"
SRC="$(cd "$(dirname "$0")/.." && pwd)"

cd "$SRC"

if [ ! -d .git ]; then
    echo "ERROR: $SRC is not a git repo" >&2
    exit 1
fi

if [ -n "$(git status --porcelain)" ]; then
    echo "ERROR: working tree is dirty. Commit or stash first." >&2
    git status --short
    exit 1
fi

if [ -e "$DEST" ]; then
    echo "ERROR: $DEST already exists. Remove it first:  rm -rf $DEST" >&2
    exit 1
fi

PATH_IN_MONOREPO="Examples/ACommerce.MagneticLM"

if [ ! -d "$PATH_IN_MONOREPO" ]; then
    echo "ERROR: $PATH_IN_MONOREPO does not exist in the source repo." >&2
    exit 1
fi

echo "==> Splitting $PATH_IN_MONOREPO into $DEST"

# The subtree split carries ONLY the history that touched this path.
echo -n "  SPLIT $PATH_IN_MONOREPO ... "
SHA=$(git subtree split --prefix="$PATH_IN_MONOREPO" HEAD)
echo "${SHA:0:10}"

# Bootstrap the destination repo + import the split sha. We use
# fetch-then-read-tree because it works in a single fresh init without
# the "unrelated histories" gymnastics that `git pull` requires.
echo "==> Bootstrapping $DEST"
mkdir -p "$DEST"
cd "$DEST"
git init -q -b main
git fetch -q "$SRC" "$SHA"
# Files from the split land at the repo root (the split already lifted
# the prefix away). --reset is required so read-tree happily writes into
# a fresh empty repo and updates the working tree in one shot. The README
# we add below sits next to colab/.
git read-tree --reset -u "$SHA"

# Root-level README pointing to the research notes
cat > README.md <<'EOF'
# MagneticLM

A graph-based language model that hits **14.20 perplexity on WikiText-103**
with no neural networks — just Modified Kneser–Ney 5-gram counting and a
3D physics simulation for word positions.

**Primary entry point: [colab/RESEARCH-NOTES.md](colab/RESEARCH-NOTES.md)**

That file is the continuation log: what's done, what's not, current
results on Colab / Kaggle T4 / T4 x2, open questions, and the next
experiments to try. Read it before touching the code so you don't
need to re-derive the research state.

## Run on Kaggle (dual T4)

```bash
python MagneticLMFastRunner.py \
    --train-lines 1000000 \
    --physics-iters 300 \
    --max-order 5 \
    --multi-gpu
```

Last reported numbers:

| Config | PPL |
|---|---|
| 860k lines, 30 iters, order 3, single T4 | 21.72 |
| 860k lines, 300 iters, order 3, single T4 | 20.94 |
| 860k lines, 300 iters, order 4, T4 x2 | 16.39 |
| 860k lines, 300 iters, order 5, T4 x2 | **14.36** |
| 860k lines, 1000 iters, order 5, T4 x2 | **14.20** |

For comparison on WT103: Transformer-XL ≈ 16.4, GPT-2 small ≈ 35,
AWD-LSTM+Cache ≈ 52, published KN-5 ≈ 141.
EOF

git add -A
# This is the second commit (the first was the read-tree import).
git commit -q -m "docs: root README pointing to RESEARCH-NOTES.md" || true

# Sanity check
TRACKED=$(git ls-files | wc -l)
COMMITS=$(git rev-list --count HEAD)

echo
echo "==============================================================="
echo "  DONE. New repo at $DEST"
echo "  Files tracked: $TRACKED       Commits: $COMMITS"
echo "==============================================================="
echo
if [ "$TRACKED" -lt 5 ]; then
    echo "  WARNING: only $TRACKED tracked files. Something is wrong —"
    echo "  the import step probably failed. Check the messages above."
    echo
fi
echo "Next steps:"
echo
echo "  1. Create an empty repo on GitHub. Two ways:"
echo
echo "     a) With the GitHub CLI (if installed):"
echo "        gh repo create $ACCOUNT/$REPO_NAME --public \\"
echo "            --description \"Graph-based LM — 14.20 PPL on WikiText-103\""
echo
echo "     b) Without 'gh' — open in a browser:"
echo "        https://github.com/new"
echo "        Owner = $ACCOUNT     Name = $REPO_NAME"
echo "        DO NOT initialise with README / .gitignore / license."
echo
echo "  2. Push the local repo. Two ways:"
echo
echo "     a) HTTPS (no SSH key needed):"
echo "        cd $DEST"
echo "        git remote add origin https://github.com/$ACCOUNT/$REPO_NAME.git"
echo "        git push -u origin main"
echo
echo "     b) SSH (if you've added an SSH key to GitHub):"
echo "        cd $DEST"
echo "        git remote add origin git@github.com:$ACCOUNT/$REPO_NAME.git"
echo "        git push -u origin main"
echo
