namespace Engrslan.Domain.Shared.Events;

public abstract class DomainEventBase : IDomainEvent
{
    public DateTime OccurredOn { get; }
    public Guid EventId { get; }

    protected DomainEventBase()
    {
        EventId = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
    }
}