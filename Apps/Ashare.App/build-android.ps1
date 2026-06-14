# Build Android — switches to .NET 9 SDK by swapping global.json temporarily.
# Mac uses .NET 8 (kept in global.json for iOS), Windows uses .NET 9 here.
#
# Requirements on Windows:
#   - .NET 9 SDK installed (e.g. 9.0.300 or higher 9.0.x)
#   - maui-android workload installed for .NET 9:
#       dotnet workload install maui-android
#
# Run from anywhere; the script cds to the repo root by itself.

$ErrorActionPreference = "Stop"

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

    Write-Host "→ Publish for net9.0-android"
    dotnet publish Apps/Ashare.App/Ashare.App.csproj `
        --no-restore `
        -c Release `
        -f net9.0-android `
        -p:MobilePlatform=android
    if ($LASTEXITCODE -ne 0) { throw "publish failed" }

    $apk = Get-ChildItem -Path "Apps\Ashare.App\bin\Release" -Recurse -Filter "*-Signed.apk" `
        | Select-Object -First 1
    if ($apk) {
        Write-Host "`n✅ Signed APK: $($apk.FullName)"
        Copy-Item $apk.FullName "$env:USERPROFILE\Desktop\"
        Write-Host "✅ Copied to Desktop"
    } else {
        $aab = Get-ChildItem -Path "Apps\Ashare.App\bin\Release" -Recurse -Filter "*.aab" `
            | Select-Object -First 1
        if ($aab) {
            Write-Host "`n✅ AAB: $($aab.FullName)"
            Copy-Item $aab.FullName "$env:USERPROFILE\Desktop\"
            Write-Host "✅ Copied to Desktop"
        } else {
            Write-Warning "Build finished but no APK / AAB found under bin/Release."
        }
    }
}
finally {
    if (Test-Path $backup) {
        Write-Host "→ Restoring .NET 8 global.json (so iOS workflow keeps working)"
        Move-Item -Force $backup $globalJson
    }
    Pop-Location
}
