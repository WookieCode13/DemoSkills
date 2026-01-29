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

    public async Task AddEmployeeAsync(Employee employee, CancellationToken ct)
    {
        await _db.Employees.AddAsync(employee, ct);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateEmployeeAsync(Employee employee, CancellationToken ct)
    {
        _db.Employees.Update(employee);
        await _db.SaveChangesAsync(ct);
    }

    /// <summary>
    /// Soft delete an employee by setting DeletedUtc.
    /// </summary>    
    public async Task DeleteEmployeeAsync(Guid id, CancellationToken ct)
    {
        var employee = await _db.Employees.FirstOrDefaultAsync(e => e.Id == id && e.DeletedUtc == null, ct);
        if (employee != null)
        {
            employee.DeletedUtc = DateTime.UtcNow;
            _db.Employees.Update(employee);
            await _db.SaveChangesAsync(ct);
        }
    }
}
