using TaxCalculatorAPI.Domain.Entities;

namespace TaxCalculatorAPI.Application.Auditing;

public interface IAuditLogRepository
{
    Task AddAsync(AuditLog entry, CancellationToken ct);
}
