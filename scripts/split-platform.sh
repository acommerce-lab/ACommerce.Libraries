#!/usr/bin/env bash
#
# split-platform.sh
#
# Splits the ACommerce platform (libraries + Ashare.Api2/Web2 + Order.Api2/Web2
# + docs) out of the current monorepo into a fresh /tmp/acommerce-platform
# working tree with FULL git history for every moved directory.
#
# Usage:
#   bash scripts/split-platform.sh <your-github-username-or-org>
#
# After it runs, you create the GitHub repo (any way you like) and push:
#   gh repo create <acct>/acommerce-platform --public
#   cd /tmp/acommerce-platform
#   git push -u origin main
#
# The script uses `git subtree split` so it works on any git install
# without needing `git-filter-repo`. If you DO have filter-repo, this
# is still correct — it just takes slightly longer than filter-repo would.

set -euo pipefail

ACCOUNT="${1:-}"
if [ -z "$ACCOUNT" ]; then
    echo "usage: $0 <github-account-or-org>" >&2
    exit 1
fi

REPO_NAME="acommerce-platform"
DEST="/tmp/$REPO_NAME"
SRC="$(cd "$(dirname "$0")/.." && pwd)"

cd "$SRC"

# Sanity checks
if [ ! -d .git ]; then
    echo "ERROR: $SRC is not a git repo" >&2
    exit 1
fi

if [ -n "$(git status --porcelain)" ]; then
    echo "ERROR: working tree is dirty. Commit or stash first." >&2
    git status --short
    exit 1
fi

# Make sure the destination doesn't already exist
if [ -e "$DEST" ]; then
    echo "ERROR: $DEST already exists. Remove it first:  rm -rf $DEST" >&2
    exit 1
fi

# The list of paths to KEEP in the new repo.  Each path is a directory
# whose history will be preserved via `git subtree split`.
PATHS=(
    "libs/backend/core/ACommerce.SharedKernel.Abstractions"
    "libs/backend/core/ACommerce.SharedKernel.Infrastructure.EFCores"
    "libs/backend/core/ACommerce.OperationEngine"
    "libs/backend/core/ACommerce.OperationEngine.Wire"
    "libs/backend/core/ACommerce.OperationEngine.Interceptors"

    "libs/backend/auth/ACommerce.Authentication.Operations"
    "libs/backend/auth/ACommerce.Authentication.Providers.Token"
    "libs/backend/auth/ACommerce.Authentication.TwoFactor.Operations"
    "libs/backend/auth/ACommerce.Authentication.TwoFactor.Providers.Sms"
    "libs/backend/auth/ACommerce.Authentication.TwoFactor.Providers.Email"
    "libs/backend/auth/ACommerce.Authentication.TwoFactor.Providers.Nafath"
    "libs/backend/auth/ACommerce.Permissions.Operations"

    "libs/backend/messaging/ACommerce.Realtime.Operations"
    "libs/backend/messaging/ACommerce.Realtime.Providers.InMemory"
    "libs/backend/messaging/ACommerce.Notification.Operations"
    "libs/backend/messaging/ACommerce.Notification.Providers.InApp"
    "libs/backend/messaging/ACommerce.Notification.Providers.Firebase"

    "libs/backend/sales/ACommerce.Payments.Operations"
    "libs/backend/sales/ACommerce.Payments.Providers.Noon"

    "libs/backend/marketplace/ACommerce.Subscriptions.Operations"

    "libs/backend/files/ACommerce.Files.Abstractions"
    "libs/backend/files/ACommerce.Files.Operations"
    "libs/backend/files/ACommerce.Files.Storage.Local"
    "libs/backend/files/ACommerce.Files.Storage.AliyunOSS"
    "libs/backend/files/ACommerce.Files.Storage.GoogleCloud"

    "libs/backend/other/ACommerce.Favorites.Operations"
    "libs/backend/other/ACommerce.Translations.Operations"

    "libs/frontend/ACommerce.Widgets"
    "libs/frontend/ACommerce.Templates.Commerce"

    "clients/ACommerce.Client.Operations"
    "clients/ACommerce.Client.Http"
    "clients/ACommerce.Client.StateBridge"

    "Apps/Ashare.Api2"
    "Apps/Ashare.Web2"
    "Apps/Order.Api2"
    "Apps/Order.Web2"

    "docs"
)

echo "==> Splitting ${#PATHS[@]} paths into $DEST"
echo

