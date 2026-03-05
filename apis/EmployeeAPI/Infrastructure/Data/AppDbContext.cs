using System.Text.Json;
using EmployeeAPI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Shared.Security.Net.Auditing;

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
    }

    public DbSet<Employee> Employees { get; set; } = null!;
    public DbSet<AuditLog> AuditLogs { get; set; } = null!;
}
