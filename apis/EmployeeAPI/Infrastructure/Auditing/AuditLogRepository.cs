using EmployeeAPI.Application.Auditing;
using EmployeeAPI.Domain.Entities;
using EmployeeAPI.Infrastructure.Data;

namespace EmployeeAPI.Infrastructure.Auditing;

public class AuditLogRepository : IAuditLogRepository
{
    private readonly AppDbContext _db;

    public AuditLogRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task AddAsync(AuditLog entry, CancellationToken ct)
    {
        _db.AuditLogs.Add(entry);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken ct)
    {
        return _db.SaveChangesAsync(ct);
    }
}
