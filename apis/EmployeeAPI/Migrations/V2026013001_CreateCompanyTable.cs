using FluentMigrator;

namespace EmployeeAPI.Migrations;

[Migration(2026013001, "Create company table if missing (moved from Alembic)")]
public sealed class V2026013001_CreateCompanyTable : Migration
{
    public override void Up()
    {
        Execute.Sql(@"
            CREATE EXTENSION IF NOT EXISTS ""uuid-ossp"";
            CREATE TABLE IF NOT EXISTS company (
                id uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
                name varchar(255) NOT NULL,
                industry varchar(255) NULL,
                email varchar(255) NULL,
                phone varchar(20) NULL,
                created_utc timestamptz NOT NULL DEFAULT timezone('utc', now()),
                updated_utc timestamptz NOT NULL DEFAULT timezone('utc', now()),
                deleted_utc timestamptz NULL
            );
        ");
    }

    public override void Down()
    {
        Execute.Sql("DROP TABLE IF EXISTS company;");
    }
}
