using Domain.Enums;
using MediatR;

namespace Application.Features.Chat.Queries.SuggestThreadTitle;

/// <summary>
/// Asks the LLM for a short, structured title (+ topic tags) for an existing chat thread.
/// Read-only: it does not mutate the thread — persisting the title is a follow-up step.
/// </summary>
public class SuggestThreadTitleQuery : IRequest<ThreadTitleDto>
{
    public string ThreadId { get; set; }
    public LlmProvider Provider { get; set; } = LlmProvider.Gemini;
}

public class ThreadTitleDto
{
    public string Title { get; set; }
    public List<string> Topics { get; set; } = [];
}
