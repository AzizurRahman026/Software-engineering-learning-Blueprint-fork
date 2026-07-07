namespace Infrastructure.Persistence.Indexing;

/// <summary>
/// Declares one collection's indexes. MongoIndexInitializer consumes all
/// implementations, so indexing a new collection = a new implementation —
/// new code, not modified code (OCP).
/// </summary>
public interface IMongoIndexConfiguration
{
    /// <summary>Collection name, used only for logging.</summary>
    string CollectionName { get; }

    /// <summary>Creates this collection's indexes (idempotent). Returns the index names.</summary>
    Task<IEnumerable<string>> ApplyAsync(DatabaseContext ctx, CancellationToken ct);
}
