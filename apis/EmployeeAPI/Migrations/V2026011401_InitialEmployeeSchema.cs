using FluentMigrator;
using Serilog;

namespace EmployeeAPI.Migrations;

[Migration(2026011401, "Initial employee schema")]
public sealed class V2026011401_InitialEmployeeSchema : Migration
{
    public override void Up()
    {
        // Ensure the uuid-ossp extension is available for generating UUIDs
        Execute.Sql("CREATE EXTENSION IF NOT EXISTS \"uuid-ossp\";");

        Create.Table("employee").
            WithColumn("id").AsGuid().PrimaryKey().WithDefault(SystemMethods.NewGuid).
            WithColumn("first_name").AsString(255).NotNullable().
            WithColumn("last_name").AsString(255).NotNullable().
            WithColumn("email").AsString(255).NotNullable().
            WithColumn("phone").AsString(20).Nullable().
            WithColumn("ssn").AsString(11).NotNullable().Unique().
            WithColumn("date_of_birth").AsDateTime().Nullable();
    }

    public override void Down()
    {
        Log.Information("Reverting initial employee schema migration");
        Delete.Table("employee");
    }
}
