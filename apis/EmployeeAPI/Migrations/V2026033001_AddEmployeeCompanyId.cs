using FluentMigrator;
using Serilog;

namespace EmployeeAPI.Migrations;

[Migration(2026033001, "Add company_id to employee")]
public sealed class V2026033001_AddEmployeeCompanyId : Migration
{
    public override void Up()
    {
        Log.Information("Adding employee.company_id");

        Execute.Sql(@"
            ALTER TABLE employee
            ADD COLUMN IF NOT EXISTS company_id uuid NULL;
        ");

        Execute.Sql(@"
            UPDATE employee
            SET company_id = c.id
            FROM company c
            WHERE employee.company_id IS NULL
              AND c.short_code = 'DEMOSKILLS';
        ");

        Execute.Sql(@"
            CREATE INDEX IF NOT EXISTS ix_employee_company_id
            ON employee (company_id);
        ");

        Execute.Sql(@"
            ALTER TABLE employee
            ADD CONSTRAINT fk_employee_company
            FOREIGN KEY (company_id) REFERENCES company (id);
        ");
    }

    public override void Down()
    {
        Log.Information("Removing employee.company_id");

        Execute.Sql(@"
            ALTER TABLE employee
            DROP CONSTRAINT IF EXISTS fk_employee_company;
        ");

        Execute.Sql(@"
            DROP INDEX IF EXISTS ix_employee_company_id;
        ");

        Execute.Sql(@"
            ALTER TABLE employee
            DROP COLUMN IF EXISTS company_id;
        ");
    }
}
