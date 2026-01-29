using EmployeeAPI.Application.Employees;
using EmployeeAPI.Contracts;
using EmployeeAPI.Contracts.Employees;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using EmployeeAPI.Mappings;

namespace EmployeeAPI.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class EmployeesController : ControllerBase
{
    private readonly ILogger<EmployeesController> _logger;
    private readonly EmployeeService _employeeService;

    public EmployeesController(ILogger<EmployeesController> logger, EmployeeService employeeService)
    {
        _logger = logger;
        _employeeService = employeeService;
    }

    [HttpGet("health")]
    public IActionResult Health()
    {
        _logger.LogInformation(
            "Health check requested from {RemoteIp}",
            HttpContext.Connection.RemoteIpAddress?.ToString());
        var payload = new HealthResponse(
            Status: "ok",
            Service: "EmployeeAPI",
            Timestamp: DateTime.UtcNow.ToString("o")
        );
        _logger.LogInformation("Health check OK at {Timestamp}", payload.Timestamp);
        return Ok(payload);
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<EmployeeResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<EmployeeResponse>>> Employees(CancellationToken ct)
    {
        _logger.LogInformation("GET /employees requested");

        var employees = await _employeeService.GetAllAsync(ct);

        return Ok(employees
            .Select(e => e.ToResponse())
            .ToList());
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(EmployeeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<EmployeeResponse>> GetByIdAsync(Guid id, CancellationToken ct)
    {
        _logger.LogInformation("GET /employees/{EmployeeId} requested", id);

        var employee = await _employeeService.GetByIdAsync(id, ct);

        if (employee is null)
        {
            _logger.LogWarning("Employee {EmployeeId} not found", id);
            return NotFound();
        }

        return Ok(employee.ToResponse());
    }
}
