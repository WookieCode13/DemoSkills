using Microsoft.AspNetCore.Mvc;
using TaxCalculatorAPI.Models;

namespace TaxCalculatorAPI.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class TaxesController : ControllerBase
{
    private readonly ILogger<TaxesController> _logger;

    public TaxesController(ILogger<TaxesController> logger)
    {
        _logger = logger;
    }

    [HttpGet("health")]
    public IActionResult Health()
    {
        _logger.LogInformation("Health check requested from {RemoteIp}", HttpContext.Connection.RemoteIpAddress?.ToString());
        var payload = new HealthResponse(
            Status: "ok",
            Service: "TaxCalculatorAPI",
            Timestamp: DateTime.UtcNow.ToString("o")
        );
        _logger.LogInformation("Health check OK at {Timestamp}", payload.Timestamp);
        return Ok(payload);
    }
}
