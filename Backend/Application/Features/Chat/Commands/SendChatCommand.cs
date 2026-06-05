
using Application.Features.Chat.DTOs;
using Domain.Enums;
using MediatR;

namespace Application.Features.Chat.Commands;

public class SendChatCommand : IRequest<ChatResponseDto>
{
    public string? ThreadId { get; set; }
    public string? UserId { get; set; } // owner of the thread (per-user history)
    public string Query { get; set; } // user's question
    public LlmProvider Provider { get; set; } // which LLM to use (e.g. Gemini, Claude)
}
