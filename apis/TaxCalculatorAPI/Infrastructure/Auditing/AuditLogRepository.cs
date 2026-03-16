using TaxCalculatorAPI.Infrastructure.Data;
using Shared.Security.Net.Auditing;

namespace TaxCalculatorAPI.Infrastructure.Auditing;

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
