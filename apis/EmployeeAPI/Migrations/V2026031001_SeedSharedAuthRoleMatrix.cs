using FluentMigrator;
using Serilog;

namespace EmployeeAPI.Migrations;

// Shared database migration executed from EmployeeAPI for operational simplicity.
[Migration(2026031001, "Seed shared _auth role and permission matrix")]
public sealed class V2026031001_SeedSharedAuthRoleMatrix : Migration
{
    public override void Up()
    {
        Log.Information("Seeding shared _auth role and permission matrix");

        Execute.Sql(@"
            DELETE FROM _auth.role_permission;
            DELETE FROM _auth.permission;
            DELETE FROM _auth.role;
        ");

        Execute.Sql(@"
            INSERT INTO _auth.role (role_code, role_name, description, is_active)
            VALUES
                ('super_admin', 'Super Admin', 'Full cross-company admin', true),
                ('customer_service_create', 'Customer Service Create', 'Global create support role', true),
                ('customer_service', 'Customer Service', 'Assigned-company support role', true),
                ('payroll_admin', 'Payroll Admin', 'Payroll management role', true),
                ('employee_self_service', 'Employee Self Service', 'Employee self-service role', true);
        ");

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
                ('company-create', 'Company Create', 'Create company data', 'company_api', 'company', true, false, false, false, true),
                ('company-read', 'Company Read', 'Read company data', 'company_api', 'company', false, true, false, false, true),
                ('company-update', 'Company Update', 'Update company data', 'company_api', 'company', false, true, true, false, true),
                ('company-delete', 'Company Delete', 'Soft delete company', 'company_api', 'company', false, false, false, true, true),
                ('pay-view', 'Pay View', 'Read payroll data', 'pay_api', 'payroll', false, true, false, false, true),
                ('pay-create', 'Pay Create', 'Create payroll data', 'pay_api', 'payroll', true, false, false, false, true),
                ('pay-process', 'Pay Process', 'Process payroll data', 'pay_api', 'payroll', false, true, true, false, true),
                ('pay-close', 'Pay Close', 'Close payroll period', 'pay_api', 'payroll', false, true, true, false, true),
                ('pay-delete', 'Pay Delete', 'Delete payroll data', 'pay_api', 'payroll', false, false, false, true, true),
                ('employee-profile-read', 'Employee Profile Read', 'Read employee profile data', 'employee_api', 'profile', false, true, false, false, true),
                ('employee-profile-update', 'Employee Profile Update', 'Update employee profile data', 'employee_api', 'profile', false, true, true, false, true),
                ('employee-delete', 'Employee Delete', 'Delete employee record', 'employee_api', 'employee', false, false, false, true, true),
                ('tax-read', 'Tax Read', 'Read tax calculation data', 'tax_api', 'tax', false, true, false, false, true),
                ('report-read', 'Report Read', 'Read report data', 'report_api', 'report', false, true, false, false, true);
        ");

        Execute.Sql(@"
            INSERT INTO _auth.role_permission (role_code, permission_code)
            VALUES
                ('super_admin', 'company-read'),
                ('super_admin', 'company-update'),
                ('super_admin', 'company-delete'),
                ('super_admin', 'company-create'),
                ('super_admin', 'pay-view'),
                ('super_admin', 'pay-create'),
                ('super_admin', 'pay-process'),
                ('super_admin', 'pay-close'),
                ('super_admin', 'pay-delete'),
                ('super_admin', 'employee-profile-read'),
                ('super_admin', 'employee-profile-update'),
                ('super_admin', 'employee-delete'),
                ('super_admin', 'tax-read'),
                ('super_admin', 'report-read'),
                ('customer_service_create', 'company-create'),
                ('customer_service_create', 'company-read'),
                ('customer_service_create', 'employee-profile-read'),
                ('customer_service', 'company-update'),
                ('customer_service', 'employee-profile-read'),
                ('payroll_admin', 'pay-view'),
                ('payroll_admin', 'pay-create'),
                ('payroll_admin', 'pay-process'),
                ('payroll_admin', 'pay-close'),
                ('payroll_admin', 'report-read'),
                ('employee_self_service', 'employee-profile-read'),
                ('employee_self_service', 'employee-profile-update');
        ");
    }

    public override void Down()
    {
        Log.Information("Removing seeded shared _auth role and permission matrix");

        Execute.Sql(@"
            DELETE FROM _auth.role_permission;
            DELETE FROM _auth.permission;
            DELETE FROM _auth.role;
        ");
    }
}
