using System;
using EmployeeAPI.Application.Auditing;
using EmployeeAPI.Application.Employees;
using EmployeeAPI.Infrastructure.Data;
using EmployeeAPI.Infrastructure.Auditing;
using EmployeeAPI.Infrastructure.Employees;
using EmployeeAPI.Migrations;
using FluentMigrator.Runner;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Formatting.Compact;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.IdentityModel.Tokens;
using Shared.Security.Net.Middleware;

// Bootstrap Serilog early
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(new CompactJsonFormatter())
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    // Comes from variable set in pipeline or default to "local"
    var buildBranch = Environment.GetEnvironmentVariable("BUILD_BRANCH") ?? "local";

    // Optional local overrides (not committed) for development only.
    if (builder.Environment.IsDevelopment())
    {
        builder.Configuration.AddJsonFile(
            $"appsettings.{builder.Environment.EnvironmentName}.Local.json",
            optional: true,
            reloadOnChange: true);
    }

    // Use Serilog for logging
    builder.Host.UseSerilog((ctx, services, cfg) => cfg
        .ReadFrom.Configuration(ctx.Configuration)
        .Enrich.FromLogContext()
        .WriteTo.Console(new CompactJsonFormatter()));

    // Add services to the container.
    builder.Services.AddControllers();
    builder.Services.AddRouting(options => options.LowercaseUrls = true);

    const string corsPolicyName = "DashboardCors";
    var dashboardOrigins = new List<string>
    {
        "http://longranch.com",
        "http://dashboard.longranch.com",
        "https://longranch.com",
        "https://dashboard.longranch.com",
    };
    if (builder.Environment.IsDevelopment())
    {
        dashboardOrigins.Add("http://longranch.wookie");
        dashboardOrigins.Add("http://dashboard.longranch.wookie");
    }

    builder.Services.AddCors(options =>
    {
        options.AddPolicy(corsPolicyName, policy =>
            policy.WithOrigins(dashboardOrigins.ToArray())
                .AllowAnyHeader()
                .AllowAnyMethod());
    });

    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "EmployeeAPI",
            Version = $"api v1 ({buildBranch})",
            Description = $"Build branch: {buildBranch}"
        });

        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Paste JWT access token only (no 'Bearer ' prefix)."
        });

        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
    });

    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));
    builder.Services.AddScoped<EmployeeService>();
    builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
    builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();
    builder.Services.AddScoped<CompanyTenantMiddleware>();

    builder.Services.Configure<MigrationOptions>(builder.Configuration.GetSection("Migrations"));
    builder.Services
        .AddFluentMigratorCore()
        .ConfigureRunner(runner => runner
            .AddPostgres()
            .WithGlobalConnectionString(builder.Configuration.GetConnectionString("Default") ?? string.Empty)
            .ScanIn(typeof(MigrationHostedService).Assembly).For.Migrations())
        .AddLogging(logging => logging.AddFluentMigratorConsole());    
    builder.Services.AddHostedService<MigrationHostedService>();

 Console.WriteLine("-- JWT test console --");
 Log.Information("-test log");

    builder.Services
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.Authority = builder.Configuration["Jwt:Authority"];
            var expectedClientId = builder.Configuration["Jwt:ClientId"];
            options.IncludeErrorDetails = true;

            Console.WriteLine("-- JWT Configuration --");
            Console.WriteLine($"Authority: {options.Authority}");
            Console.WriteLine($"Expected Client ID: {expectedClientId}");
            Log.Information("-- JWT Configuration --");
            Log.Information($"Authority: {options.Authority}");
            Log.Information($"Expected Client ID: {expectedClientId}");

            // Cognito access tokens use `client_id` + `token_use=access` instead of relying on `aud`.
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false
            };

            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    Log.Warning("JWT authentication failed: {Message}", context.Exception.Message);
                    return Task.CompletedTask;
                },
                OnChallenge = context =>
                {
                    Log.Warning("JWT challenge: error={Error} desc={Description}", context.Error, context.ErrorDescription);
                    return Task.CompletedTask;
                },
                OnTokenValidated = context =>
                {
                    var tokenUse = context.Principal?.FindFirst("token_use")?.Value;
                    var clientId = context.Principal?.FindFirst("client_id")?.Value;
                    Log.Information("JWT token validated claims: token_use={TokenUse} client_id={ClientId}", tokenUse, clientId);
                    if (tokenUse != "access" || string.IsNullOrWhiteSpace(expectedClientId) || clientId != expectedClientId)
                    {
                        context.Fail("Invalid Cognito access token.");
                    }

                    return Task.CompletedTask;
                }
            };
        });

    builder.Services.AddAuthorization();


    // Optional: serve the app under a sub-path (e.g., "/employee")
    var pathBase = builder.Configuration["PathBase"];
    if (!string.IsNullOrWhiteSpace(pathBase) && !pathBase.StartsWith("/"))
    {
        pathBase = "/" + pathBase;
    }

    var app = builder.Build();

    app.UseForwardedHeaders(new ForwardedHeadersOptions
    {
        ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost
    });

    // Configure the HTTP request pipeline.
    // Enable Swagger based on config or environment
    var enableSwagger = builder.Configuration.GetValue<bool>("EnableSwagger", false) || app.Environment.IsDevelopment();
    if (enableSwagger)
    {
        app.UseSwagger(c =>
        {
            c.RouteTemplate = "swagger/{documentName}/swagger.json";
            c.PreSerializeFilters.Add((swagger, httpReq) =>
            {
                var forwardedProto = httpReq.Headers["X-Forwarded-Proto"].ToString();
                var forwardedHost = httpReq.Headers["X-Forwarded-Host"].ToString();
                var scheme = string.IsNullOrWhiteSpace(forwardedProto) ? httpReq.Scheme : forwardedProto;
                var host = string.IsNullOrWhiteSpace(forwardedHost) ? httpReq.Host.Value : forwardedHost;
                var basePath = httpReq.PathBase.HasValue ? httpReq.PathBase.Value : string.Empty;
                swagger.Servers = new List<OpenApiServer>
                {
                    new OpenApiServer { Url = $"{scheme}://{host}{basePath}" }
                };
            });
        });
        app.UseSwaggerUI(c =>
        {
            c.RoutePrefix = "swagger";
            var swaggerJsonPath = string.IsNullOrWhiteSpace(pathBase)
                ? "/swagger/v1/swagger.json"
                : $"{pathBase}/swagger/v1/swagger.json";
            c.SwaggerEndpoint(swaggerJsonPath, "EmployeeAPI v1");
        });
    }

    app.UseSerilogRequestLogging();
    app.UseCors(corsPolicyName);

    // Apply PathBase early so Swagger and routes respect it
    if (!string.IsNullOrWhiteSpace(pathBase))
    {
        app.UsePathBase(pathBase);
    }

    app.UseMiddleware<CompanyHeaderLoggingMiddleware>(); // classic
    app.UseMiddleware<CompanyTenantMiddleware>();        // IMiddleware - scoped and can use DI

    app.UseAuthentication();
    app.UseAuthorization();

    // Note: No root ("/") or top-level "/health" endpoints

    app.MapControllers();

    Log.Information("Starting EmployeeAPI");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "EmployeeAPI terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
