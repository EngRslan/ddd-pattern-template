using CertManager.Domain.Interfaces;
using CertManager.Domain.Shared.Events;

namespace CertManager.Domain.Entities;

public abstract class AggregateRootEntity<T> : Entity<T>, IAggregateRootEntity<T>
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
public abstract class AggregateRootEntity : AggregateRootEntity<int>, IAggregateRootEntity;

public abstract class CreationAggregateRootEntity<T> : AggregateRootEntity<T>, ICreationAggregateRootEntity<T>
{
    public Guid? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
}

public abstract class CreationAggregateRootEntity : CreationAggregateRootEntity<int>, ICreationAggregateRootEntity;

public abstract class AuditableAggregateRootEntity<T> : CreationAggregateRootEntity<T>, IAuditableAggregateRootEntity<T>
{
    public Guid? UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public abstract class AuditableAggregateRootEntity : AuditableAggregateRootEntity<int>, IAuditableAggregateRootEntity;

public abstract class FullAuditableAggregateRootEntity<T> : AuditableAggregateRootEntity<T>, IFullAuditableAggregateRootEntity<T>
{
    public Guid? DeletedBy { get; set; }
    public DateTime? DeletedAt { get; set; }
    public bool IsDeleted { get; set; }
}

public abstract class FullAuditableAggregateRootEntity : FullAuditableAggregateRootEntity<int>,
    IFullAuditableAggregateRootEntity;
