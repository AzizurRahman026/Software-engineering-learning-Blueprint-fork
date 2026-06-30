namespace Domain.Common;

/// <summary>
/// Marker for something that has already happened inside the Domain and that
/// other parts of the system may want to react to (e.g. send a welcome email).
/// </summary>
public interface IDomainEvent
{
    DateTime OccurredOnUtc { get; }
}
