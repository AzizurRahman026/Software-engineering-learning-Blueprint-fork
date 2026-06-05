using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Services;
using Domain.Entities;
using Microsoft.Extensions.AI;
using System.Text.Json;

namespace Infrastructure.Chat;

/// <summary>
/// MongoDB-backed chat history store. One <see cref="ChatThread"/> document per thread,
/// holding the full message list serialized as JSON so the agentic loop (tool calls /
/// tool results) survives restarts. Threads are scoped to the owning user.
/// </summary>
public class MongoChatHistoryStore : IChatHistoryStore
{
    private readonly IDatabaseContext _db;

    // Polymorphic options that know how to (de)serialize ChatMessage / AIContent.
    private static readonly JsonSerializerOptions _jsonOptions = AIJsonUtilities.DefaultOptions;

    public MongoChatHistoryStore(IDatabaseContext db)
    {
        _db = db;
    }

    public async Task<string> CreateThreadAsync(string userId)
    {
        var now = DateTime.UtcNow;
        var thread = new ChatThread
        {
            UserId = userId,
            Title = "New chat",
            CreatedAt = now,
            LastMessageAt = now,
            MessagesJson = "[]"
        };
        await _db.AddAsync(thread);
        return thread.Id;
    }

    public async Task DeleteThreadAsync(string threadId, string userId)
    {
        var thread = await _db.GetItemByConditionAsync<ChatThread>(t => t.Id == threadId);
        if (thread is null || thread.UserId != userId)
            return; // not found, or not the owner — nothing to do

        await _db.DeleteAsync(thread);
    }

    public async Task<List<ChatThreadInfo>> GetAllThreadAsync(string userId)
    {
        var threads = await _db.GetItemsByConditionAsync<ChatThread>(t => t.UserId == userId)
                      ?? new List<ChatThread>();

        return threads
            .OrderByDescending(t => t.LastMessageAt)
            .Select(t => new ChatThreadInfo
            {
                ThreadId = t.Id,
                Title = t.Title,
                CreatedAt = t.CreatedAt,
                LastMessageAt = t.LastMessageAt
            })
            .ToList();
    }

    public async Task<List<ChatMessage>> GetHistoryAsync(string threadId)
    {
        var thread = await _db.GetItemByConditionAsync<ChatThread>(t => t.Id == threadId);
        if (thread is null || string.IsNullOrWhiteSpace(thread.MessagesJson))
            return new List<ChatMessage>();

        return JsonSerializer.Deserialize<List<ChatMessage>>(thread.MessagesJson, _jsonOptions)
               ?? new List<ChatMessage>();
    }

    public async Task SaveChatMessageAsync(string threadId, List<ChatMessage> messages)
    {
        var thread = await _db.GetItemByConditionAsync<ChatThread>(t => t.Id == threadId);
        if (thread is null)
            return;

        thread.MessagesJson = JsonSerializer.Serialize(messages, _jsonOptions);
        thread.LastMessageAt = DateTime.UtcNow;

        var firstUserMessage = messages.FirstOrDefault(m => m.Role == ChatRole.User);
        if (firstUserMessage is not null && !string.IsNullOrWhiteSpace(firstUserMessage.Text))
        {
            thread.Title = firstUserMessage.Text.Length > 50
                ? firstUserMessage.Text[..50]
                : firstUserMessage.Text;
        }

        await _db.UpdateAsync(thread);
    }
}
