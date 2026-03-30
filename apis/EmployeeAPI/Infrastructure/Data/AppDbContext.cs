using System.Text.Json;
using EmployeeAPI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Shared.Security.Net.Auditing;
using Shared.Security.Net.Auth.Domain;

namespace EmployeeAPI.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Employee>(entity =>
        {
            entity.ToTable("employee");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.FirstName).HasColumnName("first_name");
            entity.Property(e => e.LastName).HasColumnName("last_name");
            entity.Property(e => e.Email).HasColumnName("email");
            entity.Property(e => e.Phone).HasColumnName("phone");
            entity.Property(e => e.SSN).HasColumnName("ssn");
            entity.Property(e => e.DateOfBirth).HasColumnName("date_of_birth").HasColumnType("date");
            entity.Property(e => e.CreatedUtc).HasColumnName("created_utc");
            entity.Property(e => e.UpdatedUtc).HasColumnName("updated_utc");
            entity.Property(e => e.DeletedUtc).HasColumnName("deleted_utc");
        });

        modelBuilder.Entity<Company>(entity =>
        {
            entity.ToTable("company");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ShortCode).HasColumnName("short_code");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.Industry).HasColumnName("industry");
            entity.Property(e => e.Email).HasColumnName("email");
            entity.Property(e => e.Phone).HasColumnName("phone");
            entity.Property(e => e.CreatedUtc).HasColumnName("created_utc");
            entity.Property(e => e.UpdatedUtc).HasColumnName("updated_utc");
            entity.Property(e => e.DeletedUtc).HasColumnName("deleted_utc");
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.ToTable("audit_log");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.EntityType).HasColumnName("entity_type");
            entity.Property(e => e.EntityId).HasColumnName("entity_id");
            entity.Property(e => e.Action).HasColumnName("action");
            entity.Property(e => e.OccurredUtc).HasColumnName("occurred_utc");
            entity.Property(e => e.PerformedBy).HasColumnName("performed_by");
            entity.Property(e => e.ChangedFields)
                .HasColumnName("changed_fields")
                .HasColumnType("jsonb")
                .HasConversion(
                    value => value == null ? null : JsonSerializer.Serialize(value, (JsonSerializerOptions?)null),
                    value => value == null ? null : JsonSerializer.Deserialize<List<string>>(value, (JsonSerializerOptions?)null));
            entity.Property(e => e.Changes)
                .HasColumnName("changes")
                .HasColumnType("jsonb")
                .HasConversion(
                    value => value == null ? null : JsonSerializer.Serialize(value, (JsonSerializerOptions?)null),
                    value => value == null ? null : JsonSerializer.Deserialize<Dictionary<string, object?>>(value, (JsonSerializerOptions?)null));
            entity.Property(e => e.Note).HasColumnName("note");
            entity.Property(e => e.CorrelationId).HasColumnName("correlation_id");
        });

        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.ToTable("app_user", "_auth");
            entity.HasKey(e => e.AppUserId);
            entity.Property(e => e.AppUserId).HasColumnName("app_user_id");
            entity.Property(e => e.CognitoSub).HasColumnName("cognito_sub");
            entity.Property(e => e.Email).HasColumnName("email");
            entity.Property(e => e.BaseRoleLevel).HasColumnName("base_role_level");
            entity.Property(e => e.GlobalRoleCode).HasColumnName("global_role_code");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.CreatedUtc).HasColumnName("created_utc");
            entity.Property(e => e.UpdatedUtc).HasColumnName("updated_utc");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("role", "_auth");
            entity.HasKey(e => e.RoleCode);
            entity.Property(e => e.RoleCode).HasColumnName("role_code");
            entity.Property(e => e.RoleName).HasColumnName("role_name");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.CreatedUtc).HasColumnName("created_utc");
            entity.Property(e => e.UpdatedUtc).HasColumnName("updated_utc");
        });

        modelBuilder.Entity<Permission>(entity =>
        {
            entity.ToTable("permission", "_auth");
            entity.HasKey(e => e.PermissionCode);
            entity.Property(e => e.PermissionCode).HasColumnName("permission_code");
            entity.Property(e => e.PermissionName).HasColumnName("permission_name");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.SystemCode).HasColumnName("system_code");
            entity.Property(e => e.ResourceCode).HasColumnName("resource_code");
            entity.Property(e => e.CanCreate).HasColumnName("can_create");
            entity.Property(e => e.CanRead).HasColumnName("can_read");
            entity.Property(e => e.CanUpdate).HasColumnName("can_update");
            entity.Property(e => e.CanDelete).HasColumnName("can_delete");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.CreatedUtc).HasColumnName("created_utc");
            entity.Property(e => e.UpdatedUtc).HasColumnName("updated_utc");
        });

        modelBuilder.Entity<RolePermission>(entity =>
        {
            entity.ToTable("role_permission", "_auth");
            entity.HasKey(e => new { e.RoleCode, e.PermissionCode });
            entity.Property(e => e.RoleCode).HasColumnName("role_code");
            entity.Property(e => e.PermissionCode).HasColumnName("permission_code");
            entity.Property(e => e.CreatedUtc).HasColumnName("created_utc");
        });

        modelBuilder.Entity<UserCompanyAccess>(entity =>
        {
            entity.ToTable("user_company_access", "_auth");
            entity.HasKey(e => e.UserCompanyAccessId);
            entity.Property(e => e.UserCompanyAccessId).HasColumnName("user_company_access_id");
            entity.Property(e => e.AppUserId).HasColumnName("app_user_id");
            entity.Property(e => e.CompanyId).HasColumnName("company_id");
            entity.Property(e => e.CompanyRoleCode).HasColumnName("company_role_code");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.CreatedUtc).HasColumnName("created_utc");
            entity.Property(e => e.UpdatedUtc).HasColumnName("updated_utc");
        });
    }

    public DbSet<Employee> Employees { get; set; } = null!;
    public DbSet<Company> Companies { get; set; } = null!;
    public DbSet<AuditLog> AuditLogs { get; set; } = null!;

    public DbSet<AppUser> AuthAppUsers { get; set; } = null!;
    public DbSet<Role> AuthRoles { get; set; } = null!;
    public DbSet<Permission> AuthPermissions { get; set; } = null!;
    public DbSet<RolePermission> AuthRolePermissions { get; set; } = null!;
    public DbSet<UserCompanyAccess> AuthUserCompanyAccess { get; set; } = null!;
}

