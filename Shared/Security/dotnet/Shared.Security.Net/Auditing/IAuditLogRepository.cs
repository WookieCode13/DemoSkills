namespace Shared.Security.Net.Auditing;

public interface IAuditLogRepository
{
    Task AddAsync(AuditLog entry, CancellationToken ct);
}
