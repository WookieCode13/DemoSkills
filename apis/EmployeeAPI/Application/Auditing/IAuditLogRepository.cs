using EmployeeAPI.Domain.Entities;

namespace EmployeeAPI.Application.Auditing;

public interface IAuditLogRepository
{
    Task AddAsync(AuditLog entry, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}
