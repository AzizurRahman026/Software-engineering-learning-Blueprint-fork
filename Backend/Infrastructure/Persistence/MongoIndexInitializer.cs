using Application.Common.Interfaces.Persistence;
using Infrastructure.Persistence.Indexing;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Persistence;

/// <summary>
/// Ensures at startup that the Mongo indexes behind hot read paths exist.
/// OCP: each collection declares its indexes in its own IMongoIndexConfiguration
/// (Persistence/Indexing/) — a new collection means a new config class; this file
/// never changes. Idempotent, so re-running on every boot is safe.
/// </summary>
public sealed class MongoIndexInitializer : IHostedService
{
    private readonly IDatabaseContext _dbContext;
    private readonly IEnumerable<IMongoIndexConfiguration> _configurations;
    private readonly ILogger<MongoIndexInitializer> _logger;

    public MongoIndexInitializer(
        IDatabaseContext dbContext,
        IEnumerable<IMongoIndexConfiguration> configurations,
        ILogger<MongoIndexInitializer> logger)
    {
        _dbContext = dbContext;
        _configurations = configurations;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        // IDatabaseContext exposes no driver types (Application must stay driver-free);
        // index management needs IMongoCollection, so reach it via the concrete context.
        if (_dbContext is not DatabaseContext ctx)
        {
            _logger.LogWarning(
                "MongoIndexInitializer skipped: IDatabaseContext resolved to {ContextType}, not DatabaseContext",
                _dbContext.GetType().Name);
            return;
        }

        foreach (var configuration in _configurations)
        {
            // One collection failing must not stop the others or crash startup —
            // a missing index means slow reads, not broken ones. Log and continue.
            try
            {
                var names = await configuration.ApplyAsync(ctx, cancellationToken);
                _logger.LogInformation("Mongo indexes ensured on {Collection}: {IndexNames}",
                    configuration.CollectionName, string.Join(", ", names));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Index creation failed for {Collection}; reads fall back to collection scans until next restart",
                    configuration.CollectionName);
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
