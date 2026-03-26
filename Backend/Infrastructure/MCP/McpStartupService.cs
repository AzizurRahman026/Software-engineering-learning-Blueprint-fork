
using Application.Common.Interfaces.Services;
using DnsClient.Internal;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.MCP;

public class McpStartupService : IHostedService
{
    private readonly IMcpService _mcpService;
    private readonly ILogger<McpStartupService> _logger;
    public McpStartupService(IMcpService mcpService,
        ILogger<McpStartupService> logger)
    {
        _mcpService = mcpService;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken ct)
    {
        try
        {
            _logger.LogInformation("McpStartupService: connecting to MCP server...");
            await _mcpService.ConnectAsync(ct);
            _logger.LogInformation("McpStartupService: connected to MCP server.");
        }
        catch (Exception ex)
        {
            _logger.LogError("McpStartupService: failed to connect to MCP server.");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
