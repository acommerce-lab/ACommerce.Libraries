-- =====================================================================
-- ACommerce.AppConfig — Manual bootstrap script
-- Date  : 2026-05-30
-- Scope : Add a minimal set of Feature Flags / UI Strings / Theme Tokens
--         WITHOUT running the C# seeder. Useful for environments where
--         SKIP_SEEDING=true is set or to add a single new key/value.
--
-- HOW TO RUN
--   SQLite     : sqlite3 ashare.db < 2026-05-30-appconfig-bootstrap.sql
--   SQL Server : sqlcmd -d Ashare  -i 2026-05-30-appconfig-bootstrap.sql
--   PostgreSQL : psql  -d ashare   -f 2026-05-30-appconfig-bootstrap.sql
--
-- The script is idempotent — re-running it is safe (uses NOT EXISTS).
-- =====================================================================

-- ─── Feature Flags ──────────────────────────────────────────────
INSERT INTO "FeatureFlags" ("Id","Key","Enabled","Platforms","MinAppVersion","MaxAppVersion","Description","RequiresClientRestart","CreatedAt","IsDeleted")
SELECT '11111111-aaaa-aaaa-aaaa-000000000001','payments.enabled',1,NULL,NULL,NULL,'نظام الدفع العام',0,CURRENT_TIMESTAMP,0
WHERE NOT EXISTS (SELECT 1 FROM "FeatureFlags" WHERE "Key" = 'payments.enabled' AND "IsDeleted" = 0);

INSERT INTO "FeatureFlags" ("Id","Key","Enabled","Platforms","MinAppVersion","MaxAppVersion","Description","RequiresClientRestart","CreatedAt","IsDeleted")
SELECT '11111111-aaaa-aaaa-aaaa-000000000002','booking.enabled',1,NULL,NULL,NULL,'إظهار زر الحجز ومسار BookingCreate',0,CURRENT_TIMESTAMP,0
WHERE NOT EXISTS (SELECT 1 FROM "FeatureFlags" WHERE "Key" = 'booking.enabled' AND "IsDeleted" = 0);

INSERT INTO "FeatureFlags" ("Id","Key","Enabled","Platforms","MinAppVersion","MaxAppVersion","Description","RequiresClientRestart","CreatedAt","IsDeleted")
SELECT '11111111-aaaa-aaaa-aaaa-000000000003','auth.nafath',1,NULL,NULL,NULL,'تسجيل الدخول عبر نفاذ',0,CURRENT_TIMESTAMP,0
WHERE NOT EXISTS (SELECT 1 FROM "FeatureFlags" WHERE "Key" = 'auth.nafath' AND "IsDeleted" = 0);

INSERT INTO "FeatureFlags" ("Id","Key","Enabled","Platforms","MinAppVersion","MaxAppVersion","Description","RequiresClientRestart","CreatedAt","IsDeleted")
SELECT '11111111-aaaa-aaaa-aaaa-000000000004','version_check.enabled',1,NULL,NULL,NULL,'فحص توفر تحديث',0,CURRENT_TIMESTAMP,0
WHERE NOT EXISTS (SELECT 1 FROM "FeatureFlags" WHERE "Key" = 'version_check.enabled' AND "IsDeleted" = 0);

-- ─── UI String overrides (samples) ──────────────────────────────
INSERT INTO "UiStrings" ("Id","Key","Language","Value","IsActive","Note","CreatedAt","IsDeleted")
SELECT '22222222-bbbb-bbbb-bbbb-000000000001','AppTagline','ar','ابحث عن عشيرك في السكن',1,'override remotely without redeploy',CURRENT_TIMESTAMP,0
WHERE NOT EXISTS (SELECT 1 FROM "UiStrings" WHERE "Key" = 'AppTagline' AND "Language" = 'ar' AND "IsDeleted" = 0);

INSERT INTO "UiStrings" ("Id","Key","Language","Value","IsActive","Note","CreatedAt","IsDeleted")
SELECT '22222222-bbbb-bbbb-bbbb-000000000002','AppTagline','en','Find your Ashir in shared housing',1,NULL,CURRENT_TIMESTAMP,0
WHERE NOT EXISTS (SELECT 1 FROM "UiStrings" WHERE "Key" = 'AppTagline' AND "Language" = 'en' AND "IsDeleted" = 0);

-- ─── Theme Tokens (Light) ───────────────────────────────────────
INSERT INTO "ThemeTokens" ("Id","Key","Mode","Value","IsActive","CreatedAt","IsDeleted")
SELECT '33333333-cccc-cccc-cccc-000000000001','primary',0,'#345454',1,CURRENT_TIMESTAMP,0
WHERE NOT EXISTS (SELECT 1 FROM "ThemeTokens" WHERE "Key" = 'primary' AND "Mode" = 0 AND "IsDeleted" = 0);

INSERT INTO "ThemeTokens" ("Id","Key","Mode","Value","IsActive","CreatedAt","IsDeleted")
SELECT '33333333-cccc-cccc-cccc-000000000002','secondary',0,'#F4844C',1,CURRENT_TIMESTAMP,0
WHERE NOT EXISTS (SELECT 1 FROM "ThemeTokens" WHERE "Key" = 'secondary' AND "Mode" = 0 AND "IsDeleted" = 0);

-- =====================================================================
-- Example follow-ups (commented):
--
-- Disable the booking module globally:
--   UPDATE "FeatureFlags" SET "Enabled" = 0 WHERE "Key" = 'booking.enabled';
--
-- Roll out a feature only to mobile clients on v1.18+:
--   UPDATE "FeatureFlags"
--      SET "Platforms"     = 'android,ios',
--          "MinAppVersion" = '1.18'
--    WHERE "Key" = 'payments.moyasar';
--
-- Change the disclaimer wording remotely:
--   UPDATE "UiStrings" SET "Value" = '… النص الجديد …'
--    WHERE "Key" = 'ListingDisclaimerBody' AND "Language" = 'ar';
--
-- Tweak brand primary color without a deploy:
--   UPDATE "ThemeTokens" SET "Value" = '#2A4848' WHERE "Key" = 'primary' AND "Mode" = 0;
-- =====================================================================
