# FixProjectReferences.ps1

param(
    [string]$SolutionPath = ".",
    [switch]$Verbose
)

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "  Fix Project References Script          " -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan

# 1. البحث عن جميع ملفات .csproj
$projectFiles = Get-ChildItem -Path $SolutionPath -Filter "*.csproj" -Recurse

Write-Host "`nFound $($projectFiles.Count) project files" -ForegroundColor Green

# 2. بناء خريطة للمشاريع (Project Name -> Full Path)
$projectMap = @{}

foreach ($project in $projectFiles) {
    $projectName = $project.BaseName
    $projectMap[$projectName] = $project.FullName
    
    if ($Verbose) {
        Write-Host "  Mapped: $projectName -> $($project.FullName)" -ForegroundColor Gray
    }
}

Write-Host "`nBuilt project map with $($projectMap.Count) projects" -ForegroundColor Green

# 3. إصلاح المراجع في كل مشروع
$totalFixed = 0
$totalProjects = 0

foreach ($projectFile in $projectFiles) {
    $totalProjects++
    $projectDir = $projectFile.Directory.FullName
    $projectName = $projectFile.BaseName
    
    Write-Host "`n[$totalProjects/$($projectFiles.Count)] Processing: $projectName" -ForegroundColor Yellow
    
    # قراءة محتوى المشروع
    [xml]$projectXml = Get-Content $projectFile.FullName
    
    # البحث عن جميع ProjectReference
    $references = $projectXml.Project.ItemGroup.ProjectReference
    
    if ($null -eq $references) {
        Write-Host "  No project references found" -ForegroundColor Gray
        continue
    }
    
    $fixedInThisProject = 0
    $brokenReferences = @()
    
    foreach ($reference in $references) {
        $oldInclude = $reference.Include
        
        if ([string]::IsNullOrWhiteSpace($oldInclude)) {
            continue
        }
        
        # الحصول على المسار الكامل للمرجع القديم
        $oldFullPath = Join-Path $projectDir $oldInclude
        $oldFullPath = [System.IO.Path]::GetFullPath($oldFullPath)
        
        # التحقق من وجود الملف
        if (Test-Path $oldFullPath) {
            if ($Verbose) {
                Write-Host "    ✓ Valid: $oldInclude" -ForegroundColor Gray
            }
            continue
        }
        
        # المرجع معطوب - نحاول إصلاحه
        $referencedProjectName = [System.IO.Path]::GetFileNameWithoutExtension($oldInclude)
        
        if ($projectMap.ContainsKey($referencedProjectName)) {
            # وجدنا المشروع في الخريطة!
            $correctPath = $projectMap[$referencedProjectName]
            
            # حساب المسار النسبي الجديد
            $newRelativePath = Get-RelativePath -From $projectDir -To $correctPath
            
            # تحديث المرجع
            $reference.Include = $newRelativePath
            
            Write-Host "    ✓ Fixed: $referencedProjectName" -ForegroundColor Green
            Write-Host "      Old: $oldInclude" -ForegroundColor DarkGray
            Write-Host "      New: $newRelativePath" -ForegroundColor DarkGray
            
            $fixedInThisProject++
            $totalFixed++
        }
        else {
            Write-Host "    ✗ Cannot find: $referencedProjectName" -ForegroundColor Red
            $brokenReferences += $referencedProjectName
        }
    }
    
    # حفظ التغييرات إذا تم إصلاح أي شيء
    if ($fixedInThisProject -gt 0) {
        $projectXml.Save($projectFile.FullName)
        Write-Host "  Saved: $fixedInThisProject references fixed" -ForegroundColor Cyan
    }
    
    # عرض المراجع المعطوبة
    if ($brokenReferences.Count -gt 0) {
        Write-Host "  ⚠️  Still broken references:" -ForegroundColor Yellow
        foreach ($broken in $brokenReferences) {
            Write-Host "    - $broken" -ForegroundColor Red
        }
    }
}

Write-Host "`n==========================================" -ForegroundColor Cyan
Write-Host "✅ Completed!" -ForegroundColor Green
Write-Host "  Total projects processed: $totalProjects" -ForegroundColor White
Write-Host "  Total references fixed: $totalFixed" -ForegroundColor White
Write-Host "==========================================" -ForegroundColor Cyan

# دالة مساعدة لحساب المسار النسبي
function Get-RelativePath {
    param(
        [string]$From,
        [string]$To
    )
    
    $fromUri = New-Object System.Uri($From + "\")
    $toUri = New-Object System.Uri($To)
    
    $relativeUri = $fromUri.MakeRelativeUri($toUri)
    $relativePath = [System.Uri]::UnescapeDataString($relativeUri.ToString())
    
    # تحويل / إلى \
    $relativePath = $relativePath.Replace("/", "\")
    
    return $relativePath
}