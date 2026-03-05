using PayAPI.Application.Auditing;
using PayAPI.Domain.Entities;
using PayAPI.Infrastructure.Data;

namespace PayAPI.Infrastructure.Auditing;

public class AuditLogRepository : IAuditLogRepository
{
    private readonly AppDbContext _db;

    public AuditLogRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task AddAsync(AuditLog entry, CancellationToken ct)
    {
        // Wrapper for staging an audit row; service controls SaveChanges/transaction.
        _db.AuditLogs.Add(entry);
        return Task.CompletedTask;
    }
}
