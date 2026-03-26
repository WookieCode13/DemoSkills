-- Reset the March 17, 2026 migrations on a Linux/Postgres environment.
-- Intended use:
-- 1. move employee back to public
-- 2. remove the temporary demoskills schema objects
-- 3. remove FluentMigrator version rows so the edited migrations can rerun
--
-- Assumptions:
-- - FluentMigrator uses the default public.VersionInfo table
-- - _auth schema/data should be preserved
-- - data in demoskills/pay/report tables is disposable

BEGIN;

-- Move employee back to public if needed.
DO $$
BEGIN
    IF EXISTS (
        SELECT 1
        FROM information_schema.tables
        WHERE table_schema = 'demoskills' AND table_name = 'employee'
    ) AND NOT EXISTS (
        SELECT 1
        FROM information_schema.tables
        WHERE table_schema = 'public' AND table_name = 'employee'
    ) THEN
        ALTER TABLE demoskills.employee SET SCHEMA public;
    END IF;
END $$;

-- Drop tenant-style objects created by the temporary schema migration.
DROP TABLE IF EXISTS demoskills.report_data;
DROP TABLE IF EXISTS demoskills.pay_check;
DROP TABLE IF EXISTS demoskills.pay_entry;
DROP TABLE IF EXISTS demoskills.sandbox_pay_entry;

-- Drop shared metadata tables created by the temporary schema migration.
DROP TABLE IF EXISTS public.report;
DROP TABLE IF EXISTS public.tax;

-- Drop schema if nothing remains in it.
DO $$
BEGIN
    IF EXISTS (
        SELECT 1
        FROM information_schema.schemata
        WHERE schema_name = 'demoskills'
    ) THEN
        EXECUTE 'DROP SCHEMA IF EXISTS demoskills';
    END IF;
END $$;

-- Remove migration version rows so the edited migrations rerun.
DELETE FROM public."VersionInfo"
WHERE "Version" IN (2026031701, 2026031702);

COMMIT;
