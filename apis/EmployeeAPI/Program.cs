using Serilog;

// Bootstrap Serilog early
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Use Serilog for logging
    builder.Host.UseSerilog((ctx, services, cfg) => cfg
        .ReadFrom.Configuration(ctx.Configuration)
        .Enrich.FromLogContext()
        .WriteTo.Console());

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
    app.UseSwagger();
    app.UseSwaggerUI();
}

    app.UseSerilogRequestLogging();

    // Apply PathBase early so Swagger and routes respect it
    if (!string.IsNullOrWhiteSpace(pathBase))
    {
        app.UsePathBase(pathBase);
    }

    //app.UseHttpsRedirection();

app.UseAuthorization();

    // Friendly paths for ALB/routing
    app.MapGet("/employees/swagger", () => Results.Redirect("/swagger", permanent: false));

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
