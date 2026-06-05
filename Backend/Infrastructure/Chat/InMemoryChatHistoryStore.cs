
using Application.Common.Interfaces.Services;
using Microsoft.Extensions.AI;
using System.Collections.Concurrent;

namespace Infrastructure.Chat;

public class InMemoryChatHistoryStore : IChatHistoryStore
{
    public ConcurrentDictionary<string, List<ChatMessage>> _threads = new();
    public ConcurrentDictionary<string, ChatThreadInfo> _threadInfo = new();

    public async Task<string> CreateThreadAsync(string userId)
    {
        var threadId = Guid.NewGuid().ToString();
        _threads[threadId] = new();
        _threadInfo[threadId] = new ChatThreadInfo
        {
            ThreadId = threadId,
            UserId = userId,
            Title = "New Chat",
            CreatedAt = DateTime.UtcNow,
            LastMessageAt = DateTime.UtcNow
        };

        return await Task.FromResult(threadId);
    }

    public Task DeleteThreadAsync(string threadId, string userId)
    {
        if (_threadInfo.TryGetValue(threadId, out var info) && info.UserId != userId)
            return Task.CompletedTask; // not the owner

        _threadInfo.TryRemove(threadId, out _);
        _threads.TryRemove(threadId, out _);
        return Task.CompletedTask;
    }

    public Task<List<ChatThreadInfo>> GetAllThreadAsync(string userId)
    {
        var threads = _threadInfo
            .Values
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.LastMessageAt)
            .ToList();
        return Task.FromResult(threads);
    }

    public Task<List<ChatMessage>> GetHistoryAsync(string threadId)
    {
        if (_threads.TryGetValue(threadId, out var messages))
        {
            return Task.FromResult(messages);
        }
        return Task.FromResult(new List<ChatMessage>());
    }

    public Task SaveChatMessageAsync(string threadId, List<ChatMessage> messages)
    {
        _threads[threadId] = messages;
        if (_threadInfo.TryGetValue(threadId, out var threadmessagesInfo))
        {
            threadmessagesInfo.LastMessageAt = DateTime.UtcNow;

            var firstUserMessage = messages.FirstOrDefault(u => u.Role == ChatRole.User);
            if (firstUserMessage is not null)
            {
                threadmessagesInfo.Title = firstUserMessage.Text.Length > 40
                    ? firstUserMessage.Text[..50]
                    : firstUserMessage.Text ?? "New chat";
            }
        }
        return Task.CompletedTask;
    }
}
