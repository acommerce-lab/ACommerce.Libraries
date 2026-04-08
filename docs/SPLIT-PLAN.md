# Repository split plan

The user asked to split this monorepo into two new standalone
repositories:

1. **`acommerce-platform`** — the production platform (libraries + two
   demo apps + all the docs).
2. **`magneticlm`** — the graph-based language model research project.

I cannot create GitHub repositories from inside the sandbox — my MCP
token is scoped to `acommerce-lab/acommerce.libraries` and any write
to another repo is rejected. Instead, this commit adds two
self-contained bash scripts (`scripts/split-platform.sh` and
`scripts/split-magneticlm.sh`) that do the whole split with full git
history preserved. You run them on your own machine with your own
credentials and the two new repos appear in seconds.

This document explains what goes where, how the scripts work, and
the exact commands you run.

---

## What goes to `acommerce-platform`

### Keep (the new repo's content)

```
libs/backend/core/ACommerce.SharedKernel.Abstractions/
libs/backend/core/ACommerce.SharedKernel.Infrastructure.EFCores/
libs/backend/core/ACommerce.OperationEngine/
libs/backend/core/ACommerce.OperationEngine.Wire/
libs/backend/core/ACommerce.OperationEngine.Interceptors/

libs/backend/auth/ACommerce.Authentication.Operations/
libs/backend/auth/ACommerce.Authentication.Providers.Token/
libs/backend/auth/ACommerce.Authentication.TwoFactor.Operations/
libs/backend/auth/ACommerce.Authentication.TwoFactor.Providers.Sms/
libs/backend/auth/ACommerce.Authentication.TwoFactor.Providers.Email/
libs/backend/auth/ACommerce.Authentication.TwoFactor.Providers.Nafath/
libs/backend/auth/ACommerce.Permissions.Operations/

libs/backend/messaging/ACommerce.Realtime.Operations/
libs/backend/messaging/ACommerce.Realtime.Providers.InMemory/
libs/backend/messaging/ACommerce.Notification.Operations/
libs/backend/messaging/ACommerce.Notification.Providers.InApp/
libs/backend/messaging/ACommerce.Notification.Providers.Firebase/

libs/backend/sales/ACommerce.Payments.Operations/
libs/backend/sales/ACommerce.Payments.Providers.Noon/

libs/backend/marketplace/ACommerce.Subscriptions.Operations/

libs/backend/files/ACommerce.Files.Abstractions/
libs/backend/files/ACommerce.Files.Operations/
libs/backend/files/ACommerce.Files.Storage.Local/
libs/backend/files/ACommerce.Files.Storage.AliyunOSS/
libs/backend/files/ACommerce.Files.Storage.GoogleCloud/

libs/backend/other/ACommerce.Favorites.Operations/
libs/backend/other/ACommerce.Translations.Operations/

libs/frontend/ACommerce.Widgets/
libs/frontend/ACommerce.Templates.Commerce/

clients/ACommerce.Client.Operations/
clients/ACommerce.Client.Http/
clients/ACommerce.Client.StateBridge/

Apps/Ashare.Api2/
Apps/Ashare.Web2/
Apps/Order.Api2/
Apps/Order.Web2/

docs/
```

Plus a new root-level `README.md` that points to `docs/ARCHITECTURE.md`.

### Drop (stays in the old monorepo)

Everything else, specifically:

- `Apps/Ashare.Api`, `Apps/Ashare.Web`, `Apps/Ashare.Admin`, `Apps/Ashare.App` — old dead apps.
- `Apps/Order.Api`, `Apps/Order.Shared`, `Apps/Order.Customer.App` — old repository-pattern Order (MAUI-flavoured, superseded by Order.Api2 / Order.Web2).
- `Apps/HamtramckHardware.*`, `Apps/ACommerce.*`, `Apps/HamtramckHardware.Web`, etc. — unrelated apps.
- `Examples/ACommerce.MagneticLM/` — goes to its own repo.
- `Templates/` — the old "Customer template" based on the repository pattern; superseded by `libs/frontend/ACommerce.Templates.Commerce/`.
- `Other/`, `attached_assets/` — snapshots and scratch.

