using Application.Common.Events;
using Domain.Common;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

/// <summary>
/// MediatR-backed dispatcher invoked by the Repository after a successful write.
/// Why IServiceScopeFactory instead of injecting IPublisher directly.
/// Failure isolation: a throwing event handler must NEVER fail the main
/// operation - the write already committed. Failures are logged and swallowed
/// (at-most-once delivery; the outbox pattern is the future at-least-once fix).
/// </summary>
public sealed class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<DomainEventDispatcher> _logger;

    public DomainEventDispatcher(IServiceScopeFactory scopeFactory, ILogger<DomainEventDispatcher> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task DispatchAsync(AggregateRoot aggregate, CancellationToken cancellationToken = default)
    {
        var events = aggregate.GetDomainEvents().ToList();
        if (events.Count == 0)
            return;

        // Clear FIRST so a re-entrant dispatch of the same aggregate can't double-fire.
        aggregate.ClearDomainEvents();

        using var scope = _scopeFactory.CreateScope();
        var publisher = scope.ServiceProvider.GetRequiredService<IPublisher>();

        foreach (var domainEvent in events)
        {
            try
            {
                await publisher.Publish(new DomainEventNotification(domainEvent), cancellationToken);
            }
            catch (Exception ex)
            {
                // Per-event catch: one failing handler must not block the remaining
                // it is visible in monitoring (correlation id comes from the log scope).
                _logger.LogError(ex,
                    "Domain event {DomainEventType} for {AggregateType}/{AggregateId} failed to dispatch and was dropped",
                    domainEvent.GetType().Name, aggregate.GetType().Name, aggregate.Id);
            }
        }
    }
}
