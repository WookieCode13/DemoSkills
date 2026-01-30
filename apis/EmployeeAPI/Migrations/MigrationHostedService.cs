using FluentMigrator.Runner;
using Microsoft.Extensions.Options;

namespace EmployeeAPI.Migrations;

public sealed class MigrationHostedService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MigrationHostedService> _logger;
    private readonly MigrationOptions _options;

    public MigrationHostedService(
        IServiceProvider serviceProvider,
        ILogger<MigrationHostedService> logger,
        IOptions<MigrationOptions> options)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _options = options.Value;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (!_options.Enabled)
        {
            _logger.LogInformation("Database migrations are disabled.");
            return Task.CompletedTask;
        }

        using var scope = _serviceProvider.CreateScope();
        var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
        runner.MigrateUp();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
