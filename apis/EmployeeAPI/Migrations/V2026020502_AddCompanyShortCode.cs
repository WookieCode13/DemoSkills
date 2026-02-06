using FluentMigrator;
using Serilog;

namespace EmployeeAPI.Migrations;

[Migration(2026020502, "Add company short_code")]
public sealed class V2026020502_AddCompanyShortCode : Migration
{
    public override void Up()
    {
        Log.Information("Adding company.short_code");

        Execute.Sql(@"
            ALTER TABLE company
            ADD COLUMN IF NOT EXISTS short_code varchar(10) NULL;
        ");

        Execute.Sql(@"
            UPDATE company
            SET short_code = LEFT(UPPER(regexp_replace(name, '[^A-Za-z0-9]', '', 'g')), 10)
            WHERE short_code IS NULL;
        ");

        Execute.Sql("ALTER TABLE company ALTER COLUMN short_code SET NOT NULL;");

        Execute.Sql(@"
            ALTER TABLE company
            ADD CONSTRAINT IF NOT EXISTS company_short_code_format
            CHECK (short_code IS NULL OR (char_length(short_code) = 10 AND short_code ~ '^[A-Z0-9]{10}$' AND short_code = upper(short_code)));
        ");

        Execute.Sql(@"
            CREATE UNIQUE INDEX IF NOT EXISTS ux_company_short_code
            ON company (short_code);
        ");
    }

    public override void Down()
    {
        Log.Information("Removing company.short_code");
        Execute.Sql("DROP INDEX IF EXISTS ux_company_short_code;");
        Execute.Sql("ALTER TABLE company DROP CONSTRAINT IF EXISTS company_short_code_format;");
        Execute.Sql("ALTER TABLE company ALTER COLUMN short_code DROP NOT NULL;");
        Execute.Sql("ALTER TABLE company DROP COLUMN IF EXISTS short_code;");
    }
}
