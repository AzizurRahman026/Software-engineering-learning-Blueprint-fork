using Domain.Entities;
using MongoDB.Driver;

namespace Infrastructure.Persistence.Indexing;

/// <summary>
/// Serves BlogCommentRepository: find(c => c.BlogPostId == id).
/// Equality filter on one field → single-field index is enough.
/// </summary>
public sealed class BlogCommentIndexes : MongoIndexConfiguration<BlogComment>
{
    protected override IEnumerable<CreateIndexModel<BlogComment>> BuildIndexes()
    {
        yield return new CreateIndexModel<BlogComment>(
            Builders<BlogComment>.IndexKeys.Ascending(c => c.BlogPostId),
            new CreateIndexOptions { Name = "blogcomment_blogpostid" });
    }
}
