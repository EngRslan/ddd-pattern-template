namespace Engrslan.Domain.Services;

public interface IDateTimeService
{
    DateTime Now { get; }
    DateTime UtcNow { get; }
    DateOnly Today { get; }
    TimeOnly TimeNow { get; }
}