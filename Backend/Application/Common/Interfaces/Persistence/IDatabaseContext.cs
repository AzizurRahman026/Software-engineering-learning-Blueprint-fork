using Domain.Entities;
using System.Linq.Expressions;

namespace Application.Common.Interfaces.Persistence;

public interface IDatabaseContext : IDisposable
{
    // CRUD Operations
    /// <summary>
    /// Retrieves all entities of type T from the database
    /// </summary>
    Task<List<T>> GetAllAsync<T>() where T : class;

    /// <summary>
    /// Adds a new entity to the database
    /// </summary>
    Task<bool> AddAsync<T>(T entity) where T : BaseEntity;

    /// <summary>
    /// Updates an existing entity in the database
    /// </summary>
    Task<bool> UpdateAsync<T>(T entity) where T : BaseEntity;

    /// <summary>
    /// Deletes an entity from the database
    /// </summary>
    Task<bool> DeleteAsync<T>(T entity) where T : BaseEntity;

    /// <summary>
    /// Deletes an entity by its ID
    /// </summary>
    Task<bool> DeleteByIdAsync<T>(string id) where T : BaseEntity;

    /// <summary>
    /// Deletes multiple entities from the database
    /// </summary>
    Task<int> DeleteManyAsync<T>(IEnumerable<T> entities) where T : BaseEntity;

    /// <summary>
    /// Retrieves a single entity that matches the given criteria
    /// </summary>
    Task<T?> GetItemByConditionAsync<T>(Expression<Func<T, bool>> criteria) where T : BaseEntity;

    /// <summary>
    /// Retrieves all entities that match the given criteria
    /// </summary>
    Task<List<T>?> GetItemsByConditionAsync<T>(Expression<Func<T, bool>> criteria) where T : BaseEntity;

    /// <summary>
    /// Counts the number of entities that match the given criteria
    /// </summary>
    Task<long> CountAsync<T>(Expression<Func<T, bool>> criteria) where T : class;

    /// <summary>
    /// Retrieves a paginated list of entities using offset-based pagination
    /// </summary>
    Task<List<T>> GetPagedResponseAsync<T>(
        Expression<Func<T, bool>>? criteria = null,
        int pageNumber = 1,
        int pageSize = 10,
        Expression<Func<T, object>>? orderBy = null,
        bool ascending = true);

    /// <summary>
    /// Retrieves a paginated list of entities using cursor-based pagination
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    /// <typeparam name="TCursor">The cursor field type (e.g., string, DateTime, int)</typeparam>
    /// <param name="cursorSelector">Expression to select the cursor field</param>
    /// <param name="cursor">The cursor value to start from (null for first page)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="criteria">Optional filter criteria</param>
    /// <param name="ascending">Sort direction</param>
    /// <returns>A tuple containing the items and the next cursor value</returns>
    Task<(List<T>, string)> GetCursorPagedResponseAsync<T>(
        string? lastId = null,
        int pageSize = 10,
        Expression<Func<T, bool>>? criteria = null,
        bool ascending = true,
        CancellationToken cancellationToken = default) where T : BaseEntity;


    // ========== TRANSACTION MANAGEMENT ==========
    /// <summary>
    /// Begin a transaction
    /// </summary>
    IDatabaseContext BeginTransaction();
    /// <summary>
    /// Commit transaction
    /// </summary>
    Task CommitTransactionAsync();
    /// <summary>
    /// Rollback transaction
    /// </summary>
    Task AbortTransactionAsync();
    void Dispose();
}
