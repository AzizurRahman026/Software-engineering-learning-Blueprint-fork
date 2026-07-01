using Application.Common.Events;
using Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Auth.Events;

/// <summary>
/// Reacts to domain events published after a successful write. Side-effects
/// (welcome email, analytics, audit) live HERE, out of SignupCommandHandler —
/// the command handler no longer knows or cares who reacts. As more event types
/// arrive, add more branches here (or split into per-event handlers later).
/// </summary>
public sealed class DomainEventHandler : INotificationHandler<DomainEventNotification>
{
    private readonly ILogger<DomainEventHandler> _logger;

    public DomainEventHandler(ILogger<DomainEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(DomainEventNotification notification, CancellationToken cancellationToken)
    {
        switch (notification.DomainEvent)
        {
            case UserRegisteredEvent e:
                // TODO: queue a real welcome email via IEmailSender. For now, log the intent.
                // Email is PII — log the UserId, not the address.
                _logger.LogInformation(
                    "UserRegistered side-effect: queueing welcome email for {UserId} ({Username})",
                    e.UserId, e.Username);
                break;
        }

        return Task.CompletedTask;
    }
}
