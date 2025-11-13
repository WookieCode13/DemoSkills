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

    var app = builder.Build();

// Configure the HTTP request pipeline.
var enableSwagger = app.Environment.IsDevelopment() ||
                    builder.Configuration.GetValue<bool>("EnableSwagger", false);
if (enableSwagger)
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

    app.UseSerilogRequestLogging();

    //app.UseHttpsRedirection();

app.UseAuthorization();

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
