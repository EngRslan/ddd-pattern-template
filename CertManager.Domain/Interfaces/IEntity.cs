namespace CertManager.Domain.Interfaces;

public interface IEntity<out T>
{
    T Id { get; }    
}

public interface IEntity : IEntity<int>
{
}

public interface ICreationEntity<out T> : IEntity<T>
{
    public Guid? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
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