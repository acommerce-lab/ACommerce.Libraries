# IncrementVersion.ps1
# زيادة رقم الإصدار لجميع مكتبات ACommerce

param(
    [string]$RootPath = ".",
    [ValidateSet("Major", "Minor", "Patch")]
    [string]$IncrementType = "Patch"  # Default: 0.0.1
)

$ErrorActionPreference = "Stop"

Write-Host "═══════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  Incrementing Package Versions" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

# Find all .csproj files
$projectFiles = Get-ChildItem -Path $RootPath -Filter "*.csproj" -Recurse | 
    Where-Object { $_.FullName -like "*ACommerce*" -or $_.FullName -like "*Ashare*" }

Write-Host "Found $($projectFiles.Count) projects" -ForegroundColor Green
Write-Host ""

$updatedCount = 0

foreach ($projectFile in $projectFiles) {
    try {
        Write-Host "Processing: $($projectFile.Name)" -ForegroundColor Yellow
        
        [xml]$xml = Get-Content $projectFile.FullName
        
        # Find Version node
        $versionNode = $xml.SelectSingleNode("//Version")
        
        if ($null -eq $versionNode) {
            Write-Host "  ⊘ No <Version> tag found, skipping..." -ForegroundColor Gray
            Write-Host ""
            continue
        }
        
        $currentVersion = $versionNode.InnerText
        Write-Host "  Current version: $currentVersion" -ForegroundColor White
        
        # Parse version (supports X.Y.Z and X.Y.Z-suffix)
        if ($currentVersion -match '^(\d+)\.(\d+)\.(\d+)(.*)$') {
            $major = [int]$matches[1]
            $minor = [int]$matches[2]
            $patch = [int]$matches[3]
            $suffix = $matches[4]  # e.g., -alpha, -beta, etc.
            
            # Increment based on type
            switch ($IncrementType) {
                "Major" {
                    $major++
                    $minor = 0
                    $patch = 0
                }
                "Minor" {
                    $minor++
                    $patch = 0
                }
                "Patch" {
                    $patch++
                }
            }
            
            $newVersion = "$major.$minor.$patch$suffix"
            
            # Update XML
            $versionNode.InnerText = $newVersion
            $xml.Save($projectFile.FullName)
            
            Write-Host "  ✓ Updated to: $newVersion" -ForegroundColor Green
            $updatedCount++
        }
        else {
            Write-Host "  ✗ Invalid version format: $currentVersion" -ForegroundColor Red
        }
        
        Write-Host ""
    }
    catch {
        Write-Host "  ✗ Error: $($_.Exception.Message)" -ForegroundColor Red
        Write-Host ""
    }
}

Write-Host "═══════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  Summary" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════" -ForegroundColor Cyan
Write-Host "Total projects found: $($projectFiles.Count)" -ForegroundColor White
Write-Host "Successfully updated: $updatedCount" -ForegroundColor Green
Write-Host ""

if ($updatedCount -gt 0) {
    Write-Host "✓ Version increment complete!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Next steps:" -ForegroundColor Cyan
    Write-Host "1. dotnet build --configuration Release" -ForegroundColor White
    Write-Host "2. dotnet pack --configuration Release" -ForegroundColor White
    Write-Host "3. git add ." -ForegroundColor White
    Write-Host "4. git commit -m 'Bump version to X.Y.Z'" -ForegroundColor White
    Write-Host "5. git push" -ForegroundColor White
} else {
    Write-Host "⊘ No projects were updated" -ForegroundColor Yellow
}