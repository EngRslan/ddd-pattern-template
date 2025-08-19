using CertManager.Domain.Events;
using CertManager.Domain.Shared.Events;
using Microsoft.Extensions.Logging;

namespace CertManager.Application.EventHandlers;

public class EntityCreatedEventHandler : IEventHandler<EntityCreatedEvent>
{
    private readonly ILogger<EntityCreatedEventHandler> _logger;

    public EntityCreatedEventHandler(ILogger<EntityCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task HandleAsync(EntityCreatedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Entity created: Type={EntityType}, Id={EntityId}, CreatedBy={CreatedBy}, EventId={EventId}", 
            domainEvent.EntityType, 
            domainEvent.EntityId, 
            domainEvent.CreatedBy,
            domainEvent.EventId);

        // Add your business logic here
        // For example: send notifications, update read models, trigger workflows, etc.

        return Task.CompletedTask;
    }
}