using EmployeeAPI.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EmployeeAPI.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Employee>().ToTable("employee");
        modelBuilder.Entity<AuditLog>().ToTable("audit_log");
    }

    public DbSet<Employee> Employees { get; set; } = null!;
    public DbSet<AuditLog> AuditLogs { get; set; } = null!;
}
