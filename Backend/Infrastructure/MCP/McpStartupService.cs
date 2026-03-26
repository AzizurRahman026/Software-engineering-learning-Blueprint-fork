using Application.Common.Interfaces.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.MCP;

public class McpStartupService : IHostedService
{
    private readonly IMcpService _mcpService;
    private readonly IHostApplicationLifetime _lifetime;
    private readonly ILogger<McpStartupService> _logger;
    private CancellationTokenRegistration _startedRegistration;

    public McpStartupService(
        IMcpService mcpService,
        IHostApplicationLifetime lifetime,
        ILogger<McpStartupService> logger)
    {
        _mcpService = mcpService;
        _lifetime = lifetime;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _startedRegistration = _lifetime.ApplicationStarted.Register(() =>
        {
            _ = ConnectAfterStartedAsync(_lifetime.ApplicationStopping);
        });
        return Task.CompletedTask;
    }

    private async Task ConnectAfterStartedAsync(CancellationToken stoppingToken)
    {
        try
        {
            _logger.LogInformation("McpStartupService: connecting to MCP HTTP endpoint...");
            await _mcpService.ConnectAsync(stoppingToken);
            _logger.LogInformation("McpStartupService: MCP client ready.");
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Shutting down before connect finished.
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "McpStartupService: failed to connect MCP client.");
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _startedRegistration.Dispose();
        try
        {
            await _mcpService.DisconnectAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "McpStartupService: error while disconnecting MCP client.");
        }
    }
}
