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
using Microsoft.AspNetCore.HttpOverrides;
using Shared.Security.Net.Auditing;
using Shared.Security.Net.Auth;
using Shared.Security.Net.Constants;
using Shared.Security.Net.Middleware;
using EmployeeAPI.Infrastructure.Auth;
using Microsoft.AspNetCore.Authorization;

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
    builder.Services.AddHttpContextAccessor();

    // Shared auth services for repository-backed permission checks.
    builder.Services.AddScoped<IUserAuthRepository, UserAuthRepository>();
    builder.Services.AddScoped<IUserAuthContextProvider, UserAuthContextProvider>();
    builder.Services.AddScoped<IAppAuthorizationService, AppAuthorizationService>();
    builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

    builder.Services.Configure<MigrationOptions>(builder.Configuration.GetSection("Migrations"));
    builder.Services
        .AddFluentMigratorCore()
        .ConfigureRunner(runner => runner
            .AddPostgres()
            .WithGlobalConnectionString(builder.Configuration.GetConnectionString("Default") ?? string.Empty)
            .ScanIn(typeof(MigrationHostedService).Assembly).For.Migrations())
        .AddLogging(logging => logging.AddFluentMigratorConsole());

    builder.Services.AddHostedService<MigrationHostedService>();
    builder.Services.AddDemoSkillsJwtAuth(builder.Configuration);

    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("EmployeeRead", policy =>
            policy.Requirements.Add(new PermissionRequirement(Permissions.EmployeeRead)));
        options.AddPolicy("EmployeeUpdate", policy =>
            policy.Requirements.Add(new PermissionRequirement(Permissions.EmployeeUpdate)));
        options.AddPolicy("EmployeeDelete", policy =>
            policy.Requirements.Add(new PermissionRequirement(Permissions.EmployeeDelete)));
    });

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
