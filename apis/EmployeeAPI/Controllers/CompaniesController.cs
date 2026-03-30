using EmployeeAPI.Application.Companies;
using EmployeeAPI.Contracts;
using EmployeeAPI.Contracts.Companies;
using EmployeeAPI.Mappings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeAPI.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class CompaniesController : ControllerBase
{
    private readonly ILogger<CompaniesController> _logger;
    private readonly CompanyService _companyService;

    public CompaniesController(ILogger<CompaniesController> logger, CompanyService companyService)
    {
        _logger = logger;
        _companyService = companyService;
    }

    [HttpGet("health")]
    public IActionResult Health()
    {
        _logger.LogInformation("Company health check requested from {RemoteIp}", HttpContext.Connection.RemoteIpAddress?.ToString());
        return Ok(new HealthResponse("ok", "EmployeeAPI.Company", DateTime.UtcNow.ToString("o")));
    }

    [HttpGet]
    [Authorize(Policy = "CompanyRead")]
    [ProducesResponseType(typeof(List<CompanyResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<CompanyResponse>>> List(CancellationToken ct)
    {
        var companies = await _companyService.GetAllAsync(ct);
        return Ok(companies.Select(c => c.ToResponse()).ToList());
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "CompanyRead")]
    [ProducesResponseType(typeof(CompanyResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CompanyResponse>> GetById(Guid id, CancellationToken ct)
    {
        var company = await _companyService.GetByIdAsync(id, ct);
        return company is null ? NotFound() : Ok(company.ToResponse());
    }

    [HttpGet("by-short-code/{shortCode}")]
    [Authorize(Policy = "CompanyRead")]
    [ProducesResponseType(typeof(CompanyResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CompanyResponse>> GetByShortCode(string shortCode, CancellationToken ct)
    {
        var company = await _companyService.GetByShortCodeAsync(shortCode, ct);
        return company is null ? NotFound() : Ok(company.ToResponse());
    }

    [HttpPost]
    [Authorize(Policy = "CompanyCreate")]
    [ProducesResponseType(typeof(CompanyResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CompanyResponse>> Create([FromBody] CreateCompanyRequest request, CancellationToken ct)
    {
        try
        {
            var created = await _companyService.CreateAsync(request, ct);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(nameof(request.ShortCode), ex.Message);
            return ValidationProblem(ModelState);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(nameof(request.ShortCode), ex.Message);
            return ValidationProblem(ModelState);
        }
    }

    [HttpPatch("{id:guid}")]
    [Authorize(Policy = "CompanyUpdate")]
    [ProducesResponseType(typeof(CompanyResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CompanyResponse>> Patch(Guid id, [FromBody] PatchCompanyRequest request, CancellationToken ct)
    {
        try
        {
            var updated = await _companyService.PatchAsync(id, request, ct);
            return updated is null ? NotFound() : Ok(updated);
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(nameof(request.Name), ex.Message);
            return ValidationProblem(ModelState);
        }
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "CompanyDelete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var deleted = await _companyService.DeleteAsync(id, ct);
        return deleted ? NoContent() : NotFound();
    }
}
