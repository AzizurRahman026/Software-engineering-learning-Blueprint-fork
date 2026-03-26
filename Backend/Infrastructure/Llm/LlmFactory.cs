using Anthropic.SDK;
using Application.Common.Interfaces.Services;
using Domain.Enums;
using GenerativeAI;
using GenerativeAI.Microsoft;
using Infrastructure.Configuration;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;

namespace Infrastructure.Llm;

public class LlmFactory : ILlmFactory
{
    private readonly GeminiOptions _geminiOptions;
    private readonly ClaudeOptions _claudeOptions;

    public LlmFactory(
        IOptions<GeminiOptions> geminiOptions,
        IOptions<ClaudeOptions> claudeOptions)
    {
        _geminiOptions = geminiOptions.Value;
        _claudeOptions = claudeOptions.Value;
    }

    public IChatClient Create(LlmProvider provider) => provider switch
    {
        LlmProvider.Gemini => GeminiChatClient.CreateGeminiChatClient(_geminiOptions),
        LlmProvider.Claude => ClaudeChatClient.CreateClaudeChatClient(_claudeOptions),
        _ => throw new NotSupportedException($"LLM provider '{provider}' is not supported.")
    };
}