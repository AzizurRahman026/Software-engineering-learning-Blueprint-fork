using Application.Common.Interfaces.Publisher;
using Application.Common.Interfaces.Services;
using Application.Features.Chat.Commands;
using Application.Features.Chat.DTOs;
using Application.Tools.DTOs;
using Application.Tools.Queries;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.AI;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private const string UserIdHeader = "X-User-Id";

    private readonly IMessageBus _messageBus;
    private readonly IChatHistoryStore _historyStore;

    public ChatController(IMessageBus messageBus
        , IChatHistoryStore chatHistoryStore)
    {
        _messageBus = messageBus;
        _historyStore = chatHistoryStore;
    }

    // Reads the logged-in user's id from the X-User-Id header (set by the Angular interceptor).
    private string? GetUserId() =>
        Request.Headers.TryGetValue(UserIdHeader, out var value) && !string.IsNullOrWhiteSpace(value)
            ? value.ToString()
            : null;

    // ── POST /api/chat ───────────────────────────────────────────────────────
    // Request:  { "query": "What is the weather in London?", "provider": "Gemini" }
    // Response: { "answer": "...", "provider": "Gemini", "toolCalls": [...] }

    [HttpPost]
    public async Task<IActionResult> Chat([FromBody] ChatRequestDto request)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized("Missing X-User-Id header.");

        var command = new SendChatCommand
        {
            ThreadId = request.ThreadId,
            UserId = userId,
            Query = request.Query,
            Provider = request.Provider
        };
        var result = await _messageBus.SendAsync<SendChatCommand, ChatResponseDto>(command);
        return Ok(result);
    }

    // ── GET /api/chat/threads — list the current user's threads ──────
    [HttpGet("threads")]
    public async Task<IActionResult> GetThreads()
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized("Missing X-User-Id header.");

        var threads = await _historyStore.GetAllThreadAsync(userId);
        return Ok(threads);
    }

    // ── GET /api/chat/threads/{threadId}/messages — messages of a thread ──
    [HttpGet("threads/{threadId}/messages")]
    public async Task<IActionResult> GetThreadMessages(string threadId)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized("Missing X-User-Id header.");

        var history = await _historyStore.GetHistoryAsync(threadId);

        // Project to a simple UI shape: only user prompts and final assistant text replies.
        var messages = history
            .Where(m => m.Role == ChatRole.User || m.Role == ChatRole.Assistant)
            .Select(m => new
            {
                text = m.Text,
                sender = m.Role == ChatRole.User ? "user" : "ai"
            })
            .Where(m => !string.IsNullOrWhiteSpace(m.text)) // skip assistant tool-call-only turns
            .ToList();

        return Ok(messages);
    }

    // ── DELETE /api/chat/threads/{threadId} — delete a thread ────────
    [HttpDelete("threads/{threadId}")]
    public async Task<IActionResult> DeleteThread(string threadId)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized("Missing X-User-Id header.");

        await _historyStore.DeleteThreadAsync(threadId, userId);
        return NoContent();
    }

    // ── GET /api/chat/tools ──────────────────────────────────────────────────
    // Returns the list of tools the MCP server currently exposes.
    // Useful for debugging — see what tools Gemini/Claude can use.
    [HttpGet("tools")]
    public async Task<IActionResult> GetTools()
    {
        var query = new GetAvailableToolsQuery();
        var tools = await _messageBus.SendAsync<GetAvailableToolsQuery, List<ToolSummary>>(query);
        return Ok(tools);
    }
}
