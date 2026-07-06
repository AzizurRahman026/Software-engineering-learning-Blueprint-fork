using System.Collections.Concurrent;
using Application.Common.Interfaces.Services;
using Domain.Enums;
using Infrastructure.Configuration;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Llm;

public class LlmFactory : ILlmFactory
{
    private readonly GeminiOptions _geminiOptions;
    private readonly ClaudeOptions _claudeOptions;
    private readonly ILoggerFactory _loggerFactory;

    // LlmFactory is a singleton (Program.cs), so this caches ONE built pipeline
    // (and its SDK-owned HttpClient) per provider for the app's lifetime,
    // instead of constructing a new client + HttpClient on every chat request.
    // Lazy<> guarantees a single construction even under concurrent first calls.
    private readonly ConcurrentDictionary<LlmProvider, Lazy<IChatClient>> _clients = new();

    public LlmFactory(
        IOptions<GeminiOptions> geminiOptions,
        IOptions<ClaudeOptions> claudeOptions,
        ILoggerFactory loggerFactory)
    {
        _geminiOptions = geminiOptions.Value;
        _claudeOptions = claudeOptions.Value;
        _loggerFactory = loggerFactory;
    }

    public IChatClient Create(LlmProvider provider) =>
        _clients.GetOrAdd(provider, p => new Lazy<IChatClient>(() => Build(p))).Value;

    private IChatClient Build(LlmProvider provider) => provider switch
    {
        LlmProvider.Gemini => GeminiChatClient.CreateGeminiChatClient(_geminiOptions, _loggerFactory),
        LlmProvider.Claude => ClaudeChatClient.CreateClaudeChatClient(_claudeOptions, _loggerFactory),
        _ => throw new NotSupportedException($"LLM provider '{provider}' is not supported.")
    };
}
