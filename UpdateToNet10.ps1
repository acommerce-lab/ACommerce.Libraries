# UpdateToNet10.ps1
# تحديث جميع مشاريع ACommerce إلى .NET 10

$solutionPath = "C:\Users\i\Source\Repos\ACommerce.Libraries"  # غيّر حسب مسارك
$newTargetFramework = "net8.0"
$newVersion = "1.1.0"  # رقم الإصدار الجديد

# البحث عن جميع ملفات .csproj في المكتبات
$projectFiles = Get-ChildItem -Path $solutionPath -Filter "*.csproj" -Recurse | 
    Where-Object { $_.FullName -like "*ACommerce*" }

Write-Host "Found $($projectFiles.Count) projects to update" -ForegroundColor Green

foreach ($projectFile in $projectFiles) {
    Write-Host "`nUpdating: $($projectFile.Name)" -ForegroundColor Yellow
    
    $content = Get-Content $projectFile.FullName -Raw
    
    # تحديث TargetFramework
    $content = $content -replace '<TargetFramework>net\d+\.\d+</TargetFramework>', 
                                  "<TargetFramework>$newTargetFramework</TargetFramework>"
    
    # تحديث Version
    $content = $content -replace '<Version>[\d\.]+</Version>', 
                                  "<Version>$newVersion</Version>"
    
    # حفظ التغييرات
    Set-Content -Path $projectFile.FullName -Value $content -NoNewline
    
    Write-Host "  ✓ Updated to $newTargetFramework and version $newVersion" -ForegroundColor Green
}

Write-Host "`n✓ All projects updated successfully!" -ForegroundColor Green
Write-Host "`nNext steps:" -ForegroundColor Cyan
Write-Host "1. Run: dotnet restore" -ForegroundColor White
Write-Host "2. Run: dotnet build" -ForegroundColor White
Write-Host "3. Fix any compilation errors" -ForegroundColor White
Write-Host "4. Run: git add ." -ForegroundColor White
Write-Host "5. Run: git commit -m 'Update to .NET 10 v$newVersion'" -ForegroundColor White
Write-Host "6. Run: git push" -ForegroundColor White