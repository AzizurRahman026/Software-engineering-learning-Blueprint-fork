using MongoDB.Driver;

namespace Infrastructure.Persistence.Indexing;

/// <summary>
/// Base class: subclasses only declare WHICH indexes they want (BuildIndexes);
/// the create mechanics live here, written once. Note CreateManyAsync never
/// MODIFIES an index — changing keys/options/name requires dropping the old one.
/// </summary>
public abstract class MongoIndexConfiguration<T> : IMongoIndexConfiguration
{
    // Mirrors DatabaseContext.GetCollection<T>'s naming convention.
    public string CollectionName => typeof(T).Name.ToLower();

    /// <summary>Declare the indexes for this collection. Pure data, no IO.</summary>
    protected abstract IEnumerable<CreateIndexModel<T>> BuildIndexes();

    public async Task<IEnumerable<string>> ApplyAsync(DatabaseContext ctx, CancellationToken ct)
    {
        var collection = ctx.GetCollection<T>();
        return await collection.Indexes.CreateManyAsync(BuildIndexes(), cancellationToken: ct);
    }
}
