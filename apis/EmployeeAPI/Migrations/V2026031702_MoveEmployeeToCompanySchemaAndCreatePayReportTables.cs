using FluentMigrator;
using Serilog;

namespace EmployeeAPI.Migrations;

[Migration(2026031702, "Create public pay, tax, and report tables")]
public sealed class V2026031702_MoveEmployeeToCompanySchemaAndCreatePayReportTables : Migration
{

    public override void Up()
    {
        Log.Information("Creating public pay, tax, and report tables");

        // Limiting FK usage between tables keeps the demo closer to separate-service ownership.
        // This first cut is intentionally simple so UI/backend work can move forward quickly.
        // sandbox_pay_entry is the mutable work area; pay_entry is append-only final payroll data.
        // Corrections in pay_entry should be negative/reversing inserts rather than updates.
        // pay_check is the printable/finalized output snapshot for checks and check-style reports.
        // public.report is shared report metadata; public.report_data stores report output for now.
        // public.tax is shared tax metadata/rates.
        // Tax code pattern idea:
        //   TAX-<scope>-<jurisdiction>-<type>-<subtype>-<local>-Y<year>
        // Examples:
        //   TAX-F-US-INCOME-NA-NA-Y2026
        //   TAX-S-MD-INCOME-NA-NA-Y2026
        //   TAX-O-PA-LST-PSD1234-NA-Y2026
        // Keep year as its own DB column for filtering/search even if the generated code also ends with Y####.
        // Report code pattern idea:
        //   RP-<ALPHA_CODE>-V<version>
        // Examples:
        //   RP-PAYCK-V1
        //   RP-PAYRL-V1
        //   RP-TAXSM-V1
        // Follow-up ideas: tax, timesheet, department, job, and employee pay-profile tables.

        Execute.Sql($@"
            CREATE EXTENSION IF NOT EXISTS ""uuid-ossp"";

            CREATE TABLE IF NOT EXISTS public.tax (
                id uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
                year integer NOT NULL,
                tax_code varchar(50) NOT NULL,
                percent numeric(10, 4) NULL,
                amount numeric(10, 2) NULL,
                created_utc timestamp with time zone NOT NULL DEFAULT now(),
                updated_utc timestamp with time zone NOT NULL DEFAULT now()
            );

            CREATE TABLE IF NOT EXISTS public.report (
                id uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
                report_code varchar(100) NOT NULL,
                descr varchar(255) NULL,
                created_utc timestamp with time zone NOT NULL DEFAULT now(),
                updated_utc timestamp with time zone NOT NULL DEFAULT now()
            );

            CREATE TABLE IF NOT EXISTS public.pay_entry (
                id uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
                pay_code varchar(50) NOT NULL,
                credit numeric(10, 2) NOT NULL,
                debit numeric(10, 2) NOT NULL,
                employee_id uuid NOT NULL,
                pay_id varchar(50) NOT NULL,
                created_utc timestamp with time zone NOT NULL DEFAULT now(),
                updated_utc timestamp with time zone NOT NULL DEFAULT now()
            );

            CREATE TABLE IF NOT EXISTS public.sandbox_pay_entry (
                id uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
                pay_code varchar(50) NOT NULL,
                credit numeric(10, 2) NOT NULL,
                debit numeric(10, 2) NOT NULL,
                employee_id uuid NOT NULL,
                pay_id varchar(50) NOT NULL,
                created_utc timestamp with time zone NOT NULL DEFAULT now(),
                updated_utc timestamp with time zone NOT NULL DEFAULT now()
            );

            CREATE TABLE IF NOT EXISTS public.pay_check (
                id uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
                pay_id varchar(50) NOT NULL,
                employee_id uuid NOT NULL,
                employee_name varchar(255) NOT NULL,
                company_name varchar(255) NOT NULL,
                company_address text,
                employee_address text,
                gross_pay numeric(10, 2) NOT NULL DEFAULT 0,
                total_deductions numeric(10, 2) NOT NULL DEFAULT 0,
                net_pay numeric(10, 2) NOT NULL DEFAULT 0,
                check_date timestamp with time zone,
                created_utc timestamp with time zone NOT NULL DEFAULT now(),
                updated_utc timestamp with time zone NOT NULL DEFAULT now()
            );

            CREATE TABLE IF NOT EXISTS public.report_data (
                id uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
                report_code varchar(100) NOT NULL,
                report_data jsonb NOT NULL DEFAULT '{{}}'::jsonb,
                created_utc timestamp with time zone NOT NULL DEFAULT now(),
                updated_utc timestamp with time zone NOT NULL DEFAULT now()
            );
        ");
    }

    public override void Down()
    {
        Log.Information("Removing public pay, tax, and report tables");

        Execute.Sql($@"
            DROP TABLE IF EXISTS public.report_data;
            DROP TABLE IF EXISTS public.pay_check;
            DROP TABLE IF EXISTS public.pay_entry;
            DROP TABLE IF EXISTS public.sandbox_pay_entry;
            DROP TABLE IF EXISTS public.report;
            DROP TABLE IF EXISTS public.tax;
        ");
    }
}
