using PayAPI.Domain.Entities;

namespace PayAPI.Application.Auditing;

public interface IAuditLogRepository
{
    Task AddAsync(AuditLog entry, CancellationToken ct);
}
