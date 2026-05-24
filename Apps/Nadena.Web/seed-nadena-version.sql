-- ============================================================================
--  نادينا - تسجيل إصدار التطبيق في جدول AppVersions
--  قاعدة البيانات: asharedb  (SQL Server)
--  الغرض: يجعل فحص الإصدار يُرجع "Latest" بدل "Unsupported/حظر"
--
--  مهم: يجب أن يطابق ApplicationCode + VersionNumber ما يرسله التطبيق تماماً:
--        ApplicationCode = 'nadena-web'   (من VersionCheckService.WebAppCode)
--        VersionNumber   = '1.0.0'        (من <Version> في Nadena.Web.csproj)
--        BuildNumber     = 0
--  (إن غيّرت <Version> في المشروع لاحقاً، أضف صفّاً جديداً بنفس الرقم)
--
--  Status: 0=Development 1=Latest 2=Supported 3=Deprecated 4=Unsupported
-- ============================================================================

-- إصدار الويب (nadena-web)
IF NOT EXISTS (SELECT 1 FROM AppVersions
               WHERE ApplicationCode = 'nadena-web' AND VersionNumber = '1.0.0' AND IsDeleted = 0)
BEGIN
    INSERT INTO AppVersions
        (Id, CreatedAt, UpdatedAt, IsDeleted,
         ApplicationCode, ApplicationNameAr, ApplicationNameEn,
         VersionNumber, BuildNumber, Status, ReleaseDate,
         DeprecationStartDate, EndOfSupportDate,
         ReleaseNotesAr, ReleaseNotesEn, UpdateUrl, DownloadUrl,
         IsForceUpdate, MinimumSupportedVersion, IsActive, Metadata)
    VALUES
        (NEWID(), SYSUTCDATETIME(), NULL, 0,
         'nadena-web', N'موقع نادينا', N'Nadena Web App',
         '1.0.0', 0, 1 /* Latest */, SYSUTCDATETIME(),
         NULL, NULL,
         N'الإصدار الأول من منصة نادينا للويب', N'First release of Nadena web platform', NULL, NULL,
         0 /* IsForceUpdate */, NULL, 1 /* IsActive */, NULL);
END;

-- (اختياري) إصدار الموبايل (nadena-mobile) - شغّله عند بناء تطبيق الموبايل،
-- وعدّل VersionNumber/BuildNumber ليطابقا ما يرسله تطبيق MAUI.
-- IF NOT EXISTS (SELECT 1 FROM AppVersions
--                WHERE ApplicationCode = 'nadena-mobile' AND VersionNumber = '1.0.0' AND IsDeleted = 0)
-- BEGIN
--     INSERT INTO AppVersions
--         (Id, CreatedAt, UpdatedAt, IsDeleted,
--          ApplicationCode, ApplicationNameAr, ApplicationNameEn,
--          VersionNumber, BuildNumber, Status, ReleaseDate,
--          DeprecationStartDate, EndOfSupportDate,
--          ReleaseNotesAr, ReleaseNotesEn, UpdateUrl, DownloadUrl,
--          IsForceUpdate, MinimumSupportedVersion, IsActive, Metadata)
--     VALUES
--         (NEWID(), SYSUTCDATETIME(), NULL, 0,
--          'nadena-mobile', N'تطبيق نادينا', N'Nadena Mobile App',
--          '1.0.0', 1, 1 /* Latest */, SYSUTCDATETIME(),
--          NULL, NULL,
--          N'الإصدار الأول من تطبيق نادينا', N'First release of Nadena mobile app',
--          N'https://nadena-nc.com', NULL,
--          0, NULL, 1, NULL);
-- END;

-- للتأكد بعد التنفيذ:
-- SELECT ApplicationCode, VersionNumber, BuildNumber, Status, IsActive
-- FROM AppVersions WHERE ApplicationCode LIKE 'nadena-%';
