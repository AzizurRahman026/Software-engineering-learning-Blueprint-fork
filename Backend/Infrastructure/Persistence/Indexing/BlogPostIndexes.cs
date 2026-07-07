using Domain.Entities;
using MongoDB.Driver;

namespace Infrastructure.Persistence.Indexing;

/// <summary>
/// Serves BlogPostRepository.GetPagedAsync: unfiltered find + sort(CreatedAt desc)
/// + skip/limit. Without it: COLLSCAN + blocking in-memory sort (hard 100 MB cap).
/// </summary>
public sealed class BlogPostIndexes : MongoIndexConfiguration<BlogPost>
{
    protected override IEnumerable<CreateIndexModel<BlogPost>> BuildIndexes()
    {
        yield return new CreateIndexModel<BlogPost>(
            Builders<BlogPost>.IndexKeys.Descending(p => p.CreatedAt),
            new CreateIndexOptions { Name = "blogpost_createdat_desc" });
    }
}
