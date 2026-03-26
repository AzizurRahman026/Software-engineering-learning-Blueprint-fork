using Anthropic.SDK;
using Application.Common.Interfaces.Services;
using Domain.Enums;
using GenerativeAI;
using GenerativeAI.Microsoft;
using Infrastructure.Configuration;
using Microsoft.Extensions.AI;

namespace Infrastructure.Llm;

public class LlmFactory : ILlmFactory
{
    private readonly GeminiOptions _geminiOptions;
    private readonly ClaudeOptions _claudeOptions;

    public LlmFactory(GeminiOptions geminiOptions, ClaudeOptions claudeOptions)
    {
        _geminiOptions = geminiOptions;
        _claudeOptions = claudeOptions;
    }

    public IChatClient Create(LlmProvider provider) => provider switch
    {
        LlmProvider.Gemini => GeminiChatClient.CreateGeminiChatClient(_geminiOptions),
        LlmProvider.Claude => ClaudeChatClient.CreateClaudeChatClient(_claudeOptions),
        _ => throw new NotSupportedException($"LLM provider '{provider}' is not supported.")
    };
}