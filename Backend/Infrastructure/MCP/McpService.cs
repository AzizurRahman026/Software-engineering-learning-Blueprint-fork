using Application.Common.Interfaces.Services;
using Infrastructure.Configuration;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

namespace Infrastructure.MCP;

public class McpService : IMcpService
{
    private McpClient? _client;
    private IClientTransport? _transport;
    private IList<McpClientTool>? _toolsCache;

    private readonly McpServerOptions _options;
    private readonly ILogger<McpService> _logger;

    public McpService(IOptions<McpServerOptions> options, ILogger<McpService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task ConnectAsync(CancellationToken ct = default)
    {
        await DisconnectAsync(ct);

        if (string.IsNullOrWhiteSpace(_options.Endpoint))
        {
            throw new InvalidOperationException(
                "MCP is not configured: set McpServer:Endpoint to the Streamable HTTP URL (e.g. http://localhost:5000/mcp).");
        }

        var endpoint = new Uri(_options.Endpoint, UriKind.Absolute);
        _transport = new HttpClientTransport(new HttpClientTransportOptions
        {
            Name = "MCP Streamable HTTP",
            Endpoint = endpoint,
            TransportMode = HttpTransportMode.StreamableHttp,
        });

        _client = await McpClient.CreateAsync(_transport, clientOptions: null, loggerFactory: null, cancellationToken: ct);
        _toolsCache = await _client.ListToolsAsync(cancellationToken: ct);
        _logger.LogInformation("MCP client connected. Endpoint={Endpoint}, toolCount={Count}.", endpoint, _toolsCache.Count);
    }

    public async Task DisconnectAsync(CancellationToken ct = default)
    {
        _toolsCache = null;

        if (_client is not null)
        {
            await _client.DisposeAsync();
            _client = null;
        }

        if (_transport is IAsyncDisposable asyncTransport)
        {
            await asyncTransport.DisposeAsync();
        }

        _transport = null;
    }

    public Task<List<AITool>> GetToolsAsync(CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(_toolsCache, "MCP server not connected.");
        var tools = _toolsCache
            .Cast<AITool>()
            .ToList();
        return Task.FromResult(tools);
    }

    public async Task<string> CallToolAsync(string toolName, Dictionary<string, object?> arguments, CancellationToken ct)
    {
        if (_client is null)
        {
            throw new InvalidOperationException("MCP client not connected.");
        }

        _logger.LogInformation("Calling MCP tool. Name={ToolName}.", toolName);
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
