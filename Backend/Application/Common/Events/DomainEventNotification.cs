using Domain.Common;
using MediatR;

namespace Application.Common.Events;

/// <summary>
/// Wraps a Domain <see cref="IDomainEvent"/> as a MediatR <see cref="INotification"/>
/// so it can be published — keeping MediatR out of the Domain layer.
/// </summary>
public sealed record DomainEventNotification(IDomainEvent DomainEvent) : INotification;
