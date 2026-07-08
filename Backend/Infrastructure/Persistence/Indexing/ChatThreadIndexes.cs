using Domain.Entities;
using MongoDB.Driver;

namespace Infrastructure.Persistence.Indexing;

/// <summary>
/// Serves MongoChatHistoryStore.GetAllThreadAsync: filter(UserId == userId) then
public sealed class ChatThreadIndexes : MongoIndexConfiguration<ChatThread>
{
    protected override IEnumerable<CreateIndexModel<ChatThread>> BuildIndexes()
    {
        yield return new CreateIndexModel<ChatThread>(
            Builders<ChatThread>.IndexKeys
                .Ascending(t => t.UserId)
                .Descending(t => t.LastMessageAt),
            new CreateIndexOptions { Name = "chatthread_userid_lastmessageat_desc" });
    }
}
