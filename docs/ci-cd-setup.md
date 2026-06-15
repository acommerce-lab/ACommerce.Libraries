# CI/CD Setup (Ashare)

Three GitHub Actions pipelines, all **manually triggered** (Actions tab → pick
the workflow → **Run workflow**). Nothing ships on its own — you choose the
version and track each run.

| Pipeline | File | What it does |
|---|---|---|
| Android Release | `.github/workflows/android-release.yml` | Build signed **AAB** → upload to Google Play (internal track by default) |
| Backend Deploy | `.github/workflows/backend-deploy.yml` | Build Docker image → stream to Alibaba ECS over SSH → restart + health check |
| iOS Release | `.github/workflows/ios-release.yml` | Build signed **IPA** → upload to App Store Connect / TestFlight |

> **Why CI helps you specifically:** builds run in GitHub's cloud, independent
> of your network in Yemen and of whether the Intel Mac is online.

Do them in this order: **Android first** (most reliable), then **Backend**,
then **iOS** (treat its first run as validation).

Set secrets at: **GitHub repo → Settings → Secrets and variables → Actions → New repository secret**.

---

## 1) Android — secrets

| Secret | What it is |
|---|---|
| `ANDROID_KEYSTORE_BASE64` | Your **upload** keystore (`.keystore`/`.jks`), base64-encoded |
| `ANDROID_KEYSTORE_PASSWORD` | Store password |
| `ANDROID_KEY_ALIAS` | Key alias inside the keystore |
| `ANDROID_KEY_PASSWORD` | Key password (often same as store) |
| `ANDROID_GOOGLE_SERVICES_JSON_BASE64` | `google-services.json`, base64-encoded (needed for FCM) |
| `PLAY_SERVICE_ACCOUNT_JSON` | Service-account JSON key (raw JSON, paste as-is) |

> The keystore **must be the same upload key** you already used on Play, or Play
> rejects the upload ("signed with the wrong key").

**Encode a file to base64:**

```powershell
# Windows PowerShell
[Convert]::ToBase64String([IO.File]::ReadAllBytes("C:\keys\ashare-upload.keystore")) | Set-Clipboard
[Convert]::ToBase64String([IO.File]::ReadAllBytes("C:\path\google-services.json"))   | Set-Clipboard
```
```bash
# macOS / Linux
base64 -w0 ashare-upload.keystore | pbcopy      # macOS: use | pbcopy ; Linux: | xclip -selection clipboard
```

### Create the Play service account (this is the modern replacement for the old
### "API access / Client ID + Secret" that Visual Studio asked for)

1. **Play Console** → switch to **account level** (not a single app) → find
   **API access** in the left menu.
2. **Link a Google Cloud project** (create one if you don't have it).
3. **Create new service account** → this jumps to Google Cloud Console → create
   the service account (no roles needed at the GCP step) → Done.
4. Back in Play Console **API access** → the service account now appears →
   **Grant access** / **Manage permissions** → give it at least:
   - *Releases* → **Release to testing tracks** (and **Manage testing track releases**).
   - Add **production** release permission only when you want CI to push to production.
5. In **Google Cloud Console** → that service account → **Keys** → **Add key** →
   **JSON** → download. Paste the file's **entire contents** into the
   `PLAY_SERVICE_ACCOUNT_JSON` secret.

### Run it
Actions → **Android Release** → Run workflow → enter `version_code` (must be
**> 233**), pick track (`internal` to start). The signed AAB is also saved as a
downloadable artifact.

---

## 2) Backend — secrets

| Secret | Value |
|---|---|
| `ALIBABA_SSH_HOST` | ECS public IP (e.g. `8.213.82.216`) |
| `ALIBABA_SSH_USER` | `ecs-user` |
| `ALIBABA_SSH_KEY` | Contents of the private key (`~/.ssh/ashare-key`), full file incl. BEGIN/END lines |

The DB connection string and OSS keys are **not** in CI — they remain in
`/home/ecs-user/ashare.env` on the server and are read at container runtime.

### Run it
Actions → **Backend Deploy** → Run workflow. To auto-deploy on push, uncomment
the `push:` block in `backend-deploy.yml` and set your default branch.

---

## 3) iOS — secrets

| Secret | What it is |
|---|---|
| `APPLE_DIST_CERT_P12_BASE64` | Apple **Distribution** cert + private key exported as `.p12`, base64 |
| `APPLE_DIST_CERT_PASSWORD` | Password you set when exporting the `.p12` |
| `APPLE_PROVISIONING_PROFILE_BASE64` | The `.mobileprovision` (named **"With Associated Domains"**), base64 |
| `ASC_API_KEY_ID` | App Store Connect API key ID |
| `ASC_API_ISSUER_ID` | App Store Connect API issuer ID |
| `ASC_API_KEY_P8_BASE64` | The `AuthKey_XXXX.p8`, base64 |

**Export the certificate (.p12):** on the Mac, Keychain Access → find
`Apple Distribution: SAFQAT ASHEER DIGITAL BORKERAGE CO (CTX2C777UU)` → right-click
the cert **and** its private key → Export 2 items → `.p12` → set a password.

**Provisioning profile:** download the distribution profile named
*With Associated Domains* from the Apple Developer portal (or copy it from
`~/Library/MobileDevice/Provisioning Profiles/`).

**App Store Connect API key:** App Store Connect → Users and Access → Integrations
→ **App Store Connect API** → generate a key (role: App Manager) → download the
`.p8` (one-time download) and note the Key ID + Issuer ID.

> The runner must have **Xcode 26.3** for the .NET 8 iOS workload. If the first
> run warns that Xcode_26.3 wasn't found, adjust the "Select Xcode" step to an
> installed version (the run log lists what's available) and re-run.

### Run it
Actions → **iOS Release** → Run workflow → enter `build_number` (must be
**> 233**). The build also lands as a downloadable IPA artifact.

---

## Versioning rule (both stores)
Every upload needs a **higher** version code than the last accepted one. Baseline
is **233**, so the next is **234+**. You enter it as the workflow input; the
pipeline overrides `ApplicationVersion` for that build (the value committed in the
csproj stays as a baseline).
