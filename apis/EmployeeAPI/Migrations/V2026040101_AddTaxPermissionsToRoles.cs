using FluentMigrator;
using Serilog;

namespace EmployeeAPI.Migrations;

[Migration(2026040101, "Add tax CRUD permissions and assign tax access roles")]
public sealed class V2026040101_AddTaxPermissionsToRoles : Migration
{
    public override void Up()
    {
        Log.Information("Adding tax CRUD permissions and role mappings");

        Execute.Sql(@"
            INSERT INTO _auth.permission (
                permission_code,
                permission_name,
                description,
                system_code,
                resource_code,
                can_create,
                can_read,
                can_update,
                can_delete,
                is_active
            )
            VALUES
                ('tax-create', 'Tax Create', 'Create tax calculation data', 'tax_api', 'tax', true, false, false, false, true),
                ('tax-update', 'Tax Update', 'Update tax calculation data', 'tax_api', 'tax', false, true, true, false, true),
                ('tax-delete', 'Tax Delete', 'Delete tax calculation data', 'tax_api', 'tax', false, false, false, true, true)
            ON CONFLICT (permission_code) DO UPDATE
            SET
                permission_name = EXCLUDED.permission_name,
                description = EXCLUDED.description,
                system_code = EXCLUDED.system_code,
                resource_code = EXCLUDED.resource_code,
                can_create = EXCLUDED.can_create,
                can_read = EXCLUDED.can_read,
                can_update = EXCLUDED.can_update,
                can_delete = EXCLUDED.can_delete,
                is_active = EXCLUDED.is_active,
                updated_utc = timezone('utc', now());
        ");

        Execute.Sql(@"
            INSERT INTO _auth.role_permission (role_code, permission_code)
            VALUES
                ('super_admin', 'tax-create'),
                ('super_admin', 'tax-update'),
                ('super_admin', 'tax-delete'),
                ('customer_service_create', 'tax-read'),
                ('payroll_admin', 'tax-read')
            ON CONFLICT (role_code, permission_code) DO NOTHING;
        ");
    }

    public override void Down()
    {
        Log.Information("Removing tax CRUD permissions and added role mappings");

        Execute.Sql(@"
            DELETE FROM _auth.role_permission
            WHERE (role_code = 'super_admin' AND permission_code IN ('tax-create', 'tax-update', 'tax-delete'))
               OR (role_code = 'customer_service_create' AND permission_code = 'tax-read')
               OR (role_code = 'payroll_admin' AND permission_code = 'tax-read');
        ");

        Execute.Sql(@"
            DELETE FROM _auth.permission
            WHERE permission_code IN ('tax-create', 'tax-update', 'tax-delete');
        ");
    }
}
