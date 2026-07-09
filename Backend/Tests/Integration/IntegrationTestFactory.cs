using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Testcontainers.MongoDb;

namespace Tests.Integration;

/// <summary>
/// Boots the real API in-memory against a throwaway MongoDB container.
///
/// - Starts a single-node Mongo container (Docker required) and repoints the app's
///   <c>MongoSettings:ConnectionString</c>/<c>DatabaseName</c> at it via in-memory config,
///   so <c>IOptions&lt;MongoSettings&gt;</c> picks it up without re-registering the singleton
///   repositories.
///
/// The MCP client connects lazily on first use, so it stays dormant for tests that don't exercise
/// the chat/agentic endpoint — no special teardown needed here.
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
            // JWT signing key so the host boots (startup fails fast without one).
            ["Jwt:Issuer"] = "BlueprintApi",
            ["Jwt:Audience"] = "BlueprintClient",
            ["Jwt:Key"] = "integration-test-signing-key-0123456789-abcdef",
        }));
    }
}

/// <summary>
/// xUnit collection that shares a single <see cref="IntegrationTestFactory"/> (and its Mongo
/// container) across every test class marked <c>[Collection("integration")]</c>.
/// </summary>
[CollectionDefinition("integration")]
public class IntegrationCollection : ICollectionFixture<IntegrationTestFactory>;
