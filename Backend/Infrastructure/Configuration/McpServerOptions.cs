namespace Infrastructure.Configuration;

/// <summary>Client settings for connecting to this app's MCP HTTP endpoint (Streamable HTTP).</summary>
public class McpServerOptions
{
    /// <summary>Full URL, e.g. http://localhost:5000/mcp — must match <c>MapMcp</c> route and Kestrel URLs.</summary>
    public string Endpoint { get; set; } = "http://localhost:5000/mcp";
}
