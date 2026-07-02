using Domain.Common;

namespace Application.Common.Events;

/// <summary>
/// Drains an aggregate root's pending domain events and publishes them.
/// Called by the persistence boundary (Repository) AFTER a successful write.
/// Takes AggregateRoot - not BaseEntity - so only event-capable aggregates
/// can ever reach the dispatcher; plain entities are excluded at compile time.
/// </summary>
public interface IDomainEventDispatcher
{
    Task DispatchAsync(AggregateRoot aggregate, CancellationToken cancellationToken = default);
}
