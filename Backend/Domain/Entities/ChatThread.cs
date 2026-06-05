
namespace Domain.Entities;

// One document per chat thread. BaseEntity.Id is used as the threadId.
public class ChatThread : BaseEntity
{
    public string UserId { get; set; }          // owner — for per-user scoping
    public string Title { get; set; } = "New chat";
    public DateTime CreatedAt { get; set; }
    public DateTime LastMessageAt { get; set; }
    public string MessagesJson { get; set; } = "[]"; // serialized List<ChatMessage>
}
