using Application.Common.Interfaces.Services;
using Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace Application.Features.Chat.Queries.SuggestThreadTitle;

/// <summary>
/// Structured-output pattern: the C# record IS the contract. Microsoft.Extensions.AI's
/// GetResponseAsync&lt;T&gt; derives a JSON schema from it, instructs the model to answer
/// with JSON only, and parses the reply back into T. We then validate the parsed value
/// like any other untrusted input — model output is never trusted blindly.
/// </summary>
public class SuggestThreadTitleQueryHandler : IRequestHandler<SuggestThreadTitleQuery, ThreadTitleDto>
{
    /// <summary>The shape the model is forced to produce. Single source of truth for the schema.</summary>
    private sealed record ThreadTitleResult(string Title, List<string>? Topics);

    private const int MaxTitleLength = 80;
    private const int MaxTopics = 3;
    private const int MaxPromptChars = 500; // cap what we send: cost + prompt-injection surface

    private readonly IChatHistoryStore _historyStore;
    private readonly ILlmFactory _llmFactory;
    private readonly ILogger<SuggestThreadTitleQueryHandler> _logger;

    public SuggestThreadTitleQueryHandler(
        IChatHistoryStore historyStore,
        ILlmFactory llmFactory,
        ILogger<SuggestThreadTitleQueryHandler> logger)
    {
        _historyStore = historyStore;
        _llmFactory = llmFactory;
        _logger = logger;
    }

    public async Task<ThreadTitleDto> Handle(SuggestThreadTitleQuery request, CancellationToken ct)
    {
        var history = await _historyStore.GetHistoryAsync(request.ThreadId);
        var firstUserMessage = history.FirstOrDefault(m => m.Role == ChatRole.User)?.Text;

        if (string.IsNullOrWhiteSpace(firstUserMessage))
            throw new NotFoundException($"Thread '{request.ThreadId}' has no user messages to summarize.");

        var llm = _llmFactory.Create(request.Provider);

        List<ChatMessage> messages =
        [
            new(ChatRole.System,
                "You generate short, specific titles for chat conversations. " +
                "Respond with JSON matching the requested schema only — no prose, no markdown."),
            new(ChatRole.User,
                $"Create a title (max 6 words, no quotes) and 1-{MaxTopics} lowercase topic tags " +
                $"for a conversation that begins with:\n\"{Truncate(firstUserMessage, MaxPromptChars)}\"")
        ];

        ChatResponse<ThreadTitleResult> response;
        try
        {
            // (inside the factory-built pipeline) still gives us timeout + retry for free.
            response = await llm.GetResponseAsync<ThreadTitleResult>(messages, cancellationToken: ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Structured title generation failed. Provider={Provider} ThreadId={ThreadId}",
                request.Provider, request.ThreadId);
            throw new LlmUnavailableException($"The {request.Provider} provider failed to respond.", ex);
        }

        // TryGetResult guards malformed JSON (truncated output, prose around the JSON, wrong shape).
        // Malformed MODEL output is an upstream failure, not the caller's fault → 502, never 400.
        if (!response.TryGetResult(out var result) || string.IsNullOrWhiteSpace(result.Title))
        {
            _logger.LogWarning(
                "Model returned malformed structured output. Provider={Provider} ThreadId={ThreadId} RawLength={RawLength}",
                request.Provider, request.ThreadId, response.Text?.Length ?? 0);
            throw new LlmUnavailableException("The model returned malformed structured output for the thread title.");
        }

        // Schema conformance is not business validity — enforce our own bounds after parsing.
        var title = result.Title.Trim().Trim('"');
        if (title.Length > MaxTitleLength)
            title = title.Substring(0, MaxTitleLength).Trim() + "…";

        var topics = (result.Topics ?? [])
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .Select(t => t.Trim().ToLowerInvariant())
            .Distinct()
            .Take(MaxTopics)
            .ToList();

        // Persist so the title survives reload. Ownership is enforced
        // INSIDE the store, so we just pass UserId — a mismatch is a silent no-op there.
        await _historyStore.UpdateThreadTitleAsync(request.ThreadId, request.UserId, title);

        return new ThreadTitleDto { Title = title, Topics = topics };
    }

    private static string Truncate(string value, int max) =>
        value.Length <= max ? value : value[..max];
}
