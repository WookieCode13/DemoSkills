using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PayAPI.Application.Taxes;
using PayAPI.Contracts;
using PayAPI.Contracts.Taxes;
using PayAPI.Mappings;

namespace PayAPI.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class TaxesController : ControllerBase
{
    private readonly ILogger<TaxesController> _logger;
    private readonly ITaxRepository _taxRepository;

    public TaxesController(ILogger<TaxesController> logger, ITaxRepository taxRepository)
    {
        _logger = logger;
        _taxRepository = taxRepository;
    }

    [HttpGet("health")]
    public IActionResult Health()
    {
        _logger.LogInformation("Tax health check requested from {RemoteIp}", HttpContext.Connection.RemoteIpAddress?.ToString());
        var payload = new HealthResponse(
            Status: "ok",
            Service: "PayAPI.Tax",
            Timestamp: DateTime.UtcNow.ToString("o")
        );
        return Ok(payload);
    }

    [HttpGet]
    [Authorize(Policy = "TaxRead")]
    [ProducesResponseType(typeof(List<TaxResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<TaxResponse>>> List(CancellationToken ct)
    {
        var taxes = await _taxRepository.GetTaxesAsync(ct);
        return Ok(taxes.Select(t => t.ToResponse()).ToList());
    }
}
