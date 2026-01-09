using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace EmployeeAPI.Controllers;

[ApiController]
[Route("api/v1/db")]
public class DbPingController : ControllerBase
{
    private readonly ILogger<DbPingController> _logger;
    private readonly string? _connectionString;

    public DbPingController(IConfiguration configuration, ILogger<DbPingController> logger)
    {
        _logger = logger;
        _connectionString = configuration.GetConnectionString("Default");
    }

    [HttpGet("ping")]
    public async Task<IActionResult> Ping(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_connectionString) || _connectionString.Contains("TODO", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("DB ping skipped: connection string is not configured.");
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { status = "unconfigured" });
        }

        try
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            await using var command = new NpgsqlCommand("SELECT 1", connection);
            var result = await command.ExecuteScalarAsync(cancellationToken);

            _logger.LogInformation("DB ping succeeded with result {Result}.", result);
            return Ok(new { status = "ok", result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DB ping failed.");
            return StatusCode(StatusCodes.Status500InternalServerError, new { status = "error", message = "Database ping failed" });
        }
    }
}
