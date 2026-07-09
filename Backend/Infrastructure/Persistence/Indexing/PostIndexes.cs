using Domain.Entities;
using MongoDB.Driver;

namespace Infrastructure.Persistence.Indexing;

/// <summary>
/// Serves PostRepository.GetPagedAsync (public feed): find(Status == Published) +
/// sort(PublishedAt desc) + skip/limit. Compound (Status, PublishedAt) so the filter
/// and sort are both index-served. Without it: COLLSCAN + blocking in-memory sort.
/// </summary>
public sealed class PostIndexes : MongoIndexConfiguration<Post>
{
    protected override IEnumerable<CreateIndexModel<Post>> BuildIndexes()
    {
        yield return new CreateIndexModel<Post>(
            Builders<Post>.IndexKeys
                .Ascending(p => p.Status)
                .Descending(p => p.PublishedAt),
            new CreateIndexOptions { Name = "post_status_publishedat_desc" });
    }
}
