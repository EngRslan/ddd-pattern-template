namespace Engrslan.Dtos;

public abstract class EntityDto<T>
{
    public T Id { get; set; } = default!;
}

public abstract class EntityDto : EntityDto<int>;

public abstract class CreationAuditedEntityDto<T> : EntityDto<T>
{
    public Guid CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
}

public abstract class CreationAuditedEntityDto : CreationAuditedEntityDto<int>;

public abstract class AuditedEntityDto<T> : CreationAuditedEntityDto<T>
{
    public Guid? ModifiedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }
}

public abstract class AuditedEntityDto : AuditedEntityDto<int>;

public abstract class FullAuditedEntityDto<T> : AuditedEntityDto<T>
{
    public Guid? DeletedBy { get; set; }
    public DateTime? DeletedAt { get; set; }
    public bool IsDeleted { get; set; }
}

public abstract class FullAuditedEntityDto : FullAuditedEntityDto<int>;