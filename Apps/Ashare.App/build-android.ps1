# Build Android — switches to .NET 9 SDK by swapping global.json temporarily.
# Mac uses .NET 8 (kept in global.json for iOS), Windows uses .NET 9 here.
#
# Requirements on Windows:
#   - .NET 9 SDK installed (e.g. 9.0.300 or higher 9.0.x)
#   - maui-android workload installed for .NET 9:
#       dotnet workload install maui-android
#   - For a Play-ready signed build: Apps/Ashare.App/android-signing.props
#     filled in (copy from android-signing.props.example). Without it the
#     build still works but is debug-signed (local testing only).
#
# Usage:
#   .\Apps\Ashare.App\build-android.ps1          # AAB for Google Play (default)
#   .\Apps\Ashare.App\build-android.ps1 -Apk     # APK for sideload testing
#
# Run from anywhere; the script cds to the repo root by itself.

param(
    [switch]$Apk
)

$ErrorActionPreference = "Stop"

$packageFormat = if ($Apk) { "apk" } else { "aab" }

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..\..")
Push-Location $repoRoot

try {
    $globalJson = Join-Path $repoRoot "global.json"
    $backup     = "$globalJson.ios-bak"

    if (Test-Path $globalJson) {
        Write-Host "→ Stashing .NET 8 global.json (for iOS) → $backup"
        Move-Item -Force $globalJson $backup
    }

    # Pick the highest 9.0.x SDK actually installed (so we don't roll past 9 into 10).
    $sdkDir = "C:\Program Files\dotnet\sdk"
    $net9 = Get-ChildItem $sdkDir -Directory `
        | Where-Object { $_.Name -match '^9\.0\.\d+$' } `
        | Sort-Object Name -Descending `
        | Select-Object -First 1
    if (-not $net9) {
        throw "No .NET 9 SDK found under $sdkDir. Install 9.0.x from https://dotnet.microsoft.com/download/dotnet/9.0"
    }
    $net9Version = $net9.Name

    Write-Host "→ Writing global.json pinned to $net9Version (rollForward=latestFeature, stays in 9.0.x)"
    @"
{
  "sdk": {
    "version": "$net9Version",
    "rollForward": "latestFeature",
    "allowPrerelease": false
  }
}
"@ | Out-File -Encoding utf8 -NoNewline $globalJson

    Write-Host "→ Active SDK:"
    dotnet --version

    # Clear obj/ so the stale net8.0-ios project.assets.json is rebuilt for net9.0-android.
    $objDir = Join-Path $repoRoot "Apps\Ashare.App\obj"
    if (Test-Path $objDir) {
        Write-Host "→ Cleaning Apps\Ashare.App\obj (stale assets file)"
        Remove-Item -Recurse -Force $objDir
    }
    $binDir = Join-Path $repoRoot "Apps\Ashare.App\bin\Release"
    if (Test-Path $binDir) {
        Write-Host "→ Cleaning Apps\Ashare.App\bin\Release"
        Remove-Item -Recurse -Force $binDir
    }

    Write-Host "→ Restore (explicit, with MobilePlatform=android so TargetFrameworks resolves to net9.0-android)"
    dotnet restore Apps/Ashare.App/Ashare.App.csproj `
        -p:MobilePlatform=android `
        --disable-parallel
    if ($LASTEXITCODE -ne 0) { throw "restore failed" }

    $signing = Join-Path $repoRoot "Apps\Ashare.App\android-signing.props"
    if (Test-Path $signing) {
        Write-Host "→ Signing: android-signing.props found (release upload key)"
    } else {
        Write-Warning "android-signing.props NOT found — build will be debug-signed (Play will reject it). Copy android-signing.props.example and fill it in for a Play-ready build."
    }

    Write-Host "→ Publish for net9.0-android (format: $packageFormat)"
    dotnet publish Apps/Ashare.App/Ashare.App.csproj `
        --no-restore `
        -c Release `
        -f net9.0-android `
        -p:MobilePlatform=android `
        -p:AndroidPackageFormat=$packageFormat
    if ($LASTEXITCODE -ne 0) { throw "publish failed" }

    # Output goes to <repo>\artifacts (always exists; Desktop is unreliable under OneDrive).
    $artifacts = Join-Path $repoRoot "artifacts"
    New-Item -ItemType Directory -Force -Path $artifacts | Out-Null

    if ($packageFormat -eq "aab") {
        $artifact = Get-ChildItem -Path "Apps\Ashare.App\bin\Release" -Recurse -Filter "*.aab" `
            | Select-Object -First 1
    } else {
        $artifact = Get-ChildItem -Path "Apps\Ashare.App\bin\Release" -Recurse -Filter "*-Signed.apk" `
            | Select-Object -First 1
    }

    if ($artifact) {
        $dest = Join-Path $artifacts $artifact.Name
        Copy-Item $artifact.FullName $dest -Force
        Write-Host "`n✅ $($artifact.Name)"
        Write-Host "   Source: $($artifact.FullName)"
        Write-Host "   Copied: $dest"
        # Open Explorer on the artifacts folder with the file selected.
        Start-Process explorer.exe "/select,`"$dest`""
    } else {
        Write-Warning "Build finished but no APK / AAB found under bin/Release."
    }
}
finally {
    if (Test-Path $backup) {
        Write-Host "→ Restoring .NET 8 global.json (so iOS workflow keeps working)"
        Move-Item -Force $backup $globalJson
    }
    Pop-Location
}
