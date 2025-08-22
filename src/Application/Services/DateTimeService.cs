using Engrslan.Domain.Services;
using Engrslan.Domain.Shared.DependencyInjection;

namespace Engrslan.Application.Services;

public class DateTimeService : IDateTimeService, ISingletonService
{
    public DateTime Now => DateTime.Now;
    public DateTime UtcNow => DateTime.UtcNow;
    public DateOnly Today => DateOnly.FromDateTime(DateTime.Now);
    public TimeOnly TimeNow => TimeOnly.FromDateTime(DateTime.Now);
}