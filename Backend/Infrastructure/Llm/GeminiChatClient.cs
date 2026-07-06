
using GenerativeAI.Microsoft;
using Infrastructure.Configuration;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Llm;

public class GeminiChatClient
{
    public static IChatClient CreateGeminiChatClient(GeminiOptions geminiOptions, ILoggerFactory loggerFactory)
    {
        return new GenerativeAIChatClient(geminiOptions.ApiKey, geminiOptions.Model)
            .AsBuilder()
            .Use(inner => new ResilientChatClient(inner, loggerFactory.CreateLogger<ResilientChatClient>()))
            .Build();
    }
}
