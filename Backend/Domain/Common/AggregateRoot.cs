using Domain.Entities;

namespace Domain.Common;

/// <summary>
/// Base class for aggregate roots - the ONLY entities that can raise domain events.
/// This is the opt-in switch for event dispatch: the persistence boundary publishes
/// events exclusively for aggregates that explicitly raised something, so ordinary
/// entity writes (BlogLike, Chapter, ...) never trigger any publishing at all.
/// </summary>
public abstract class AggregateRoot : BaseEntity
{
    /// <summary>Events are in-memory only, never persisted (get-only members are not mapped by the MongoDB driver).</summary>
    private readonly List<IDomainEvent> _domainEvents = new();

    /// <summary>Cheap guard the Repository checks before involving the dispatcher.</summary>
    public bool HasDomainEvents => _domainEvents.Count > 0;

    /// <summary>Events raised by this aggregate since it was loaded/created, awaiting dispatch.</summary>
    public IReadOnlyCollection<IDomainEvent> GetDomainEvents() => _domainEvents.AsReadOnly();

    /// <summary>Records something that happened. Only the aggregate itself may call this.</summary>
    protected void RaiseDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);

    /// <summary>Called after the events have been dispatched, so they fire only once.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();
}
