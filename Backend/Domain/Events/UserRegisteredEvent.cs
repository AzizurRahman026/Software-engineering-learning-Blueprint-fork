using Domain.Common;

namespace Domain.Events;

/// <summary>
/// Raised when a brand-new User is created via User.Register.
/// </summary>
public sealed record UserRegisteredEvent(string UserId, string Username, string Email) : IDomainEvent
{
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}
