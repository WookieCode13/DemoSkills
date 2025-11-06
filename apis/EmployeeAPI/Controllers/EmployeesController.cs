using Microsoft.AspNetCore.Mvc;

namespace EmployeeAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmployeesController : ControllerBase
{
    [HttpGet("health")]
    public IActionResult Health()
    {
        var payload = new
        {
            status = "ok",
            service = "EmployeeAPI",
            timestamp = DateTime.UtcNow.ToString("o")
        };
        return Ok(payload);
    }
}

