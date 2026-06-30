using System;
using System.Collections.Generic;
using Domain.Common;

namespace Domain.Entities;

public abstract class BaseEntity
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Events are in-memory only, never persisted.</summary>
    private readonly List<IDomainEvent> _domainEvents = new();

    /// <summary>Events raised by this entity since it was loaded/created, awaiting dispatch.</summary>
    public IReadOnlyCollection<IDomainEvent> GetDomainEvents() => _domainEvents.AsReadOnly();

    /// <summary>Records something that happened. Only the entity itself may call this.</summary>
    protected void RaiseDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);

    /// <summary>Called after the events have been dispatched, so they fire only once.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    public override bool Equals(object? obj)
    {
        if (obj is not BaseEntity other)
        {
            return false;
        }
        if (ReferenceEquals(this, other))
        {
            return true;
        }
        if (GetType() != other.GetType())
        {
            return false;
        }
        return Id == other.Id;
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
}
