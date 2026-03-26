
using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;

namespace Application.Common.Interfaces.Services;

public interface IMcpService
{
    // connects to the MCP server
    Task ConnectAsync(CancellationToken ct = default);

    Task DisconnectAsync(CancellationToken ct = default);
    // Get the list of tools the MCP server exposes
    Task<List<AITool>> GetToolsAsync(CancellationToken ct = default);

    // Executes a named tool with the given arguments, returns text result
    Task<string> CallToolAsync(
                        string toolName,
                        Dictionary<string, object?> arguments,
                        CancellationToken ct);
}
