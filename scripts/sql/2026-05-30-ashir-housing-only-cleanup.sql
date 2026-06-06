-- =====================================================================
-- Ashir (عشير) — Housing-only cleanup migration
-- Date  : 2026-05-30
-- Scope : Ashare.Api database (SQLite / SQL Server / PostgreSQL)
--
-- WHAT THIS DOES
--   1) Deactivates the Administrative (10000000-0000-0000-0001-000000000004)
--      and Commercial (10000000-0000-0000-0001-000000000005) categories so
--      they stop appearing in `/api/categories` while preserving historic
--      product/listing rows for audit. If you want a hard cleanup, see the
--      commented HARD DELETE block at the bottom.
--   2) Removes the CategoryAttributeMapping rows that bind phone/WhatsApp
--      contact attributes to any category — the app no longer surfaces
--      these contact channels.
--   3) Deactivates the "Commercial & Administrative" subscription plan
--      (30000000-0000-0000-0000-000000000002).
--   4) Sets is_phone_allowed = false and is_whatsapp_allowed = false on all
--      existing AttributeValues so old listings don't keep promoting those
--      contact buttons.
--
-- HOW TO RUN
--   • SQLite      : sqlite3 ashare.db < 2026-05-30-ashir-housing-only-cleanup.sql
--   • SQL Server  : sqlcmd -d Ashare -i 2026-05-30-ashir-housing-only-cleanup.sql
--   • PostgreSQL  : psql -d ashare -f 2026-05-30-ashir-housing-only-cleanup.sql
--
-- Idempotent — safe to re-run.
-- =====================================================================

-- (1) Deactivate Administrative + Commercial categories
UPDATE "ProductCategories"
   SET "IsActive" = 0
 WHERE "Id" IN (
       '10000000-0000-0000-0001-000000000004',   -- Administrative
       '10000000-0000-0000-0001-000000000005'    -- Commercial
 );

-- (2) Remove phone/WhatsApp attribute mappings from every category
DELETE FROM "CategoryAttributeMappings"
 WHERE "AttributeDefinitionId" IN (
       '20000000-0000-0000-0002-000000000011',   -- is_phone_allowed
       '20000000-0000-0000-0002-000000000012'    -- is_whatsapp_allowed
 );

-- (3) Deactivate the legacy Commercial & Administrative subscription plan
UPDATE "SubscriptionPlans"
   SET "IsActive" = 0
 WHERE "Id" = '30000000-0000-0000-0000-000000000002';

-- (4) Force-disable phone / WhatsApp on every existing listing's attributes
--     so the UI never shows those buttons again, even on legacy rows.
UPDATE "AttributeValues"
   SET "Value" = 'false'
 WHERE "AttributeDefinitionId" IN (
       '20000000-0000-0000-0002-000000000011',   -- is_phone_allowed
       '20000000-0000-0000-0002-000000000012'    -- is_whatsapp_allowed
 );

-- =====================================================================
-- OPTIONAL HARD CLEANUP (uncomment only after confirming no live data
-- needs to be preserved). This removes listings + products attached to
-- Administrative/Commercial categories entirely.
-- =====================================================================
-- DELETE FROM "ProductListings"
--  WHERE "CategoryId" IN (
--        '10000000-0000-0000-0001-000000000004',
--        '10000000-0000-0000-0001-000000000005'
--  );
--
-- DELETE FROM "Products"
--  WHERE "CategoryId" IN (
--        '10000000-0000-0000-0001-000000000004',
--        '10000000-0000-0000-0001-000000000005'
--  );
--
-- DELETE FROM "ProductCategories"
--  WHERE "Id" IN (
--        '10000000-0000-0000-0001-000000000004',
--        '10000000-0000-0000-0001-000000000005'
--  );
--
-- DELETE FROM "AttributeDefinitions"
--  WHERE "Id" IN (
--        '20000000-0000-0000-0002-000000000011',   -- is_phone_allowed
--        '20000000-0000-0000-0002-000000000012',   -- is_whatsapp_allowed
--        '20000000-0000-0000-0004-000000000001'    -- commercial_property_type
--  );
--
-- DELETE FROM "SubscriptionPlans"
--  WHERE "Id" = '30000000-0000-0000-0000-000000000002';
