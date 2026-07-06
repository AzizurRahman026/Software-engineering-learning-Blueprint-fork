
using Anthropic.SDK;
using Infrastructure.Configuration;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Llm;

public class ClaudeChatClient
{
    public static IChatClient CreateClaudeChatClient(ClaudeOptions claudeOptions, ILoggerFactory loggerFactory)
    {
        return new AnthropicClient(claudeOptions.ApiKey)
            .Messages
            .AsBuilder()
            .UseFunctionInvocation()
            .Use(inner => new ResilientChatClient(inner, loggerFactory.CreateLogger<ResilientChatClient>()))
            .Build();
    }
}
