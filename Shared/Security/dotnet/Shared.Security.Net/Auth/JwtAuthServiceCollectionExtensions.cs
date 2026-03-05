using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace Shared.Security.Net.Auth;

public static class JwtAuthServiceCollectionExtensions
{
    public static IServiceCollection AddDemoSkillsJwtAuth(this IServiceCollection services, IConfiguration configuration)
    {
        var authority = configuration["Jwt:Authority"];
        var expectedClientId = configuration["Jwt:ClientId"];

        if (string.IsNullOrWhiteSpace(authority))
        {
            throw new InvalidOperationException("Missing required configuration key: Jwt:Authority");
        }

        if (string.IsNullOrWhiteSpace(expectedClientId))
        {
            throw new InvalidOperationException("Missing required configuration key: Jwt:ClientId");
        }

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = authority;
                options.IncludeErrorDetails = true;

                // Cognito access tokens use `client_id` + `token_use=access` instead of relying on `aud`.
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = false
                };

                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        var logger = context.HttpContext.RequestServices
                            .GetRequiredService<ILoggerFactory>()
                            .CreateLogger("JwtAuth");
                        logger.LogWarning("JWT authentication failed: {Message}", context.Exception.Message);
                        return Task.CompletedTask;
                    },
                    OnChallenge = context =>
                    {
                        var logger = context.HttpContext.RequestServices
                            .GetRequiredService<ILoggerFactory>()
                            .CreateLogger("JwtAuth");
                        logger.LogWarning("JWT challenge: error={Error} desc={Description}", context.Error, context.ErrorDescription);
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        var logger = context.HttpContext.RequestServices
                            .GetRequiredService<ILoggerFactory>()
                            .CreateLogger("JwtAuth");
                        var tokenUse = context.Principal?.FindFirst("token_use")?.Value;
                        var clientId = context.Principal?.FindFirst("client_id")?.Value;

                        logger.LogInformation("JWT token validated claims: token_use={TokenUse} client_id={ClientId}", tokenUse, clientId);

                        if (tokenUse != "access" || clientId != expectedClientId)
                        {
                            context.Fail("Invalid Cognito access token.");
                        }

                        return Task.CompletedTask;
                    }
                };
            });

        services.AddAuthorization();
        return services;
    }
}
