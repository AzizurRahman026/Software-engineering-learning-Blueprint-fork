using Application.Common.Events;
using Application.Common.Interfaces.Persistence;
using Domain.Common;
using Domain.Entities;
using Domain.Repositories.Base;
using Infrastructure.Persistence;
using System.Linq.Expressions;

namespace Infrastructure.Repositories.Base;

public class Repository : IRepository
{
    protected readonly IDatabaseContext DbContext;
    private readonly IDomainEventDispatcher _domainEventDispatcher;

    public Repository(IDatabaseContext dbContext, IDomainEventDispatcher domainEventDispatcher)
    {
        DbContext = dbContext;
        _domainEventDispatcher = domainEventDispatcher;
    }

    // The persistence boundary owns domain-event dispatch, but it is OPT-IN:
    // events fire only when the entity is an AggregateRoot AND its domain logic
    // explicitly raised an event (e.g. User.Register). Plain entities and
    // event-less writes skip the dispatcher entirely — a type check, nothing more.
    public async Task<bool> AddAsync<T>(T entity) where T : BaseEntity
    {
        var succeeded = await DbContext.AddAsync<T>(entity);
        if (succeeded)
            await DispatchDomainEventsIfAnyAsync(entity);
        return succeeded;
    }

    public async Task<bool> DeleteAsync<T>(T entity) where T : BaseEntity
    {
        var succeeded = await DbContext.DeleteAsync<T>(entity);
        if (succeeded)
            await DispatchDomainEventsIfAnyAsync(entity);
        return succeeded;
    }

    // NOTE: no entity instance here, so any events raised on the aggregate
    // cannot be dispatched via this path. Prefer DeleteAsync(entity) for
    // aggregates that raise deletion events.
    public async Task<bool> DeleteByIdAsync<T>(string Id) where T : BaseEntity
    {
        return await DbContext.DeleteByIdAsync<T>(Id);
    }

    public async Task<List<T>> GetAllAsync<T>() where T : class
    {
        return await DbContext.GetAllAsync<T>();
    }

    public async Task<T?> GetByIdAsync<T>(string userId) where T : BaseEntity
    {
        return await DbContext.GetItemByConditionAsync<T>(u => u.Id == userId);
    }

    public async Task<T?> GetItemByConditionAsync<T>(Expression<Func<T, bool>> criteria) where T : BaseEntity
    {
        return await DbContext.GetItemByConditionAsync<T>(criteria);
    }

    public async Task<List<T>?> GetItemsByConditionAsync<T>(Expression<Func<T, bool>> criteria) where T : BaseEntity
    {
        return await DbContext.GetItemsByConditionAsync<T>(criteria);
    }

    public async Task<bool> UpdateAsync<T>(T entity) where T : BaseEntity
    {
        var succeeded = await DbContext.UpdateAsync<T>(entity);
        if (succeeded)
            await DispatchDomainEventsIfAnyAsync(entity);
        return succeeded;
    }

    private Task DispatchDomainEventsIfAnyAsync(BaseEntity entity)
        => entity is AggregateRoot { HasDomainEvents: true } aggregate
            ? _domainEventDispatcher.DispatchAsync(aggregate)
            : Task.CompletedTask;
    
    public async Task<List<T>> GetPagedAsync<T>(Expression<Func<T, bool>>? filter = null,
        int pageNumber = 1,
        int pageSize = 10,
        Expression<Func<T, object>>? orderBy = null,
        bool ascending = true)
    {
        return await DbContext.GetPagedResponseAsync<T>(filter, pageNumber, pageSize, orderBy, ascending);
    }

    public async Task<long> CountAsync<T>(Expression<Func<T, bool>> filter) where T : class
    {
        return await DbContext.CountAsync<T>(filter);
    }
}
