namespace CertManager.Domain.Interfaces;

public interface IEntity 
{
    public int Id { get; set; }
}

public interface IEntity<out T> : IEntity
{
    new T Id { get; }    
}

public interface ICreationEntity<out T> : IEntity<T>
{
    Guid? CreatedBy { get; set; }
    DateTime CreatedAt { get; set; }
}

public interface IAuditableEntity<out T> : ICreationEntity<T>
{
    public Guid? UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }   
}

public interface IFullAuditableEntity<out T> : IAuditableEntity<T>
{
    public Guid? DeletedBy { get; set; }
    public DateTime? DeletedAt { get; set; }   
    public bool IsDeleted { get; set; }
}

public interface IHasConcurrencyStamp
{
    public string ConcurrencyStamp { get; set; }
}