The list of things to drop is long but the script handles it
automatically — `git subtree split` only moves the paths you
explicitly name.

---

## What goes to `magneticlm`

```
Examples/ACommerce.MagneticLM/colab/        -> MagneticLM research code
Examples/ACommerce.MagneticLM/RESEARCH-NOTES.md
```

The old C# version (`Examples/ACommerce.MagneticLM/*.cs`) is kept
because the current perplexity numbers are measured against it as the
conceptual reference. Plus its source is small (~1200 lines) and
self-contained.

A new root-level `README.md` points to `RESEARCH-NOTES.md` as the
primary entry point for picking up the research.

---

## The scripts

Both scripts follow the same shape:

1. `git subtree split --prefix=<path> -b <branch>` for each path to
   keep. This creates a new local branch whose history is just that
   path's history.
2. Merge all those branches into a single temporary branch via
   `git subtree merge` with a rewritten prefix.
3. Create a fresh git repo in `/tmp/<name>`, pull the temporary branch
   into it, and push to `git@github.com:<your-account>/<name>.git`.

### The catch — subtree split is one-prefix-at-a-time

`git subtree split` can only process **one** directory at a time.
For the platform we have 30+ directories to keep. The script does
them all sequentially and merges them via a scratch repo.

An alternative is `git filter-repo` (a Python tool that's faster and
more flexible). If you have it installed (`pip install git-filter-repo`),
the script will use it instead; otherwise it falls back to
`git subtree split`.

### The commands you run

```bash
# Ensure your working tree is clean
git status
# (on branch claude/local-dotnet-build-testing-b5DgA, clean)

# 1. Split the platform
bash scripts/split-platform.sh my-github-username
# -> Creates /tmp/acommerce-platform/ with full history for every
#    kept directory. Prints the next steps for creating the repo on
#    GitHub and pushing.

# 2. Split MagneticLM
bash scripts/split-magneticlm.sh my-github-username
# -> Same, for /tmp/magneticlm/.

# 3. Create the empty GitHub repos (the script can't do this because
#    it doesn't have your token, but the command is in the script's
#    final output):
gh repo create my-github-username/acommerce-platform --public --description "Multi-vendor e-commerce platform on the accounting OperationEngine"
gh repo create my-github-username/magneticlm --public --description "Graph-based LM — 14.20 PPL on WikiText-103"

# 4. Push
cd /tmp/acommerce-platform && git push -u origin main
cd /tmp/magneticlm && git push -u origin main
```

After that, the original repo is untouched. You now have two new
repos, each with the relevant history, ready for independent
development.

---

## FAQ

**Q: Why not just create the repos from inside this session?**  
A: My GitHub token is scoped to `acommerce-lab/acommerce.libraries`
and any attempt to create or write to another repository is rejected
by the MCP server. The split has to happen on your machine with your
token.

**Q: Will the new repos have full git history?**  
A: Yes. Both `git subtree split` and `git filter-repo` preserve the
commit history of the kept paths. You'll see every commit that
touched `Apps/Order.Api2` in the platform repo's history, with the
same hashes (or rewritten hashes if you use filter-repo).

**Q: What about the old monorepo after the split?**  
A: It stays exactly as it is. The split doesn't delete anything from
the source. If you later want to clean it up, that's a separate
operation (which I can also help with).

**Q: What about external contributors who had PRs against the
monorepo?**  
A: Any open PR against the old repo stays there. New PRs should be
directed to the new repos after the split.

**Q: Do the new repos need CI?**  
A: Yes, but that's a follow-up. Each new repo should have a simple
GitHub Actions workflow that builds on push. Samples will be in the
scripts' final-output checklist.
