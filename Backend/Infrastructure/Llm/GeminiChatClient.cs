
using GenerativeAI.Microsoft;
using Infrastructure.Configuration;
using Microsoft.Extensions.AI;

namespace Infrastructure.Llm;

public class GeminiChatClient
{
    public static IChatClient CreateGeminiChatClient(GeminiOptions geminiOptions)
    {
        return new GenerativeAIChatClient(geminiOptions.ApiKey, geminiOptions.Model)
            .AsBuilder()
            .UseFunctionInvocation()
            .Build();
    }
}
