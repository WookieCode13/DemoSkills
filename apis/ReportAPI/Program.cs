using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ReportAPI.Application.Reports;
using Microsoft.OpenApi.Models;
using ReportAPI.Infrastructure.Auth;
using ReportAPI.Infrastructure.Data;
using ReportAPI.Infrastructure.Reports;
using Serilog;
using Serilog.Formatting.Compact;
using Shared.Security.Net.Auth;
using Shared.Security.Net.Constants;

var buildBranch = Environment.GetEnvironmentVariable("BUILD_BRANCH") ?? "local";

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(new CompactJsonFormatter())
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    if (builder.Environment.IsDevelopment())
    {
        builder.Configuration.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.Local.json", optional: true, reloadOnChange: true);
    }

    builder.Host.UseSerilog((ctx, services, cfg) => cfg
        .ReadFrom.Configuration(ctx.Configuration)
        .Enrich.FromLogContext()
        .WriteTo.Console(new CompactJsonFormatter()));

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

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "ReportAPI",
            Version = $"v1 - {buildBranch}",
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
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<IReportRepository, ReportRepository>();
    builder.Services.AddScoped<IUserAuthRepository, UserAuthRepository>();
    builder.Services.AddScoped<IUserAuthContextProvider, UserAuthContextProvider>();
    builder.Services.AddScoped<IAppAuthorizationService, AppAuthorizationService>();
    builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
    builder.Services.AddDemoSkillsJwtAuth(builder.Configuration);
    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("ReportRead", policy =>
            policy.Requirements.Add(new PermissionRequirement(Permissions.ReportRead)));
    });

    var app = builder.Build();

    var enableSwagger = builder.Configuration.GetValue<bool>("EnableSwagger", false) || app.Environment.IsDevelopment();
    if (enableSwagger)
    {
        app.UseSwagger(c =>
        {
            c.RouteTemplate = "swagger/{documentName}/swagger.json";
        });
        app.UseSwaggerUI(c =>
        {
            c.RoutePrefix = "swagger";
            c.SwaggerEndpoint("v1/swagger.json", "ReportAPI v1");
        });
    }

    app.UseSerilogRequestLogging();
    app.UseCors(corsPolicyName);
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    Log.Information("Starting ReportAPI");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "ReportAPI terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
