using EmployeeAPI.Domain.Entities;

namespace EmployeeAPI.Application.Employees;

public interface IEmployeeRepository
{
    Task<List<Employee>> GetEmployeesAsync(CancellationToken ct);
    Task<Employee?> GetEmployeeByIdAsync(Guid id, CancellationToken ct);
    Task<Employee?> GetForUpdateAsync(Guid id, CancellationToken ct);
    Task AddAsync(Employee employee, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}
