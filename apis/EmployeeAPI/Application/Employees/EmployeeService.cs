using EmployeeAPI.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace EmployeeAPI.Application.Employees;

public class EmployeeService
{
    private readonly ILogger<EmployeeService> _logger;
    private readonly IEmployeeRepository _employeeRepository;

    public EmployeeService(ILogger<EmployeeService> logger, IEmployeeRepository employeeRepository)
    {
        _logger = logger;
        _employeeRepository = employeeRepository;
    }

    public Task<List<Employee>> GetAllAsync(CancellationToken ct) => _employeeRepository.GetEmployeesAsync(ct);
    public Task<Employee?> GetByIdAsync(Guid id, CancellationToken ct) => _employeeRepository.GetEmployeeByIdAsync(id, ct);

    public Task AddAsync(Employee employee, CancellationToken ct) => _employeeRepository.AddEmployeeAsync(employee, ct);
    public Task DeleteAsync(Guid id, CancellationToken ct) => _employeeRepository.DeleteEmployeeAsync(id, ct);
    public Task UpdateAsync(Employee employee, CancellationToken ct) => _employeeRepository.UpdateEmployeeAsync(employee, ct);

}
