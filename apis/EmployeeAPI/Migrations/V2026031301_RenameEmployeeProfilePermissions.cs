using FluentMigrator;
using Serilog;

namespace EmployeeAPI.Migrations;

[Migration(2026031301, "Rename employee profile permissions to employee permissions")]
public sealed class V2026031301_RenameEmployeeProfilePermissions : Migration
{
    public override void Up()
    {
        Log.Information("Renaming employee profile permissions");

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
                is_active,
                created_utc,
                updated_utc
            )
            SELECT
                'employee-read',
                'Employee Read',
                'Read employee data',
                system_code,
                'employee',
                can_create,
                can_read,
                can_update,
                can_delete,
                is_active,
                created_utc,
                timezone('utc', now())
            FROM _auth.permission
            WHERE permission_code = 'employee-profile-read'
            ON CONFLICT (permission_code) DO UPDATE
            SET
                permission_name = EXCLUDED.permission_name,
                description = EXCLUDED.description,
                resource_code = EXCLUDED.resource_code,
                can_create = EXCLUDED.can_create,
                can_read = EXCLUDED.can_read,
                can_update = EXCLUDED.can_update,
                can_delete = EXCLUDED.can_delete,
                is_active = EXCLUDED.is_active,
                updated_utc = timezone('utc', now());

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
                is_active,
                created_utc,
                updated_utc
            )
            SELECT
                'employee-update',
                'Employee Update',
                'Update employee data',
                system_code,
                'employee',
                can_create,
                can_read,
                can_update,
                can_delete,
                is_active,
                created_utc,
                timezone('utc', now())
            FROM _auth.permission
            WHERE permission_code = 'employee-profile-update'
            ON CONFLICT (permission_code) DO UPDATE
            SET
                permission_name = EXCLUDED.permission_name,
                description = EXCLUDED.description,
                resource_code = EXCLUDED.resource_code,
                can_create = EXCLUDED.can_create,
                can_read = EXCLUDED.can_read,
                can_update = EXCLUDED.can_update,
                can_delete = EXCLUDED.can_delete,
                is_active = EXCLUDED.is_active,
                updated_utc = timezone('utc', now());

            INSERT INTO _auth.role_permission (role_code, permission_code, created_utc)
            SELECT role_code, 'employee-read', created_utc
            FROM _auth.role_permission
            WHERE permission_code = 'employee-profile-read'
            ON CONFLICT (role_code, permission_code) DO NOTHING;

            INSERT INTO _auth.role_permission (role_code, permission_code, created_utc)
            SELECT role_code, 'employee-update', created_utc
            FROM _auth.role_permission
            WHERE permission_code = 'employee-profile-update'
            ON CONFLICT (role_code, permission_code) DO NOTHING;

            DELETE FROM _auth.role_permission
            WHERE permission_code IN ('employee-profile-read', 'employee-profile-update');

            DELETE FROM _auth.permission
            WHERE permission_code IN ('employee-profile-read', 'employee-profile-update');
        ");
    }

    public override void Down()
    {
        Log.Information("Restoring employee profile permissions");

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
                is_active,
                created_utc,
                updated_utc
            )
            SELECT
                'employee-profile-read',
                'Employee Profile Read',
                'Read employee profile data',
                system_code,
                'profile',
                can_create,
                can_read,
                can_update,
                can_delete,
                is_active,
                created_utc,
                timezone('utc', now())
            FROM _auth.permission
            WHERE permission_code = 'employee-read'
            ON CONFLICT (permission_code) DO UPDATE
            SET
                permission_name = EXCLUDED.permission_name,
                description = EXCLUDED.description,
                resource_code = EXCLUDED.resource_code,
                can_create = EXCLUDED.can_create,
                can_read = EXCLUDED.can_read,
                can_update = EXCLUDED.can_update,
                can_delete = EXCLUDED.can_delete,
                is_active = EXCLUDED.is_active,
                updated_utc = timezone('utc', now());

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
                is_active,
                created_utc,
                updated_utc
            )
            SELECT
                'employee-profile-update',
                'Employee Profile Update',
                'Update employee profile data',
                system_code,
                'profile',
                can_create,
                can_read,
                can_update,
                can_delete,
                is_active,
                created_utc,
                timezone('utc', now())
            FROM _auth.permission
            WHERE permission_code = 'employee-update'
            ON CONFLICT (permission_code) DO UPDATE
            SET
                permission_name = EXCLUDED.permission_name,
                description = EXCLUDED.description,
                resource_code = EXCLUDED.resource_code,
                can_create = EXCLUDED.can_create,
                can_read = EXCLUDED.can_read,
                can_update = EXCLUDED.can_update,
                can_delete = EXCLUDED.can_delete,
                is_active = EXCLUDED.is_active,
                updated_utc = timezone('utc', now());

            INSERT INTO _auth.role_permission (role_code, permission_code, created_utc)
            SELECT role_code, 'employee-profile-read', created_utc
            FROM _auth.role_permission
            WHERE permission_code = 'employee-read'
            ON CONFLICT (role_code, permission_code) DO NOTHING;

            INSERT INTO _auth.role_permission (role_code, permission_code, created_utc)
            SELECT role_code, 'employee-profile-update', created_utc
            FROM _auth.role_permission
            WHERE permission_code = 'employee-update'
            ON CONFLICT (role_code, permission_code) DO NOTHING;

            DELETE FROM _auth.role_permission
            WHERE permission_code IN ('employee-read', 'employee-update');

            DELETE FROM _auth.permission
            WHERE permission_code IN ('employee-read', 'employee-update');
        ");
    }
}
