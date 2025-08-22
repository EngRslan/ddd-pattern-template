using Engrslan.Domain.Shared.Events;

namespace Engrslan.Domain.Interfaces;

public interface IAggregateRootEntity : IEntity
{
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }
    void AddDomainEvent(IDomainEvent domainEvent);
    void ClearDomainEvents();
    IEnumerable<IDomainEvent> GetAndClearDomainEvents();
}

public interface IAggregateRootEntity<out T> : IAggregateRootEntity;
public interface ICreationAggregateRootEntity<out T> : IAggregateRootEntity<T>, ICreationEntity<T>;
public interface ICreationAggregateRootEntity : IAggregateRootEntity<int>, ICreationEntity<int>;
public interface IAuditableAggregateRootEntity<out T> : IAggregateRootEntity<T>, IAuditableEntity<T>;
public interface IAuditableAggregateRootEntity : IAggregateRootEntity<int>, IAuditableEntity<int>;
public interface IFullAuditableAggregateRootEntity<out T> : IAggregateRootEntity<T>, IFullAuditableEntity<T>;
public interface IFullAuditableAggregateRootEntity : IAggregateRootEntity<int>, IFullAuditableEntity<int>;


