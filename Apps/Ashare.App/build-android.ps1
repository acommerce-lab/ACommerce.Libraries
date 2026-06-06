# Build Android — switches to .NET 9 SDK by swapping global.json temporarily.
# Mac uses .NET 8 (kept in global.json for iOS), Windows uses .NET 9 here.
#
# Requirements on Windows:
#   - .NET 9 SDK installed (e.g. 9.0.300)
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

    Write-Host "→ Writing .NET 9 global.json"
    @"
{
  "sdk": {
    "version": "9.0.0",
    "rollForward": "latestMajor"
  }
}
"@ | Out-File -Encoding utf8 -NoNewline $globalJson

    Write-Host "→ Active SDK:"
    dotnet --version

    Write-Host "→ dotnet publish for Android (net9.0-android)"
    dotnet publish Apps/Ashare.App/Ashare.App.csproj `
        -c Release `
        -f net9.0-android `
        -p:MobilePlatform=android

    $apk = Get-ChildItem -Path Apps/Ashare.App/bin/Release -Recurse -Filter "*-Signed.apk" | Select-Object -First 1
    if ($apk) {
        Write-Host "`n✅ Signed APK: $($apk.FullName)"
        Copy-Item $apk.FullName "$env:USERPROFILE\Desktop\"
        Write-Host "✅ Copied to Desktop"
    } else {
        $aab = Get-ChildItem -Path Apps/Ashare.App/bin/Release -Recurse -Filter "*.aab" | Select-Object -First 1
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
