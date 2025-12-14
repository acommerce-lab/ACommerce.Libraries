#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Restructures ACommerce.Libraries.sln to match the new libs/ folder structure.
.DESCRIPTION
    This script updates the Solution file to:
    1. Create new Solution Folders matching libs/backend/* and libs/frontend/*
    2. Reassign projects to the correct folders
    3. Remove old Solution Folders
.NOTES
    Run from the repository root directory.
#>

param(
    [switch]$DryRun = $false
)

$SolutionPath = "ACommerce.Libraries.sln"
$BackupPath = "ACommerce.Libraries.sln.backup-$(Get-Date -Format 'yyyyMMdd-HHmmss')"

# Solution Folder GUID type
$SolutionFolderGuid = "2150E333-8FDC-42A3-9474-1A3956D46DE8"

# New folder structure with generated GUIDs
$NewFolders = @{
    # Root
    "libs" = @{ Guid = [guid]::NewGuid().ToString().ToUpper(); Parent = $null }
    
    # Backend
    "backend" = @{ Guid = [guid]::NewGuid().ToString().ToUpper(); Parent = "libs" }
    "core" = @{ Guid = [guid]::NewGuid().ToString().ToUpper(); Parent = "backend" }
    "auth" = @{ Guid = [guid]::NewGuid().ToString().ToUpper(); Parent = "backend" }
    "catalog" = @{ Guid = [guid]::NewGuid().ToString().ToUpper(); Parent = "backend" }
    "sales" = @{ Guid = [guid]::NewGuid().ToString().ToUpper(); Parent = "backend" }
    "marketplace" = @{ Guid = [guid]::NewGuid().ToString().ToUpper(); Parent = "backend" }
    "messaging" = @{ Guid = [guid]::NewGuid().ToString().ToUpper(); Parent = "backend" }
    "files" = @{ Guid = [guid]::NewGuid().ToString().ToUpper(); Parent = "backend" }
    "shipping" = @{ Guid = [guid]::NewGuid().ToString().ToUpper(); Parent = "backend" }
    "integration" = @{ Guid = [guid]::NewGuid().ToString().ToUpper(); Parent = "backend" }
    "other" = @{ Guid = [guid]::NewGuid().ToString().ToUpper(); Parent = "backend" }
    
    # Frontend
    "frontend" = @{ Guid = [guid]::NewGuid().ToString().ToUpper(); Parent = "libs" }
    "fe-core" = @{ Guid = [guid]::NewGuid().ToString().ToUpper(); Parent = "frontend"; DisplayName = "core" }
    "clients" = @{ Guid = [guid]::NewGuid().ToString().ToUpper(); Parent = "frontend" }
    "realtime" = @{ Guid = [guid]::NewGuid().ToString().ToUpper(); Parent = "frontend" }
    "discovery" = @{ Guid = [guid]::NewGuid().ToString().ToUpper(); Parent = "frontend" }
    
    # Apps
    "apps" = @{ Guid = [guid]::NewGuid().ToString().ToUpper(); Parent = $null }
}

# Path to folder mapping
$PathToFolder = @{
    "libs\backend\core\" = "core"
    "libs\backend\auth\" = "auth"
    "libs\backend\catalog\" = "catalog"
    "libs\backend\sales\" = "sales"
    "libs\backend\marketplace\" = "marketplace"
    "libs\backend\messaging\" = "messaging"
    "libs\backend\files\" = "files"
    "libs\backend\shipping\" = "shipping"
    "libs\backend\integration\" = "integration"
    "libs\backend\other\" = "other"
    "libs\frontend\core\" = "fe-core"
    "libs\frontend\clients\" = "clients"
    "libs\frontend\realtime\" = "realtime"
    "libs\frontend\discovery\" = "discovery"
    "Apps\" = "apps"
}

Write-Host "=== ACommerce Solution Restructurer ===" -ForegroundColor Cyan
Write-Host ""

if (-not (Test-Path $SolutionPath)) {
    Write-Error "Solution file not found: $SolutionPath"
    exit 1
}

# Backup
if (-not $DryRun) {
    Write-Host "Creating backup: $BackupPath" -ForegroundColor Yellow
    Copy-Item $SolutionPath $BackupPath
}

# Read solution
$content = Get-Content $SolutionPath -Raw -Encoding UTF8

Write-Host "Analyzing solution structure..." -ForegroundColor Green

# Find all projects and their paths
$projectPattern = 'Project\("\{([^}]+)\}"\)\s*=\s*"([^"]+)",\s*"([^"]+)",\s*"\{([^}]+)\}"'
$projects = [regex]::Matches($content, $projectPattern)

Write-Host "Found $($projects.Count) projects" -ForegroundColor Green

# Categorize projects by their path
$projectsByFolder = @{}
foreach ($proj in $projects) {
    $typeGuid = $proj.Groups[1].Value
    $name = $proj.Groups[2].Value
    $path = $proj.Groups[3].Value
    $projGuid = $proj.Groups[4].Value
    
    # Skip solution folders
    if ($typeGuid -eq $SolutionFolderGuid) {
        continue
    }
    
    $folder = $null
    foreach ($pathPrefix in $PathToFolder.Keys) {
        if ($path.StartsWith($pathPrefix)) {
            $folder = $PathToFolder[$pathPrefix]
            break
        }
    }
    
    if ($folder) {
        if (-not $projectsByFolder.ContainsKey($folder)) {
            $projectsByFolder[$folder] = @()
        }
        $projectsByFolder[$folder] += @{
            Name = $name
            Guid = $projGuid
            Path = $path
        }
    }
}

Write-Host ""
Write-Host "Projects by folder:" -ForegroundColor Cyan
foreach ($folder in $projectsByFolder.Keys | Sort-Object) {
    Write-Host "  $folder`: $($projectsByFolder[$folder].Count) projects" -ForegroundColor White
}

if ($DryRun) {
    Write-Host ""
    Write-Host "=== DRY RUN - No changes made ===" -ForegroundColor Yellow
    Write-Host "Run without -DryRun to apply changes"
    exit 0
}

Write-Host ""
Write-Host "To apply these changes, open the solution in Visual Studio and:" -ForegroundColor Yellow
Write-Host "1. Right-click Solution -> Add -> New Solution Folder" -ForegroundColor White
Write-Host "2. Create folders: libs, libs/backend, libs/frontend" -ForegroundColor White
Write-Host "3. Create subfolders under backend: core, auth, catalog, sales, marketplace, messaging, files, shipping, integration, other" -ForegroundColor White
Write-Host "4. Create subfolders under frontend: core, clients, realtime, discovery" -ForegroundColor White
Write-Host "5. Drag projects from old folders to new folders" -ForegroundColor White
Write-Host "6. Delete empty old folders" -ForegroundColor White
Write-Host ""
Write-Host "Alternative: Use 'slngen' tool from Microsoft for automatic reorganization" -ForegroundColor Cyan

Write-Host ""
Write-Host "Done! Backup saved to: $BackupPath" -ForegroundColor Green
