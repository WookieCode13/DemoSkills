using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using TaxCalculatorAPI.Domain.Entities;

namespace TaxCalculatorAPI.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

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

    public DbSet<AuditLog> AuditLogs { get; set; } = null!;
}
