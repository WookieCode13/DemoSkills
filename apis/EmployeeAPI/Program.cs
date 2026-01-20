using System;
using EmployeeAPI.Migrations;
using FluentMigrator.Runner;
using Microsoft.OpenApi.Models;
using Serilog;

// Bootstrap Serilog early
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    var buildBranch = Environment.GetEnvironmentVariable("BUILD_BRANCH") ?? "local";

    // Optional per-environment local overrides (not committed): appsettings.{Environment}.Local.json
    builder.Configuration.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.Local.json", optional: true, reloadOnChange: true);

    // Use Serilog for logging
builder.Host.UseSerilog((ctx, services, cfg) => cfg
    .ReadFrom.Configuration(ctx.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console());

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddRouting(options => options.LowercaseUrls = true);
const string corsPolicyName = "DashboardCors";
var dashboardOrigins = new List<string>
{
    "http://longranch.com",
    "http://dashboard.longranch.com",
};
if (builder.Environment.IsDevelopment())
{
    dashboardOrigins.Add("http://longranch.local");
    dashboardOrigins.Add("http://dashboard.longranch.local");
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
});

    builder.Services.Configure<MigrationOptions>(builder.Configuration.GetSection("Migrations"));
    builder.Services
        .AddFluentMigratorCore()
        .ConfigureRunner(runner => runner
            .AddPostgres()
            .WithGlobalConnectionString(builder.Configuration.GetConnectionString("Default") ?? string.Empty)
            .ScanIn(typeof(MigrationHostedService).Assembly).For.Migrations())
        .AddLogging(logging => logging.AddFluentMigratorConsole());
    builder.Services.AddHostedService<MigrationHostedService>();

    // Optional: serve the app under a sub-path (e.g., "/employee")
    var pathBase = builder.Configuration["PathBase"];
    if (!string.IsNullOrWhiteSpace(pathBase) && !pathBase.StartsWith("/"))
    {
        pathBase = "/" + pathBase;
    }

    var app = builder.Build();

// Configure the HTTP request pipeline.
// Enable Swagger based on config or environment
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
        c.SwaggerEndpoint("v1/swagger.json", "EmployeeAPI v1");
    });
}

    app.UseSerilogRequestLogging();
    app.UseCors(corsPolicyName);

    // Apply PathBase early so Swagger and routes respect it
    if (!string.IsNullOrWhiteSpace(pathBase))
    {
        app.UsePathBase(pathBase);
    }

    //app.UseHttpsRedirection();

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
