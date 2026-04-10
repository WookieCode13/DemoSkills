using EmployeeAPI.Application.Companies;
using EmployeeAPI.Domain.Entities;
using EmployeeAPI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EmployeeAPI.Infrastructure.Companies;

public class CompanyRepository : ICompanyRepository
{
    private readonly AppDbContext _db;

    public CompanyRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<List<Company>> GetCompaniesAsync(CancellationToken ct)
    {
        return _db.Companies
            .AsNoTracking()
            .Where(c => c.DeletedUtc == null)
            .OrderBy(c => c.Name)
            .ToListAsync(ct);
    }

    public Task<Company?> GetCompanyByIdAsync(Guid id, CancellationToken ct)
    {
        return _db.Companies
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id && c.DeletedUtc == null, ct);
    }

    public Task<Company?> GetCompanyByShortCodeAsync(string shortCode, CancellationToken ct)
    {
        return _db.Companies
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.ShortCode == shortCode && c.DeletedUtc == null, ct);
    }

    public Task<Company?> GetForUpdateAsync(Guid id, CancellationToken ct)
    {
        return _db.Companies
            .FirstOrDefaultAsync(c => c.Id == id && c.DeletedUtc == null, ct);
    }

    public Task AddAsync(Company company, CancellationToken ct)
    {
        _db.Companies.Add(company);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync(CancellationToken ct)
    {
        await _db.SaveChangesAsync(ct);
    }
}
