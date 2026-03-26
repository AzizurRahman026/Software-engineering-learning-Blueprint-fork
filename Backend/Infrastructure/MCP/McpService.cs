using Application.Common.Interfaces.Services;
using Infrastructure.Configuration;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

namespace Infrastructure.MCP;

public class McpService : IMcpService
{
    private McpClient? _client;
    private IClientTransport? _transport;
    private IList<McpClientTool>? _toolsCache;

    private McpServerOptions? _options;
    private readonly ILogger<McpService> _logger;

    public McpService(ILogger<McpService> logger)
    {
        _logger = logger;
    }

    public async Task ConnectAsync(CancellationToken ct = default)
    {
        // transport: how we talk to the MCP server (e.g. HTTP, gRPC, WebSocket)
        _transport = new StdioClientTransport(
            new StdioClientTransportOptions
            {
                Name = "MCP Stdio Transport",
                Command = "dotnet",
                Arguments = ["run", "--project", _options.ProjectPath, "--no-build"],
            });

        // Use the McpClient static factory method that exists on McpClient.
        // McpClient.CreateAsync signature: (IClientTransport, McpClientOptions? clientOptions = null, ILoggerFactory? loggerFactory = null, CancellationToken cancellationToken = default)
        _client = await McpClient.CreateAsync(_transport, null, null, ct);
        _toolsCache = await _client.ListToolsAsync();
    }

    public async Task<List<AITool>> GetToolsAsync(CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(_toolsCache, "MCP server not connected.");
        var tools = _toolsCache
            .Cast<AITool>()
            .ToList();
        return tools;
    }

    public async Task<string> CallToolAsync(string toolName, Dictionary<string, object?> arguments, CancellationToken ct)
    {
        if (_client is null)
        {
            throw new InvalidOperationException("MCP client not connected.");
        }

        _logger.LogInformation($"Calling MCP tool. Name={toolName}.");
        var result = await _client.CallToolAsync(toolName, arguments, cancellationToken: ct);

        return string.Join("\n",
            result.Content
            .OfType<TextContentBlock>()
            .Select(c =>
            {
                return c switch
                {
                    TextContentBlock t => t.Text,
                    _ => "[Unsupported content]"
                };
            }));
    }
}
