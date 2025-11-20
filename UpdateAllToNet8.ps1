# UpdateAllToNet8.ps1
# تحديث جميع المشاريع إلى .NET 8 مع أحدث packages متوافقة

param(
    [string]$SolutionPath = ".",
    [string]$TargetFramework = "net8.0"
)

$ErrorActionPreference = "Stop"

Write-Host "═══════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  Update All Projects to $TargetFramework" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════" -ForegroundColor Cyan

# Package version mapping for .NET 8
$net8Packages = @{
    "Microsoft.EntityFrameworkCore" = "8.0.22"
    "Microsoft.EntityFrameworkCore.InMemory" = "8.0.22"
    "Microsoft.EntityFrameworkCore.SqlServer" = "8.0.22"
    "Microsoft.EntityFrameworkCore.Design" = "8.0.22"
    "Microsoft.AspNetCore.Authentication.JwtBearer" = "8.0.22"
    "Microsoft.AspNetCore.OpenApi" = "8.0.22"
    "Microsoft.Extensions.DependencyInjection" = "8.0.1"
    "Microsoft.Extensions.Configuration" = "8.0.0"
    "Microsoft.Extensions.Logging" = "8.0.1"
    "Swashbuckle.AspNetCore" = "6.5.0"  # ✅ آخر إصدار يعمل مع .NET 8
    "Microsoft.OpenApi" = "1.6.22"      # ✅ متوافق مع Swashbuckle 6.5
}

# Find all .csproj files
$projects = Get-ChildItem -Path $SolutionPath -Filter "*.csproj" -Recurse

Write-Host "`nFound $($projects.Count) projects" -ForegroundColor Green
Write-Host ""

foreach ($project in $projects) {
    Write-Host "Processing: $($project.Name)" -ForegroundColor Yellow
    
    [xml]$xml = Get-Content $project.FullName
    $modified = $false
    
    # 1. Update TargetFramework
    $targetFrameworkNode = $xml.SelectSingleNode("//TargetFramework")
    if ($targetFrameworkNode -and $targetFrameworkNode.InnerText -ne $TargetFramework) {
        Write-Host "  • Updating TargetFramework: $($targetFrameworkNode.InnerText) → $TargetFramework" -ForegroundColor Cyan
        $targetFrameworkNode.InnerText = $TargetFramework
        $modified = $true
    }
    
    # 2. Add properties to disable generators
    $propertyGroup = $xml.SelectSingleNode("//PropertyGroup")
    if ($propertyGroup) {
        $propsToAdd = @{
            "EnableOpenApiAnalyzers" = "false"
            "EnableOpenApiGenerator" = "false"
            "GenerateDocumentationFile" = "false"
        }
        
        foreach ($prop in $propsToAdd.GetEnumerator()) {
            $node = $xml.SelectSingleNode("//$($prop.Key)")
            if (-not $node) {
                Write-Host "  • Adding property: $($prop.Key) = $($prop.Value)" -ForegroundColor Cyan
                $elem = $xml.CreateElement($prop.Key)
                $elem.InnerText = $prop.Value
                $propertyGroup.AppendChild($elem) | Out-Null
                $modified = $true
            }
        }
    }
    
    # 3. Update PackageReferences
    $packageNodes = $xml.SelectNodes("//PackageReference")
    foreach ($package in $packageNodes) {
        $packageName = $package.GetAttribute("Include")
        $currentVersion = $package.GetAttribute("Version")
        
        if ($net8Packages.ContainsKey($packageName)) {
            $recommendedVersion = $net8Packages[$packageName]
            if ($currentVersion -ne $recommendedVersion) {
                Write-Host "  • Updating package: $packageName" -ForegroundColor Cyan
                Write-Host "    $currentVersion → $recommendedVersion" -ForegroundColor Gray
                $package.SetAttribute("Version", $recommendedVersion)
                $modified = $true
            }
        }
    }
    
    # Save if modified
    if ($modified) {
        $xml.Save($project.FullName)
        Write-Host "  ✓ Saved changes" -ForegroundColor Green
    } else {
        Write-Host "  • No changes needed" -ForegroundColor Gray
    }
    
    Write-Host ""
}

Write-Host "═══════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  Cleaning and Restoring" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════" -ForegroundColor Cyan

Push-Location $SolutionPath

# Clean
Write-Host "`n[1/4] Cleaning..." -ForegroundColor Yellow
dotnet clean | Out-Null

# Delete obj/bin
Write-Host "`n[2/4] Deleting obj and bin folders..." -ForegroundColor Yellow
Get-ChildItem -Path . -Recurse -Directory -Force | 
    Where-Object { $_.Name -eq 'obj' -or $_.Name -eq 'bin' } | 
    Remove-Item -Recurse -Force
Write-Host "  ✓ Deleted" -ForegroundColor Green

# Clear cache
Write-Host "`n[3/4] Clearing NuGet cache..." -ForegroundColor Yellow
dotnet nuget locals all --clear | Out-Null
Write-Host "  ✓ Cleared" -ForegroundColor Green

# Restore
Write-Host "`n[4/4] Restoring packages..." -ForegroundColor Yellow
dotnet restore --force
if ($LASTEXITCODE -eq 0) {
    Write-Host "  ✓ Restored successfully" -ForegroundColor Green
} else {
    Write-Host "  ✗ Restore failed!" -ForegroundColor Red
    Pop-Location
    exit 1
}

Pop-Location

Write-Host "`n✓✓✓ All projects updated to $TargetFramework! ✓✓✓" -ForegroundColor Green
Write-Host "`nNext step: dotnet build --configuration Release" -ForegroundColor Cyan