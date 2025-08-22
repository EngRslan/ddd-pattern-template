namespace Engrslan.Domain.Shared.Events;

public interface IDomainEvent
{
    DateTime OccurredOn { get; }
    Guid EventId { get; }
}