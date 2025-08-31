namespace Engrslan.Dtos;

public interface IEntityDto
{
    
}

public interface IEntityDto<out T> : IEntityDto
{
    T Id { get; }
}

public abstract class EntityDto : IEntityDto
{ };

public abstract class EntityDto<T> : EntityDto, IEntityDto<T>
{
    public T Id { get; set; } = default!;
}


public abstract class CreationAuditedEntityDto : EntityDto
{
    public Guid? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
}

public abstract class CreationAuditedEntityDto<T> : CreationAuditedEntityDto, IEntityDto<T>
{
    public T Id { get; set;}= default!;
}

public abstract class AuditedEntityDto : CreationAuditedEntityDto
{
    public Guid? ModifiedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }
}

public abstract class AuditedEntityDto<T> : AuditedEntityDto , IEntityDto<T>
{
    public T Id { get; set;}= default!;
};


public abstract class FullAuditedEntityDto : AuditedEntityDto
{
    public Guid? DeletedBy { get; set; }
    public DateTime? DeletedAt { get; set; }
    public bool IsDeleted { get; set; }
}

public abstract class FullAuditedEntityDto<T> : FullAuditedEntityDto , IEntityDto<T>
{
    public T Id { get; set; }= default!;
}