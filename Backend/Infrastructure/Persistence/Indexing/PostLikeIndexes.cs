using Domain.Entities;
using MongoDB.Driver;

namespace Infrastructure.Persistence.Indexing;

/// <summary>
/// One compound index serves both PostLikeRepository queries — (PostId, UserId)
/// and (PostId) alone — since compound indexes answer left-to-right prefixes.
/// NOT unique: ToggleLike enforces one-like-per-user in the handler and doesn't
/// handle a duplicate-key exception today — make it unique consciously, not by accident.
/// </summary>
public sealed class PostLikeIndexes : MongoIndexConfiguration<PostLike>
{
    protected override IEnumerable<CreateIndexModel<PostLike>> BuildIndexes()
    {
        yield return new CreateIndexModel<PostLike>(
            Builders<PostLike>.IndexKeys
                .Ascending(l => l.PostId)
                .Ascending(l => l.UserId),
            new CreateIndexOptions { Name = "postlike_postid_userid" });
    }
}
