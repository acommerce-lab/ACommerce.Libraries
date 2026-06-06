#!/bin/bash
set -e
cd "$(dirname "$0")/../.."

# 1) تطبيق patch لـ bitcode_strip إن لم يكن مطبقاً
TARGETS="$HOME/.nuget/packages/xamarin.firebase.ios.core/8.10.0.3/buildTransitive/Xamarin.Firebase.iOS.Core.targets"
if [ -f "$TARGETS" ] && ! grep -q "Skipped bitcode_strip" "$TARGETS"; then
    sed -i.bak 's|<Exec Command="$(_BitcodeStripCommand) %|<Exec Command="echo Skipped bitcode_strip on %|g' "$TARGETS"
    echo "✅ Patched Firebase targets to skip bitcode_strip"
fi

# 2) تأكّد strip في PATH
export PATH="$(dirname $(xcrun --find strip)):$PATH"

# 3) نظّف + ابنِ
rm -rf Apps/Ashare.App/bin/Release Apps/Ashare.App/obj/Release
dotnet publish Apps/Ashare.App/Ashare.App.csproj -c Release -f net8.0-ios -p:RuntimeIdentifier=ios-arm64 -p:ArchiveOnBuild=true

# 4) أظهر مسار الـ IPA
echo
echo "✅ IPA created at:"
find Apps/Ashare.App/bin/Release -name "*.ipa"