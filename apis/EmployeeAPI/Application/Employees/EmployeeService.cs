using EmployeeAPI.Contracts.Employees;
using EmployeeAPI.Domain.Entities;
using EmployeeAPI.Mappings;
using EmployeeAPI.Application.Auditing;
using Microsoft.Extensions.Logging;
using EmployeeAPI.Application.Logging;

namespace EmployeeAPI.Application.Employees;

public class EmployeeService
{
    private readonly ILogger<EmployeeService> _logger;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IAuditLogRepository _auditLogRepository;

    public EmployeeService(
        ILogger<EmployeeService> logger,
        IEmployeeRepository employeeRepository,
        IAuditLogRepository auditLogRepository)
    {
        _logger = logger;
        _employeeRepository = employeeRepository;
        _auditLogRepository = auditLogRepository;
    }

    public Task<List<Employee>> GetAllAsync(CancellationToken ct) => _employeeRepository.GetEmployeesAsync(ct);
    
    public Task<Employee?> GetByIdAsync(Guid id, CancellationToken ct) => _employeeRepository.GetEmployeeByIdAsync(id, ct);

    public async Task<EmployeeResponse> CreateAsync(CreateEmployeeRequest request, CancellationToken ct)
    {
        _logger.LogInformation("Creating employee {Email}", LogRedaction.MaskEmail(request.Email));
        var employee = new Employee
        {
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = request.Email.Trim(),              
            Phone = request.Phone,
            DateOfBirth = request.DateOfBirth,
            CreatedUtc = DateTime.UtcNow,
            UpdatedUtc = DateTime.UtcNow,
            DeletedUtc = null,
            SSN = request.SSN.Trim()
        };
        await _employeeRepository.AddAsync(employee, ct);   // stage
        await _employeeRepository.SaveChangesAsync(ct);     // commit

        await WriteAuditLogAsync(
            employee.Id,
            "created",
            changedFields: null,
            ct);

        _logger.LogInformation("Created employee {EmployeeId}", employee.Id);

        return employee.ToResponse();
    }
    
    public async Task<PatchResult> PatchAsync(Guid id, PatchEmployeeRequest request, CancellationToken ct)
    {
        var employee = await _employeeRepository.GetForUpdateAsync(id, ct); // tracked entity
        if (employee is null)
        {
            _logger.LogWarning("Patch skipped; employee {EmployeeId} not found", id);
            return PatchResult.NotFound;
        }

        var changedFields = new List<string>();
        MapChangedFields(request, employee, changedFields);

        if (changedFields.Count == 0)
        {
            _logger.LogInformation("No changes detected for employee {EmployeeId}", id);
            return PatchResult.NoChanges;
        }

        employee.UpdatedUtc = DateTime.UtcNow;

        await _employeeRepository.SaveChangesAsync(ct); // commits changes EF tracked

        await WriteAuditLogAsync(
            employee.Id,
            "updated",
            changedFields,
            ct);

        _logger.LogInformation("Updated employee {EmployeeId} ({Fields})", id, string.Join(",", changedFields));
        return PatchResult.Updated;

        static void MapChangedFields(PatchEmployeeRequest request, Employee employee, List<string> changedFields)
        {
            if (request.Email is not null && request.Email.Trim() != employee.Email)
            {
                employee.Email = request.Email.Trim();
                changedFields.Add(nameof(employee.Email));
            }
            if (request.Phone is not null && request.Phone != employee.Phone)
            {
                employee.Phone = request.Phone;
                changedFields.Add(nameof(employee.Phone));
            }
            if (request.FirstName is not null && request.FirstName.Trim() != employee.FirstName)
            {
                employee.FirstName = request.FirstName.Trim();
                changedFields.Add(nameof(employee.FirstName));
            }
            if (request.LastName is not null && request.LastName.Trim() != employee.LastName)
            {
                employee.LastName = request.LastName.Trim();
                changedFields.Add(nameof(employee.LastName));
            }
            if (request.DateOfBirth.HasValue && request.DateOfBirth != employee.DateOfBirth)
            {
                employee.DateOfBirth = request.DateOfBirth;
                changedFields.Add(nameof(employee.DateOfBirth));
            }
        }
    }


    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct)
    {
        var employee = await _employeeRepository.GetForUpdateAsync(id, ct);
        if (employee is null)
        {
            _logger.LogWarning("Delete skipped; employee {EmployeeId} not found", id);
            return false;
        }

        employee.DeletedUtc = DateTime.UtcNow;
        employee.UpdatedUtc = DateTime.UtcNow;
        
        await _employeeRepository.SaveChangesAsync(ct);

        await WriteAuditLogAsync(
            employee.Id,
            "deleted",
            changedFields: null,
            ct);

        _logger.LogInformation("Deleted employee {EmployeeId}", id);
        return true;
    }

    private async Task WriteAuditLogAsync(
        Guid entityId,
        string action,
        IEnumerable<string>? changedFields,
        CancellationToken ct)
    {
        await _auditLogRepository.AddAsync(new AuditLog
        {
            EntityType = "EmployeeAPI.Employee",
            EntityId = entityId,
            Action = action,
            OccurredUtc = DateTimeOffset.UtcNow,
            PerformedBy = "system",
            ChangedFields = changedFields is null ? null : changedFields.ToList()
        }, ct);
        await _auditLogRepository.SaveChangesAsync(ct);
    }

}
