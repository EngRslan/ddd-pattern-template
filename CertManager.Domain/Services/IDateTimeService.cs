namespace CertManager.Domain.Services;

public interface IDateTimeService
{
    DateTime Now { get; }
    DateTime UtcNow { get; }
    DateOnly Today { get; }
    TimeOnly TimeNow { get; }
}