namespace Shared.Security.Net.Middleware;


public sealed class CompanyTenantMiddleware : IMiddleware
{
    /// <summary>
    /// LEARNING NOTE: This middleware uses a different pattern than standard pipeline middleware
    /// (such as CompanyHeaderLoggingMiddleware). It is registered as a scoped service in the
    /// employee API and invoked explicitly from controllers to set the company short name in
    /// the HTTP context for use by repositories, only for requests that require it.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="next"></param>
    /// <returns></returns>
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (context.Request.Headers.TryGetValue(HeaderNames.CompanyShortName, out var headerValue)
        && !string.IsNullOrWhiteSpace(headerValue))
        {
            var company = headerValue.ToString().Trim();
            context.Items["CompanyShortName"] = company;
        }
        await next(context);
    }
}
