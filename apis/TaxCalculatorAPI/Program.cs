using System;
using Microsoft.OpenApi.Models;
using Serilog;

var buildBranch = Environment.GetEnvironmentVariable("BUILD_BRANCH") ?? "local";

// Bootstrap Serilog early
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Configuration.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.Local.json", optional: true, reloadOnChange: true);

    builder.Host.UseSerilog((ctx, services, cfg) => cfg
        .ReadFrom.Configuration(ctx.Configuration)
        .Enrich.FromLogContext()
        .WriteTo.Console());

    builder.Services.AddControllers();
    builder.Services.AddRouting(options => options.LowercaseUrls = true);
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "TaxCalculatorAPI",
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
            c.SwaggerEndpoint("swagger/v1/swagger.json", "TaxCalculatorAPI v1");
        });
    }

    app.UseSerilogRequestLogging();
    app.UseAuthorization();

    app.MapControllers();

    Log.Information("Starting TaxCalculatorAPI");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "TaxCalculatorAPI terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
