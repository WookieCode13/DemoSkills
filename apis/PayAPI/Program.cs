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

    builder.Host.UseSerilog((ctx, services, cfg) => cfg
        .ReadFrom.Configuration(ctx.Configuration)
        .Enrich.FromLogContext()
        .WriteTo.Console());

    builder.Services.AddControllers();
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
            c.RouteTemplate = "pay/swagger/{documentName}/swagger.json";
        });
        app.UseSwaggerUI(c =>
        {
            c.RoutePrefix = "pay/swagger";
            c.SwaggerEndpoint("v1/swagger.json", "PayAPI v1");
        });
    }

    app.UseSerilogRequestLogging();
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
