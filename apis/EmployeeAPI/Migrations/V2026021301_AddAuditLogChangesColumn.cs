using FluentMigrator;
using Serilog;

namespace EmployeeAPI.Migrations;

[Migration(2026021301, "Add audit_log.changes jsonb column")]
public sealed class V2026021301_AddAuditLogChangesColumn : Migration
{
    public override void Up()
    {
        Log.Information("Adding audit_log.changes");
        Execute.Sql("""
            ALTER TABLE audit_log
            ADD COLUMN IF NOT EXISTS changes jsonb NULL;
            """);
    }

    public override void Down()
    {
        Log.Information("Removing audit_log.changes");
        Execute.Sql("""
            ALTER TABLE audit_log
            DROP COLUMN IF EXISTS changes;
            """);
    }
}
