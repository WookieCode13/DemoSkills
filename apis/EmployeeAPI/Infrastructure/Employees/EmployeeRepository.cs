using EmployeeAPI.Application.Employees;
using EmployeeAPI.Domain.Entities;
using EmployeeAPI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EmployeeAPI.Infrastructure.Employees;

public class EmployeeRepository : IEmployeeRepository
{
    private readonly AppDbContext _db;

    public EmployeeRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<List<Employee>> GetEmployeesAsync(CancellationToken ct)
    {
        return _db.Employees
            .AsNoTracking()
            .Where(e => e.DeletedUtc == null)
            .OrderBy(e => e.LastName)
            .ThenBy(e => e.FirstName)
            .ToListAsync(ct);
    }

    public Task<Employee?> GetEmployeeByIdAsync(Guid id, CancellationToken ct)
    {
        return _db.Employees
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id && e.DeletedUtc == null, ct);
    }

    public Task<Employee?> GetForUpdateAsync(Guid id, CancellationToken ct)
    {
        return _db.Employees
            .FirstOrDefaultAsync(e => e.Id == id && e.DeletedUtc == null, ct);
    }

    public Task AddAsync(Employee employee, CancellationToken ct)
    {
        // Wrapper for staging a new employee; service controls SaveChanges/transaction.
        _db.Employees.Add(employee);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync(CancellationToken ct)
    {
        await _db.SaveChangesAsync(ct);
    }
}
