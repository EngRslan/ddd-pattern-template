namespace Engrslan.Interfaces;

public interface IEntity 
{
    public int Id { get; set; }
}

public interface IEntity<out T> : IEntity
{
    new T Id { get; }    
}

public interface ICreationAuditedEntity
{
    Guid? CreatedBy { get; set; }
    DateTime CreatedAt { get; set; }   
};
public interface ICreationAuditedEntity<out T> : ICreationAuditedEntity, IEntity<T>
{ }

public interface IAuditedEntity : ICreationAuditedEntity
{
    public Guid? ModifiedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }  
}


public interface IAuditedEntity<out T> : IEntity<T>, IAuditedEntity
{ }

public interface IFullAuditedEntity : IAuditedEntity
{
    public Guid? DeletedBy { get; set; }
    public DateTime? DeletedAt { get; set; }   
    public bool IsDeleted { get; set; }   
};

public interface IFullAuditedEntity<out T> :IEntity<T>, IFullAuditedEntity
{ }


public interface IHasConcurrencyStamp
{
    public string ConcurrencyStamp { get; set; }
}