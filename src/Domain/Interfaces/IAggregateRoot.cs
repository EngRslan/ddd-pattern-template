using Engrslan.Events;

namespace Engrslan.Interfaces;

public interface IAggregateRoot : IEntity
{
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }
    void AddDomainEvent(IDomainEvent domainEvent);
    void ClearDomainEvents();
    IEnumerable<IDomainEvent> GetAndClearDomainEvents();
}

public interface IAggregateRoot<out T> : IAggregateRoot;
public interface ICreationAuditedAggregateRoot<out T> : IAggregateRoot<T>, ICreationAuditedEntity<T>;
public interface ICreationAuditedAggregateRoot : IAggregateRoot<int>, ICreationAuditedEntity<int>;
public interface IAuditedAggregateRoot<out T> : IAggregateRoot<T>, IAuditedEntity<T>;
public interface IAuditedAggregateRoot : IAggregateRoot<int>, IAuditedEntity<int>;
public interface IFullAuditedAggregateRoot<out T> : IAggregateRoot<T>, IFullAuditedEntity<T>;
public interface IFullAuditedAggregateRoot : IAggregateRoot<int>, IFullAuditedEntity<int>;


