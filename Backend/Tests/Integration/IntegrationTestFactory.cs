using Infrastructure.MCP;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.MongoDb;

namespace Tests.Integration;

/// <summary>
/// Boots the real API in-memory against a throwaway MongoDB container.
///
/// - Starts a single-node Mongo container (Docker required) and repoints the app's
///   <c>MongoSettings:ConnectionString</c>/<c>DatabaseName</c> at it via in-memory config,
///   so <c>IOptions&lt;MongoSettings&gt;</c> picks it up without re-registering the singleton
///   repositories.
/// - Removes McpStartupService (it dials the in-process /mcp endpoint on boot) — these tests
///   don't need it. Tests that exercise the chat/agentic endpoint must wire MCP up separately.
///
/// Shared across all integration test classes via the "integration" collection, so the
/// container starts once per test run rather than per class.
/// </summary>
public class IntegrationTestFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly MongoDbContainer _mongo = new MongoDbBuilder("mongo:7.0").Build();

    public async Task InitializeAsync() => await _mongo.StartAsync();

    // Explicit interface impl so it doesn't clash with the base WebApplicationFactory.DisposeAsync
    // (ValueTask). Tear down the host (Kestrel TestServer + MongoClient pool) first, then the container.
    async Task IAsyncLifetime.DisposeAsync()
    {
        await base.DisposeAsync();
        await _mongo.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        // Highest-precedence config — repoint Mongo at the container.
        builder.ConfigureAppConfiguration((_, cfg) => cfg.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["MongoSettings:ConnectionString"] = _mongo.GetConnectionString(),
            ["MongoSettings:DatabaseName"] = "itest",
        }));

        builder.ConfigureServices(services =>
        {
            // Remove only McpStartupService — it dials the in-process /mcp endpoint on boot and is
            // irrelevant to these tests. Other hosted services are left intact so we don't silently
            // disable unrelated wiring. (Chat/agentic-endpoint tests would need MCP wired separately.)
            foreach (var d in services
                         .Where(d => d.ImplementationType == typeof(McpStartupService))
                         .ToList())
                services.Remove(d);
        });
    }
}

/// <summary>
/// xUnit collection that shares a single <see cref="IntegrationTestFactory"/> (and its Mongo
/// container) across every test class marked <c>[Collection("integration")]</c>.
/// </summary>
[CollectionDefinition("integration")]
public class IntegrationCollection : ICollectionFixture<IntegrationTestFactory>;