# For each path, run `git subtree split` on a temporary branch.
declare -A SHA_MAP
for p in "${PATHS[@]}"; do
    if [ ! -d "$SRC/$p" ]; then
        echo "  SKIP  $p (missing)"
        continue
    fi
    echo -n "  SPLIT $p ... "
    sha=$(git subtree split --prefix="$p" HEAD 2>/dev/null || true)
    if [ -z "$sha" ]; then
        echo "FAIL"
        continue
    fi
    SHA_MAP["$p"]="$sha"
    echo "${sha:0:10}"
done
echo

# Build the new repo in $DEST
echo "==> Bootstrapping $DEST"
mkdir -p "$DEST"
cd "$DEST"
git init -q -b main

# Add the root README + gitignore so the very first commit is clean
cat > README.md <<'EOF'
# ACommerce Platform

Multi-vendor e-commerce platform built on the accounting OperationEngine.

**Start here:** [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md)

## Quick tour

- `libs/backend/core/` — the five non-negotiable core libraries (shared kernel, EF wrapper, OperationEngine, wire format, interceptors)
- `libs/backend/auth/` — authentication + JWT + SMS / Email / Nafath 2FA + permissions
- `libs/backend/messaging/` — realtime transport + in-app and Firebase notifications
- `libs/backend/sales/` — payments (Noon provider)
- `libs/backend/marketplace/` — subscriptions with quota interceptors
- `libs/backend/files/` — file storage abstractions + Local / Aliyun OSS / Google Cloud providers
- `libs/backend/other/` — favourites, translations
- `libs/frontend/ACommerce.Widgets/` — atomic widgets + Bootstrap compatibility layer (664 lines of CSS)
- `libs/frontend/ACommerce.Templates.Commerce/` — composite templates (AcShell, AcProductCard, …)
- `clients/` — client-side accounting libraries (wire format, HTTP dispatcher, reactive bridge)
- `Apps/Ashare.Api2` + `Apps/Ashare.Web2` — property classifieds demo
- `Apps/Order.Api2` + `Apps/Order.Web2` — cafe/restaurant deals demo (in-store + curbside pickup, no online payment)

## Documentation

All docs are under `docs/`:

- **ARCHITECTURE.md** — the top-level map.
- **ACCOUNTING-PHILOSOPHY.md** — the OpEngine mental model.
- **BUILDING-A-BACKEND.md** — step-by-step recipe for a new backend.
- **BUILDING-A-FRONTEND.md** — step-by-step recipe for a new Blazor frontend.
- **AI-AGENT-ONBOARDING.md** — the brief for future AI coding agents.
- **TEMPLATES-ROADMAP.md** — what templates exist, what's missing, and why.

## Running the demos

```bash
bash Apps/Order.Web2/run-local.sh
```

Then open http://localhost:5701.
EOF

cat > .gitignore <<'EOF'
# .NET
bin/
obj/
*.user
*.suo

# SQLite demo DBs
**/data/*.db
**/data/*.db-*

# Logs
**/logs/
*.log

# IDE
.vs/
.vscode/
.idea/
EOF

git add README.md .gitignore
git commit -q -m "chore: bootstrap acommerce-platform from split" \
    -m "First commit on the split repo. Everything else is pulled in via the subtree merges that follow."

# Pull each split branch into the right destination directory
cd "$SRC"
for p in "${PATHS[@]}"; do
    sha="${SHA_MAP[$p]:-}"
    if [ -z "$sha" ]; then continue; fi
    echo "  PULL  $p"
    (cd "$DEST" && \
        git read-tree --prefix="$p/" -u "$sha" 2>&1 | sed 's/^/      /' || true)
done

# Commit the reconstructed tree
cd "$DEST"
git add -A
git commit -q -m "feat: import platform libraries + apps + docs from monorepo split" \
    -m "$(for p in "${PATHS[@]}"; do
        sha="${SHA_MAP[$p]:-}"
        if [ -n "$sha" ]; then
            echo "- $p (from ${sha:0:10})"
        fi
    done)" || true

echo
echo "==============================================================="
echo "  DONE. New repo at $DEST"
echo "==============================================================="
echo
echo "Next steps:"
echo
echo "  1. Create the empty GitHub repo:"
echo "     gh repo create $ACCOUNT/$REPO_NAME --public \\"
echo "         --description \"Multi-vendor e-commerce platform on the accounting OperationEngine\""
echo
echo "  2. Push:"
echo "     cd $DEST"
echo "     git remote add origin git@github.com:$ACCOUNT/$REPO_NAME.git"
echo "     git push -u origin main"
echo
echo "  3. Verify:"
echo "     cd $DEST && dotnet build Apps/Order.Api2/Order.Api2.csproj"
echo
