
using Domain.Enums;

namespace Application.Features.Chat.DTOs;

public class ChatRequestDto
{
    public string? ThreadId { get; set; }
    public string Query { get; set; }
    public LlmProvider Provider { get; set; } = LlmProvider.Gemini;
}

