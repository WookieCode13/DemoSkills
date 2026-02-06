using FluentMigrator;
using Serilog;

namespace EmployeeAPI.Migrations;

[Migration(2026020501, "Fix employee audit timestamps with infinity values")]
public sealed class V2026020501_FixUtcInfinityDates : Migration
{
    public override void Up()
    {
        Log.Information("Fixing employee audit timestamps with infinity values");
        Execute.Sql(@"
            UPDATE employee
            SET created_utc = CURRENT_TIMESTAMP
            WHERE created_utc = 'infinity'::timestamptz OR created_utc = '-infinity'::timestamptz;
            UPDATE employee
            SET updated_utc = CURRENT_TIMESTAMP
            WHERE updated_utc = 'infinity'::timestamptz OR updated_utc = '-infinity'::timestamptz;
        ");
    }

    public override void Down()
    {
        Log.Information("No down migration for fixing employee audit timestamp infinity values");
    }
}
