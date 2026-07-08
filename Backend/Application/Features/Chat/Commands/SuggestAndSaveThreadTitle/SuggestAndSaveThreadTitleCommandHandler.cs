using Application.Common.Ai;
using Application.Common.Interfaces.Services;
using Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace Application.Features.Chat.Commands.SuggestAndSaveThreadTitle;

/// <summary>
/// Structured-output pattern: the C# record IS the contract. Microsoft.Extensions.AI's
/// GetResponseAsync&lt;T&gt; derives a JSON schema from it, instructs the model to answer
/// with JSON only, and parses the reply back into T. We then validate the parsed value
/// like any other untrusted input — model output is never trusted blindly. The validated
/// title is then persisted, which is why this is a Command (see the command doc-comment).
/// </summary>
public class SuggestAndSaveThreadTitleCommandHandler
    : IRequestHandler<SuggestAndSaveThreadTitleCommand, ThreadTitleDto>
{
    /// <summary>The shape the model is forced to produce. Single source of truth for the schema.</summary>
    private sealed record ThreadTitleResult(string Title, List<string>? Topics);

    private const int MaxTitleLength = 80;
    private const int MaxTopics = 3;
    private const int MaxPromptChars = 500; // cap what we send: cost + prompt-injection surface

    private readonly IChatHistoryStore _historyStore;
    private readonly ILlmFactory _llmFactory;
    private readonly ILogger<SuggestAndSaveThreadTitleCommandHandler> _logger;

    public SuggestAndSaveThreadTitleCommandHandler(
        IChatHistoryStore historyStore,
        ILlmFactory llmFactory,
        ILogger<SuggestAndSaveThreadTitleCommandHandler> logger)
    {
        _historyStore = historyStore;
        _llmFactory = llmFactory;
        _logger = logger;
    }

    public async Task<ThreadTitleDto> Handle(SuggestAndSaveThreadTitleCommand request, CancellationToken ct)
    {
        var history = await _historyStore.GetHistoryAsync(request.ThreadId);
        var firstUserMessage = history.FirstOrDefault(m => m.Role == ChatRole.User)?.Text;

        if (string.IsNullOrWhiteSpace(firstUserMessage))
            throw new NotFoundException($"Thread '{request.ThreadId}' has no user messages to summarize.");

        var llm = _llmFactory.Create(request.Provider);

        // INPUT guard: fence the untrusted user message as DATA so injection attempts
        // ("ignore previous instructions...") are harder to land. Defense in depth, not a
        // guarantee — the OUTPUT guard below still assumes the model can be fooled.
        var fencedFirstMessage =
            LlmContentGuard.WrapUntrustedContent(Truncate(firstUserMessage, MaxPromptChars));

        List<ChatMessage> messages =
        [
            new(ChatRole.System,
                "You generate short, specific titles for chat conversations. " +
                "Respond with JSON matching the requested schema only — no prose, no markdown. " +
                LlmContentGuard.FenceInstruction),
            new(ChatRole.User,
                $"Create a title (max 6 words, no quotes) and 1-{MaxTopics} lowercase topic tags " +
                $"for a conversation that begins with:\n{fencedFirstMessage}")
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
        if (!response.TryGetResult(out var result) || string.IsNullOrWhiteSpace(result.Title))
        {
            _logger.LogWarning(
                "Model returned malformed structured output. Provider={Provider} ThreadId={ThreadId} RawLength={RawLength}",
                request.Provider, request.ThreadId, response.Text?.Length ?? 0);
            throw new LlmUnavailableException("The model returned malformed structured output for the thread title.");
        }

        // OUTPUT guard: parsed != safe. The title renders in the UI (HTML/markdown) and logs,
        var title = LlmContentGuard.SanitizeDisplayText(result.Title, MaxTitleLength, fallback: "New chat");

        // Topics are model output too and also render in the UI — sanitize each the same way,
        // then drop any that sanitized to nothing before de-duping.
        var topics = (result.Topics ?? [])
            .Select(t => LlmContentGuard.SanitizeDisplayText(t, maxLength: 30, fallback: string.Empty).ToLowerInvariant())
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .Distinct()
            .Take(MaxTopics)
            .ToList();

        // Persist so the title survives reload.
        await _historyStore.UpdateThreadTitleAsync(request.ThreadId, request.UserId, title);

        return new ThreadTitleDto { Title = title, Topics = topics };
    }

    private static string Truncate(string value, int max) =>
        value.Length <= max ? value : value[..max];
}
