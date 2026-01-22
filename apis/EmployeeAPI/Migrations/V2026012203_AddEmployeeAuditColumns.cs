using FluentMigrator;
using Serilog;

namespace EmployeeAPI.Migrations;

[Migration(2026012203, "Add audit columns to employee")]
public sealed class V2026012203_AddEmployeeAuditColumns : Migration
{
    public override void Up()
    {
        Alter.Table("employee")
            .AddColumn("created_utc").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime)
            .AddColumn("updated_utc").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime)
            .AddColumn("deleted_utc").AsDateTime().Nullable();

        Execute.Sql(@"
            UPDATE employee
            SET created_utc = NOW(), updated_utc = NOW()
            WHERE created_utc IS NULL OR updated_utc IS NULL;
        ");
    }

    public override void Down()
    {
        Log.Information("Reverting audit columns from employee");
        Delete.Column("created_utc").FromTable("employee");
        Delete.Column("updated_utc").FromTable("employee");
        Delete.Column("deleted_utc").FromTable("employee");
    }
}
