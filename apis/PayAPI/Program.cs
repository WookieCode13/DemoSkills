using System;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Formatting.Compact;

var buildBranch = Environment.GetEnvironmentVariable("BUILD_BRANCH") ?? "local";

// Bootstrap Serilog early
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
            Title = "PayAPI",
            Version = $"v1 - {buildBranch}",
            Description = $"Build branch: {buildBranch}"
        });
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
            c.SwaggerEndpoint("v1/swagger.json", "PayAPI v1");
        });
    }

    app.UseSerilogRequestLogging();
    app.UseCors(corsPolicyName);
    app.UseAuthorization();

    app.MapControllers();

    Log.Information("Starting PayAPI");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "PayAPI terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
