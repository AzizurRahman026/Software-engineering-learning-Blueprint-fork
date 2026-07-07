using Domain.Entities;
using MongoDB.Driver;

namespace Infrastructure.Persistence.Indexing;

/// <summary>
/// One compound index serves both BlogLikeRepository queries — (BlogPostId, UserId)
/// and (BlogPostId) alone — since compound indexes answer left-to-right prefixes.
/// NOT unique: ToggleLike enforces one-like-per-user in the handler and doesn't
/// handle a duplicate-key exception today — make it unique consciously, not by accident.
/// </summary>
public sealed class BlogLikeIndexes : MongoIndexConfiguration<BlogLike>
{
    protected override IEnumerable<CreateIndexModel<BlogLike>> BuildIndexes()
    {
        yield return new CreateIndexModel<BlogLike>(
            Builders<BlogLike>.IndexKeys
                .Ascending(l => l.BlogPostId)
                .Ascending(l => l.UserId),
            new CreateIndexOptions { Name = "bloglike_blogpostid_userid" });
    }
}
