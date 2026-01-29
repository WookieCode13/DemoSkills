using EmployeeAPI.Domain.Entities;

namespace EmployeeAPI.Application.Employees;

public interface IEmployeeRepository
{
    Task<List<Employee>> GetEmployeesAsync(CancellationToken ct);
    Task<Employee?> GetEmployeeByIdAsync(Guid id, CancellationToken ct);
    Task AddEmployeeAsync(Employee employee, CancellationToken ct);
    Task UpdateEmployeeAsync(Employee employee, CancellationToken ct);
    Task DeleteEmployeeAsync(Guid id, CancellationToken ct);
}
