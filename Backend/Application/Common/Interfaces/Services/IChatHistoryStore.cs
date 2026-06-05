
using Microsoft.Extensions.AI;

namespace Application.Common.Interfaces.Services;

public interface IChatHistoryStore
{
    Task<List<ChatMessage>> GetHistoryAsync(string threadId);
    Task SaveChatMessageAsync(string threadId, List<ChatMessage> messages);
    Task<string> CreateThreadAsync(string userId);
    Task DeleteThreadAsync(string threadId, string userId);
    Task<List<ChatThreadInfo>> GetAllThreadAsync(string userId);
}

public class ChatThreadInfo
{
    public string ThreadId { get; set; }
    public string UserId { get; set; }
    public string Title { get; set; } = "New chat";
    public DateTime CreatedAt { get; set; }
    public DateTime LastMessageAt { get; set; }
}
