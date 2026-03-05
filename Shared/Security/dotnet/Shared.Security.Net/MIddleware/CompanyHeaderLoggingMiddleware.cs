using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Shared.Security.Net.Constants;

namespace Shared.Security.Net.Middleware;

/// <summary>
/// Middleware to log the company short name from the "X-Company-ShortName" header for each incoming request.
/// There was no real reason for this, beside to learn to write custom middleware.
/// We will be using the "X-Company-ShortName" header in the future to route requests to the correct database, 
/// but for now we just log it.
/// </summary>
public sealed class CompanyHeaderLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CompanyHeaderLoggingMiddleware> _logger;

    /// <summary>
    /// LEARNING NOTE: Just for learning purposes, not a real middleware that we will be using in production.
    /// This is a CLASSIC middleware pattern, where we have a constructor that takes the next middleware in the pipeline and a logger, 
    /// and an InvokeAsync method that is called for each request.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <param name="logger">The logger to use for logging.</param>
    public CompanyHeaderLoggingMiddleware(RequestDelegate next, ILogger<CompanyHeaderLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(HeaderNames.CompanyShortName, out var company))
        {
            _logger.LogInformation("Request from company: {Company}", company.ToString());
        }
        else
        {
            _logger.LogInformation("Request without {HeaderName} header", HeaderNames.CompanyShortName);
        }

        if (context.Request.Headers.TryGetValue(HeaderNames.UserGroup, out var group))
        {
            _logger.LogInformation("Request from user group: {Group}", group.ToString());
        }
        else
        {
            _logger.LogInformation("Request without {HeaderName} header", HeaderNames.UserGroup);
        }
        await _next(context);
    }
}
