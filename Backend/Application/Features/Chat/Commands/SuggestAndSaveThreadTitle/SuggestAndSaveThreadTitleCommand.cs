using Domain.Enums;
using MediatR;

namespace Application.Features.Chat.Commands.SuggestAndSaveThreadTitle;

/// <summary>
/// Asks the LLM for a short, structured title (+ topic tags) for an existing chat thread AND
/// persists it. This is a <b>Command</b>, not a Query, because it mutates state (writes the title):
/// the Day 25 version lived under Queries and wrote as a side effect — a CQRS smell where a "read"
/// silently changed data. Naming it a Command makes the write honest and keeps read handlers
/// side-effect-free. See docs/adr/0001-thread-title-command-and-partial-update.md.
/// </summary>
public class SuggestAndSaveThreadTitleCommand : IRequest<ThreadTitleDto>
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
