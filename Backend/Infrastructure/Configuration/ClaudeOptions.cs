using Anthropic.SDK;

namespace Infrastructure.Configuration;

public class ClaudeOptions
{
    public string ApiKey { get; internal set; }
    public string Model { get; internal set; }
}
