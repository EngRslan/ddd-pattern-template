using Engrslan.Domain.Interfaces;

namespace Engrslan.Domain.Entities;

public abstract class Entity : IEntity
{
    public int Id { get; set; }
}

public abstract class Entity<T> : Entity
{
    public new T Id { get; set; } = default!;
}

public abstract class CreationEntity<T> : Entity<T>, ICreationEntity<T>
{
    public Guid? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
}

public abstract class CreationEntity : CreationEntity<int>;

public abstract class AuditableEntity<T> : CreationEntity<T>, IAuditableEntity<T>
{
    public Guid? UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public abstract class AuditableEntity : AuditableEntity<int>;

public abstract class FullAuditableEntity<T> : AuditableEntity<T>, IFullAuditableEntity<T>
{
    public Guid? DeletedBy { get; set; }
    public DateTime? DeletedAt { get; set; }
    public bool IsDeleted { get; set; }
}

public abstract class FullAuditableEntity : FullAuditableEntity<int>;
