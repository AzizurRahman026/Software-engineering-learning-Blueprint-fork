
using Anthropic.SDK;
using Infrastructure.Configuration;
using Microsoft.Extensions.AI;

namespace Infrastructure.Llm;

public class ClaudeChatClient
{
    public static IChatClient CreateClaudeChatClient(ClaudeOptions claudeOptions)
    {
        return new AnthropicClient(claudeOptions.ApiKey)
            .Messages
            .AsBuilder()
            .UseFunctionInvocation()
            .Build();
    }
}
