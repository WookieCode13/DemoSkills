using FluentMigrator;
using Serilog;

namespace EmployeeAPI.Migrations;

[Migration(2026012901, "Add audit log table")]
public sealed class V2026012901_AuditLog : Migration
{
    public override void Up()
    {
        Log.Information("Creating audit_log table");
        Create.Table("audit_log")
            .WithColumn("id").AsGuid().PrimaryKey().WithDefault(SystemMethods.NewGuid)
            .WithColumn("entity_type").AsString(200).NotNullable()
            .WithColumn("entity_id").AsGuid().NotNullable()
            .WithColumn("action").AsString(50).NotNullable()
            .WithColumn("occurred_utc").AsCustom("timestamptz").NotNullable()
            .WithColumn("performed_by").AsString(255).NotNullable().WithDefaultValue("system")
            .WithColumn("changed_fields").AsCustom("jsonb").Nullable()
            .WithColumn("note").AsCustom("text").Nullable()
            .WithColumn("correlation_id").AsCustom("text").Nullable();

        Create.Index("ix_audit_log_entity")
            .OnTable("audit_log")
            .OnColumn("entity_type").Ascending()
            .OnColumn("entity_id").Ascending()
            .OnColumn("occurred_utc").Descending();

        Create.Index("ix_audit_log_occurred_utc")
            .OnTable("audit_log")
            .OnColumn("occurred_utc").Descending();
    }

    public override void Down()
    {
        Log.Information("Dropping audit_log table");
        Delete.Table("audit_log");
    }
}
