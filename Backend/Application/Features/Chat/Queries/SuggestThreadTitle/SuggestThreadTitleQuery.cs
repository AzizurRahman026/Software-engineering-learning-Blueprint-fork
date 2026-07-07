using Domain.Enums;
using MediatR;

namespace Application.Features.Chat.Queries.SuggestThreadTitle;

/// <summary>
/// Asks the LLM for a short, structured title (+ topic tags) for an existing chat thread and
/// persists it via <see cref="Application.Common.Interfaces.Services.IChatHistoryStore"/> so the
/// title survives reloads (closing the Day 24 gap).
/// </summary>
public class SuggestThreadTitleQuery : IRequest<ThreadTitleDto>
{
    public string ThreadId { get; set; }
    public string UserId { get; set; }          // owner — required so the store can enforce ownership
    public LlmProvider Provider { get; set; } = LlmProvider.Gemini;
}

public class ThreadTitleDto
{
    public string Title { get; set; }
    public List<string> Topics { get; set; } = [];
}
