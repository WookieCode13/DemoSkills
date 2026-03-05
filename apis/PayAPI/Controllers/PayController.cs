using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using PayAPI.Contracts;

namespace PayAPI.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class PayController : ControllerBase
{
    private readonly ILogger<PayController> _logger;

    public PayController(ILogger<PayController> logger)
    {
        _logger = logger;
    }

    [HttpGet("health")]
    public IActionResult Health()
    {
        _logger.LogInformation("Health check requested from {RemoteIp}", HttpContext.Connection.RemoteIpAddress?.ToString());
        var payload = new HealthResponse(
            Status: "ok",
            Service: "PayAPI",
            Timestamp: DateTime.UtcNow.ToString("o")
        );
        _logger.LogInformation("Health check OK at {Timestamp}", payload.Timestamp);
        return Ok(payload);
    }

    [HttpGet]
    [Authorize]
    public IActionResult List()
    {
        return Ok(Array.Empty<object>());
    }
}
