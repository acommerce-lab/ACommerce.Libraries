# من جذر الحل
$projects = Get-ChildItem -Recurse -Filter *.csproj

foreach ($proj in $projects) {
    $projPath = $proj.FullName
    $projName = [System.IO.Path]::GetFileNameWithoutExtension($projPath)
    $projDir  = $proj.DirectoryName
    $readme   = Join-Path $projDir "README.md"

    if (Test-Path $readme) {
        $description = (Get-Content $readme -First 1).Trim()
        if ([string]::IsNullOrWhiteSpace($description)) {
            $description = "Default description for $projName"
        }
    } else {
        $description = "Default description for $projName"
    }

    # تحميل XML الخاص بالـ csproj
    [xml]$xml = Get-Content $projPath

    # البحث عن PropertyGroup أو إنشاؤه
    $pg = $xml.Project.PropertyGroup | Select-Object -First 1
    if (-not $pg) {
        $pg = $xml.CreateElement("PropertyGroup")
        $xml.Project.AppendChild($pg) | Out-Null
    }

    # إضافة/تحديث الخصائص
    $props = @{
        TargetFramework = "net9.0"
        PackageId       = $projName
        Version         = "1.0.0"
        Authors         = "ACommerce"
        Description     = $description
        GeneratePackageOnBuild = "true"
    }

    foreach ($key in $props.Keys) {
        $node = $pg.$key
        if ($node) {
            $node.InnerText = $props[$key]
        } else {
            $newNode = $xml.CreateElement($key)
            $newNode.InnerText = $props[$key]
            $pg.AppendChild($newNode) | Out-Null
        }
    }

    # حفظ التعديلات
    $xml.Save($projPath)
    Write-Host "Updated $projName"
}