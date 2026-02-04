using FluentMigrator;
using Serilog;

namespace EmployeeAPI.Migrations;

[Migration(2026020102, "Change audit_log.changed_fields to jsonb")]
public sealed class V2026020102_ChangeAuditLogChangedFieldsToJsonb : Migration
{
    public override void Up()
    {
        Log.Information("Changing audit_log.changed_fields to jsonb");
        Execute.Sql("""
            ALTER TABLE audit_log
            ALTER COLUMN changed_fields TYPE jsonb USING
              CASE
                WHEN changed_fields IS NULL THEN NULL
                WHEN btrim(changed_fields) LIKE '[%' THEN changed_fields::jsonb
                ELSE to_jsonb(string_to_array(changed_fields, ','))
              END;
            """);
    }

    public override void Down()
    {
        Log.Information("Reverting audit_log.changed_fields to text");
        Execute.Sql("""
            ALTER TABLE audit_log
            ALTER COLUMN changed_fields TYPE text USING
              CASE
                WHEN changed_fields IS NULL THEN NULL
                WHEN jsonb_typeof(changed_fields) = 'array' THEN (
                  SELECT array_to_string(ARRAY(
                    SELECT jsonb_array_elements_text(changed_fields)
                  ), ',')
                )
                ELSE changed_fields::text
              END;
            """);
    }
}
