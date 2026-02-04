using FluentMigrator;
using Serilog;

namespace EmployeeAPI.Migrations;

[Migration(2026020103, "Change employee.date_of_birth to date")]
public sealed class V2026020103_ChangeEmployeeDobToDate : Migration
{
    public override void Up()
    {
        Log.Information("Changing employee.date_of_birth to date");
        Alter.Column("date_of_birth").OnTable("employee").AsDate().Nullable();
    }

    public override void Down()
    {
        Log.Information("Reverting employee.date_of_birth to timestamp");
        Alter.Column("date_of_birth").OnTable("employee").AsDateTime().Nullable();
    }
}
