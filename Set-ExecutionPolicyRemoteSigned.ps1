# تعيين مجلد الإخراج
$OutputFile = "SolutionClassHierarchyReport.txt"
Clear-Content $OutputFile

# البحث عن جميع ملفات الكود (C#) في المجلد الحالي والمجلدات الفرعية
$CSharpFiles = Get-ChildItem -Path . -Include "*.cs" -Recurse -File

# البدء بالمعالجة
Add-Content -Path $OutputFile "### ACommerce Solution Class Hierarchy Report ###"
Add-Content -Path $OutputFile "---"

foreach ($File in $CSharpFiles) {
    # استخلاص اسم المشروع (المكتبة) من المسار
    # نبحث عن اول مجلد فرعي باسم المشروع (مثل ACommerce.Catalog.Products)
    $ProjectName = $File.DirectoryName -Split '\\' | Select-Object -Last 1

    # البحث عن الكلاسات والواجهات (Classes/Interfaces)
    # ملاحظة: السكريبت يبحث عن كلمات "class" أو "interface" متبوعة باسم
    $Content = Get-Content $File.FullName | Select-String -Pattern '(public|internal|private)\s+(class|interface|abstract class|record)\s+([A-Za-z0-9_]+)\s*(:\s*([A-Za-z0-9_<>,\s]+))?' -AllMatches

    if ($Content) {
        Add-Content -Path $OutputFile "`n## Project: $($ProjectName)`nFile: $($File.Name)"
        
        foreach ($Match in $Content.Matches) {
            $Type = $Match.Groups[2].Value
            $Name = $Match.Groups[3].Value
            $Inheritance = $Match.Groups[5].Value # المجموعة الخامسة هي التي تلتقط ما بعد ":"
            
            $OutputLine = "- [${Type}] **$Name**"
            
            if (-not [string]::IsNullOrEmpty($Inheritance)) {
                $OutputLine += " : Inherits/Implements ($Inheritance)"
            }

            Add-Content -Path $OutputFile $OutputLine
        }
    }
}

Add-Content -Path $OutputFile "---"
Add-Content -Path $OutputFile "### End of Report ###"

Write-Host "✅ تم الانتهاء من استخراج هيكل الفئات. تجده في ملف $OutputFile"