using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace EmployeeAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmployeesController : ControllerBase
{
    private readonly ILogger<EmployeesController> _logger;

    public EmployeesController(ILogger<EmployeesController> logger)
    {
        _logger = logger;
    }
    [HttpGet("health")]
    public IActionResult Health()
    {
        _logger.LogInformation("Health check requested from {RemoteIp}", HttpContext.Connection.RemoteIpAddress?.ToString());
        var payload = new
        {
            status = "ok",
            service = "EmployeeAPI",
            timestamp = DateTime.UtcNow.ToString("o")
        };
        _logger.LogInformation("Health check OK at {Timestamp}", payload.timestamp);
        return Ok(payload);
    }
}
