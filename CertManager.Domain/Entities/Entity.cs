using CertManager.Domain.Interfaces;

namespace CertManager.Domain.Entities;

public abstract class Entity<T> : IEntity<T>
{
    public T Id { get; set; } = default!;
}

public abstract class Entity : Entity<int>
{ }

public abstract class CreationEntity<T> : Entity<T>, ICreationEntity<T>
{
    public Guid? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
}

public abstract class AuditableEntity<T> : CreationEntity<T>, IAuditableEntity<T>
{
    public Guid? UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public abstract class FullAuditableEntity<T> : AuditableEntity<T>, IFullAuditableEntity<T>
{
    public Guid? DeletedBy { get; set; }
    public DateTime? DeletedAt { get; set; }
    public bool IsDeleted { get; set; }
}

