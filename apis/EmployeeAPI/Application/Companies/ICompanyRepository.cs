using EmployeeAPI.Domain.Entities;

namespace EmployeeAPI.Application.Companies;

public interface ICompanyRepository
{
    Task<List<Company>> GetCompaniesAsync(CancellationToken ct);
    Task<Company?> GetCompanyByIdAsync(Guid id, CancellationToken ct);
    Task<Company?> GetCompanyByShortCodeAsync(string shortCode, CancellationToken ct);
    Task<Company?> GetForUpdateAsync(Guid id, CancellationToken ct);
    Task AddAsync(Company company, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}
