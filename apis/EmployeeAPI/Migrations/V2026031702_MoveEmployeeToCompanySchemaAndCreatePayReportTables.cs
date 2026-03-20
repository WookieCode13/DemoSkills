using FluentMigrator;
using Serilog;

namespace EmployeeAPI.Migrations;

[Migration(2026031702, "Move employee table to demoskills schema and create pay/report tables")]
public sealed class V2026031702_MoveEmployeeToCompanySchemaAndCreatePayReportTables : Migration
{
    private const string CompanySchema = "demoskills";

    public override void Up()
    {
        Log.Information("Moving employee table into {Schema} schema and creating pay/report tables", CompanySchema);

        // Limiting FK usage between tables keeps the demo closer to separate-service ownership.
        // This first cut is intentionally simple so UI/backend work can move forward quickly.
        // sandbox_pay_entry is the mutable work area; pay_entry is append-only final payroll data.
        // Corrections in pay_entry should be negative/reversing inserts rather than updates.
        // pay_check is the printable/finalized output snapshot for checks and check-style reports.
        // public.report is shared report metadata; {schema}.report_data is tenant/company-scoped output.
        // public.tax is shared tax metadata/rates.
        // Follow-up ideas: tax, timesheet, department, job, and employee pay-profile tables.

        Execute.Sql($@"
            CREATE EXTENSION IF NOT EXISTS ""uuid-ossp"";
            CREATE SCHEMA IF NOT EXISTS {CompanySchema};

            DO $$
            BEGIN
                IF EXISTS (
                    SELECT 1
                    FROM information_schema.tables
                    WHERE table_schema = 'public' AND table_name = 'employee'
                ) AND NOT EXISTS (
                    SELECT 1
                    FROM information_schema.tables
                    WHERE table_schema = '{CompanySchema}' AND table_name = 'employee'
                ) THEN
                    ALTER TABLE public.employee SET SCHEMA {CompanySchema};
                END IF;
            END $$;

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

            CREATE TABLE IF NOT EXISTS {CompanySchema}.pay_entry (
                id uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
                pay_code varchar(50) NOT NULL,
                credit numeric(10, 2) NOT NULL,
                debit numeric(10, 2) NOT NULL,
                employee_id uuid NOT NULL,
                pay_id varchar(50) NOT NULL,
                created_utc timestamp with time zone NOT NULL DEFAULT now(),
                updated_utc timestamp with time zone NOT NULL DEFAULT now()
            );

            CREATE TABLE IF NOT EXISTS {CompanySchema}.sandbox_pay_entry (
                id uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
                pay_code varchar(50) NOT NULL,
                credit numeric(10, 2) NOT NULL,
                debit numeric(10, 2) NOT NULL,
                employee_id uuid NOT NULL,
                pay_id varchar(50) NOT NULL,
                created_utc timestamp with time zone NOT NULL DEFAULT now(),
                updated_utc timestamp with time zone NOT NULL DEFAULT now()
            );

            CREATE TABLE IF NOT EXISTS {CompanySchema}.pay_check (
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

            CREATE TABLE IF NOT EXISTS {CompanySchema}.report_data (
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
        Log.Information("Moving employee table back to public schema and removing {Schema} payroll/report tables", CompanySchema);

        Execute.Sql($@"
            DROP TABLE IF EXISTS {CompanySchema}.report_data;
            DROP TABLE IF EXISTS {CompanySchema}.pay_check;
            DROP TABLE IF EXISTS {CompanySchema}.pay_entry;
            DROP TABLE IF EXISTS {CompanySchema}.sandbox_pay_entry;
            DROP TABLE IF EXISTS public.report;
            DROP TABLE IF EXISTS public.tax;

            DO $$
            BEGIN
                IF EXISTS (
                    SELECT 1
                    FROM information_schema.tables
                    WHERE table_schema = '{CompanySchema}' AND table_name = 'employee'
                ) AND NOT EXISTS (
                    SELECT 1
                    FROM information_schema.tables
                    WHERE table_schema = 'public' AND table_name = 'employee'
                ) THEN
                    ALTER TABLE {CompanySchema}.employee SET SCHEMA public;
                END IF;
            END $$;
        ");
    }
}
