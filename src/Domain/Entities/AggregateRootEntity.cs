using Engrslan.Events;
using Engrslan.Interfaces;

namespace Engrslan.Entities;

public abstract class AggregateRoot<T> : Entity<T>, IAggregateRoot<T>
{
    private readonly List<IDomainEvent> _domainEvents = [];

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    public IEnumerable<IDomainEvent> GetAndClearDomainEvents()
    {
        var events = _domainEvents.ToList();
        _domainEvents.Clear();
        return events;
    }
}
public abstract class AggregateRoot : AggregateRoot<int>;

public abstract class CreationAuditedAggregateRoot<T> : AggregateRoot<T>, ICreationAuditedAggregateRoot<T>
{
    public Guid? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
}

public abstract class CreationAuditedAggregateRoot : CreationAuditedAggregateRoot<int>, ICreationAuditedAggregateRoot;

public abstract class AuditedAggregateRoot<T> : CreationAuditedAggregateRoot<T>, IAuditedAggregateRoot<T>
{
    public Guid? ModifiedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }
}

public abstract class AuditedAggregateRoot : AuditedAggregateRoot<int>, IAuditedAggregateRoot;

public abstract class FullAuditedAggregateRoot<T> : AuditedAggregateRoot<T>, IFullAuditedAggregateRoot<T>
{
    public Guid? DeletedBy { get; set; }
    public DateTime? DeletedAt { get; set; }
    public bool IsDeleted { get; set; }
}

public abstract class FullAuditedAggregateRoot : FullAuditedAggregateRoot<int>,
    IFullAuditedAggregateRoot;
