using FluentMigrator;
using Serilog;

namespace EmployeeAPI.Migrations;

// Shared database migration executed from EmployeeAPI for operational simplicity.
[Migration(2026030901, "Create shared _auth schema and core authorization tables")]
public sealed class V2026030901_CreateSharedAuthSchema : Migration
{
    public override void Up()
    {
        Log.Information("Creating shared _auth schema and core authorization tables");

        Execute.Sql(@"CREATE EXTENSION IF NOT EXISTS ""uuid-ossp"";");
        Execute.Sql(@"CREATE SCHEMA IF NOT EXISTS _auth;");

        Execute.Sql(@"
            CREATE TABLE IF NOT EXISTS _auth.app_user (
                app_user_id uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
                cognito_sub varchar(255) NOT NULL,
                email varchar(255) NULL,
                base_role_level integer NOT NULL,
                global_role_code varchar(100) NULL,
                is_active boolean NOT NULL DEFAULT true,
                created_utc timestamptz NOT NULL DEFAULT timezone('utc', now()),
                updated_utc timestamptz NOT NULL DEFAULT timezone('utc', now())
            );
        ");

        Execute.Sql(@"
            CREATE TABLE IF NOT EXISTS _auth.role (
                role_code varchar(100) PRIMARY KEY,
                role_name varchar(200) NOT NULL,
                description text NULL,
                is_active boolean NOT NULL DEFAULT true,
                created_utc timestamptz NOT NULL DEFAULT timezone('utc', now()),
                updated_utc timestamptz NOT NULL DEFAULT timezone('utc', now())
            );
        ");

        Execute.Sql(@"
            CREATE TABLE IF NOT EXISTS _auth.permission (
                permission_code varchar(100) PRIMARY KEY,
                permission_name varchar(200) NOT NULL,
                description text NULL,
                system_code varchar(100) NOT NULL,
                resource_code varchar(100) NOT NULL,
                can_create boolean NOT NULL DEFAULT false,
                can_read boolean NOT NULL DEFAULT false,
                can_update boolean NOT NULL DEFAULT false,
                can_delete boolean NOT NULL DEFAULT false,
                is_active boolean NOT NULL DEFAULT true,
                created_utc timestamptz NOT NULL DEFAULT timezone('utc', now()),
                updated_utc timestamptz NOT NULL DEFAULT timezone('utc', now())
            );
        ");

        Execute.Sql(@"
            CREATE TABLE IF NOT EXISTS _auth.user_company_access (
                user_company_access_id uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
                app_user_id uuid NOT NULL,
                company_id uuid NOT NULL,
                company_role_code varchar(100) NOT NULL,
                is_active boolean NOT NULL DEFAULT true,
                created_utc timestamptz NOT NULL DEFAULT timezone('utc', now()),
                updated_utc timestamptz NOT NULL DEFAULT timezone('utc', now()),
                CONSTRAINT fk_user_company_access_app_user
                    FOREIGN KEY (app_user_id) REFERENCES _auth.app_user (app_user_id),
                CONSTRAINT fk_user_company_access_company
                    FOREIGN KEY (company_id) REFERENCES company (id),
                CONSTRAINT fk_user_company_access_role
                    FOREIGN KEY (company_role_code) REFERENCES _auth.role (role_code)
            );
        ");

        Execute.Sql(@"
            CREATE TABLE IF NOT EXISTS _auth.role_permission (
                role_code varchar(100) NOT NULL,
                permission_code varchar(100) NOT NULL,
                created_utc timestamptz NOT NULL DEFAULT timezone('utc', now()),
                CONSTRAINT pk_role_permission PRIMARY KEY (role_code, permission_code),
                CONSTRAINT fk_role_permission_role
                    FOREIGN KEY (role_code) REFERENCES _auth.role (role_code),
                CONSTRAINT fk_role_permission_permission
                    FOREIGN KEY (permission_code) REFERENCES _auth.permission (permission_code)
            );
        ");

        Execute.Sql(@"
            ALTER TABLE _auth.app_user
            ADD CONSTRAINT fk_app_user_global_role
            FOREIGN KEY (global_role_code) REFERENCES _auth.role (role_code);
        ");

        Execute.Sql(@"
            CREATE UNIQUE INDEX IF NOT EXISTS ux_auth_app_user_cognito_sub
            ON _auth.app_user (cognito_sub);
        ");

        Execute.Sql(@"
            CREATE INDEX IF NOT EXISTS ix_auth_app_user_email
            ON _auth.app_user (email);
        ");

        Execute.Sql(@"
            CREATE UNIQUE INDEX IF NOT EXISTS ux_auth_user_company_access_user_company
            ON _auth.user_company_access (app_user_id, company_id);
        ");

        Execute.Sql(@"
            CREATE INDEX IF NOT EXISTS ix_auth_user_company_access_company_id
            ON _auth.user_company_access (company_id);
        ");

        Execute.Sql(@"
            CREATE INDEX IF NOT EXISTS ix_auth_permission_system_resource
            ON _auth.permission (system_code, resource_code);
        ");
    }

    public override void Down()
    {
        Log.Information("Dropping shared _auth schema and core authorization tables");

        Execute.Sql("DROP INDEX IF EXISTS _auth.ix_auth_permission_system_resource;");
        Execute.Sql("DROP INDEX IF EXISTS _auth.ix_auth_user_company_access_company_id;");
        Execute.Sql("DROP INDEX IF EXISTS _auth.ux_auth_user_company_access_user_company;");
        Execute.Sql("DROP INDEX IF EXISTS _auth.ix_auth_app_user_email;");
        Execute.Sql("DROP INDEX IF EXISTS _auth.ux_auth_app_user_cognito_sub;");

        Execute.Sql("DROP TABLE IF EXISTS _auth.role_permission;");
        Execute.Sql("DROP TABLE IF EXISTS _auth.user_company_access;");
        Execute.Sql("DROP TABLE IF EXISTS _auth.permission;");
        Execute.Sql("DROP TABLE IF EXISTS _auth.app_user;");
        Execute.Sql("DROP TABLE IF EXISTS _auth.role;");
        Execute.Sql("DROP SCHEMA IF EXISTS _auth;");
    }
}
