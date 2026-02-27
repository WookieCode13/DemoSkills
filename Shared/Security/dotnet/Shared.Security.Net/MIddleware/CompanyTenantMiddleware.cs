using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Shared.Security.Net.Constants;

namespace Shared.Security.Net.Middleware;


public sealed class CompanyTenantMiddleware : IMiddleware
{
    /// <summary>
    /// LEARNING NOTE: this is a different middleware pattern than the standard one(classic like CompanyHeaderLoggingMiddleware) 
    /// because we want to use it as a scoped service in the employee API and call it from the controllers, 
    /// rather than using it in the pipeline for every request.
    /// this is used in the employee API to set the company short name in the context for use in the repositories. 
    /// This is not a standard middleware that can be used in the pipeline, 
    /// it is used as a scoped service in the employee API and is called from the controllers. 
    /// This is because we want to set the company short name in the context for use in the repositories, 
    /// and we don't want to set it for every request, only for the requests that need it.
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
