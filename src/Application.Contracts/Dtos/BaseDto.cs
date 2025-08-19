namespace CertManager.Application.Contracts.Dtos;

public abstract class BaseDto
{
    public int Id { get; set; }
}

public abstract class AuditableDto : BaseDto
{
    public Guid? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public abstract class FullAuditableDto : AuditableDto
{
    public bool IsDeleted { get; set; }
    public Guid? DeletedBy { get; set; }
    public DateTime? DeletedAt { get; set; }
}