using Engrslan.DependencyInjection;
using Engrslan.Events;
using Microsoft.Extensions.Logging;

namespace Engrslan.Sample.Services;

public interface ISampleServiceWithEvents : ITransientService
{
    Task CreateEntityAsync(string entityType, string createdBy);
}

public class SampleServiceWithEvents : ISampleServiceWithEvents
{
    private readonly IEventDispatcher _eventDispatcher;
    private readonly ILogger<SampleServiceWithEvents> _logger;

    public SampleServiceWithEvents(IEventDispatcher eventDispatcher, ILogger<SampleServiceWithEvents> logger)
    {
        _eventDispatcher = eventDispatcher;
        _logger = logger;
    }

    public async Task CreateEntityAsync(string entityType, string createdBy)
    {
        // Simulate entity creation
        var entityId = Guid.NewGuid();
        _logger.LogInformation("Creating entity: Type={EntityType}, Id={EntityId}", entityType, entityId);

        // Business logic here...
        await Task.Delay(100); // Simulate work

        // Raise domain event
        var domainEvent = new EntityCreatedEvent(entityId, entityType, createdBy);
        await _eventDispatcher.DispatchAsync(domainEvent);

        _logger.LogInformation("Entity created successfully: Type={EntityType}, Id={EntityId}", entityType, entityId);
    }
}