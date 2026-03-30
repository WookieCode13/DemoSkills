using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReportAPI.Application.Reports;
using ReportAPI.Contracts;

namespace ReportAPI.Controllers;

[ApiController]
[Route("api/v1/reports")]
public class ReportsController : ControllerBase
{
    private readonly ILogger<ReportsController> _logger;
    private readonly IReportRepository _reportRepository;

    public ReportsController(ILogger<ReportsController> logger, IReportRepository reportRepository)
    {
        _logger = logger;
        _reportRepository = reportRepository;
    }

    [HttpGet("health")]
    public IActionResult Health()
    {
        _logger.LogInformation("Health check requested from {RemoteIp}", HttpContext.Connection.RemoteIpAddress?.ToString());
        var payload = new HealthResponse(
            Status: "ok",
            Service: "ReportAPI",
            Timestamp: DateTime.UtcNow.ToString("o")
        );
        _logger.LogInformation("Health check OK at {Timestamp}", payload.Timestamp);
        return Ok(payload);
    }

    [HttpGet]
    [Authorize(Policy = "ReportRead")]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var reports = await _reportRepository.GetReportsAsync(ct);
        return Ok(reports.Select(r => new ReportDefinitionResponse(
            r.Id,
            r.ReportCode,
            r.Description,
            r.CreatedUtc,
            r.UpdatedUtc)));
    }

    [HttpGet("data")]
    [Authorize(Policy = "ReportRead")]
    public async Task<IActionResult> ListData(CancellationToken ct)
    {
        var reportData = await _reportRepository.GetReportDataAsync(ct);
        return Ok(reportData.Select(r => new ReportDataResponse(
            r.Id,
            r.ReportCode,
            r.ReportData.RootElement.Clone(),
            r.CreatedUtc,
            r.UpdatedUtc)));
    }
}
