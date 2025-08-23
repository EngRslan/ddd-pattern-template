using Engrslan.Interfaces;

namespace Engrslan.Entities;

public abstract class Entity : IEntity
{
    public int Id { get; set; }
}

public abstract class Entity<T> : Entity
{
    public new T Id { get; set; } = default!;
}

public abstract class CreationAuditedEntity<T> : Entity<T>, ICreationAuditedEntity<T>
{
    public Guid? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
}

public abstract class CreationAuditedEntity : CreationAuditedEntity<int>;

public abstract class AuditedEntity<T> : CreationAuditedEntity<T>, IAuditedEntity<T>
{
    public Guid? ModifiedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }
}

public abstract class AuditedEntity : AuditedEntity<int>;

public abstract class FullAuditedEntity<T> : AuditedEntity<T>, IFullAuditedEntity<T>
{
    public Guid? DeletedBy { get; set; }
    public DateTime? DeletedAt { get; set; }
    public bool IsDeleted { get; set; }
}

public abstract class FullAuditedEntity : FullAuditedEntity<int>;
