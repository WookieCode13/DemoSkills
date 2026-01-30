using EmployeeAPI.Contracts.Employees;
using EmployeeAPI.Domain.Entities;
using EmployeeAPI.Mappings;
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

    public async Task<EmployeeResponse> CreateAsync(CreateEmployeeRequest request, CancellationToken ct)
    {
        var employee = new Employee
        {
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = request.Email.Trim(),              
            Phone = request.Phone,
            DateOfBirth = request.DateOfBirth,
            DeletedUtc = null,
            SSN = request.SSN.Trim()
        };
        await _employeeRepository.AddAsync(employee, ct);   // stage
        await _employeeRepository.SaveChangesAsync(ct);     // commit

        return employee.ToResponse();
    }
    
    public async Task<PatchResult> PatchAsync(Guid id, PatchEmployeeRequest request, CancellationToken ct)
    {
        var employee = await _employeeRepository.GetForUpdateAsync(id, ct); // tracked entity
        if (employee is null) return PatchResult.NotFound;

        if (request.Email is not null) employee.Email = request.Email.Trim();
        if (request.Phone is not null) employee.Phone = request.Phone;
        if (request.FirstName is not null) employee.FirstName = request.FirstName.Trim();
        if (request.LastName is not null) employee.LastName = request.LastName.Trim();
        if (request.DateOfBirth.HasValue) employee.DateOfBirth = request.DateOfBirth;
        
        await _employeeRepository.SaveChangesAsync( ct); // commits changes EF tracked
        return PatchResult.Updated;
    }
    
    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct)
    {
        var employee = await _employeeRepository.GetForUpdateAsync(id, ct);
        if (employee is null) return false;

        employee.DeletedUtc = DateTime.UtcNow;
        
        await _employeeRepository.SaveChangesAsync(ct);
        return true;
    }

}
