namespace Engrslan.Events;

public class EntityCreatedEvent : DomainEventBase
{
    public Guid EntityId { get; }
    public string EntityType { get; }
    public string CreatedBy { get; }

    public EntityCreatedEvent(Guid entityId, string entityType, string createdBy)
    {
        EntityId = entityId;
        EntityType = entityType;
        CreatedBy = createdBy;
    }
}