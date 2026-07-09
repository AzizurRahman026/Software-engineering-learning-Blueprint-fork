using Domain.Entities;
using MongoDB.Driver;

namespace Infrastructure.Persistence.Indexing;

/// <summary>
/// Serves PostCommentRepository: find(c => c.PostId == id).
/// Equality filter on one field → single-field index is enough.
/// </summary>
public sealed class PostCommentIndexes : MongoIndexConfiguration<PostComment>
{
    protected override IEnumerable<CreateIndexModel<PostComment>> BuildIndexes()
    {
        yield return new CreateIndexModel<PostComment>(
            Builders<PostComment>.IndexKeys.Ascending(c => c.PostId),
            new CreateIndexOptions { Name = "postcomment_postid" });
    }
}
