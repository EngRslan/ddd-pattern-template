using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Engrslan.Domain.Shared.Events;

public class EventDispatcher : IEventDispatcher
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EventDispatcher> _logger;

    public EventDispatcher(IServiceProvider serviceProvider, ILogger<EventDispatcher> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task DispatchAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        if (domainEvent == null)
        {
            throw new ArgumentNullException(nameof(domainEvent));
        }

        var eventType = domainEvent.GetType();
        _logger.LogDebug("Dispatching domain event {EventType} with ID {EventId}", eventType.Name, domainEvent.EventId);

        var handlerType = typeof(IEventHandler<>).MakeGenericType(eventType);
        var handlers = _serviceProvider.GetServices(handlerType);

        var handleTasks = handlers.Select(handler => InvokeHandlerAsync(handler!, domainEvent, eventType, cancellationToken));

        await Task.WhenAll(handleTasks);

        _logger.LogDebug("Completed dispatching domain event {EventType} with ID {EventId}", eventType.Name, domainEvent.EventId);
    }

    public async Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default)
    {
        var events = domainEvents?.ToList();
        if (events == null || !events.Any())
        {
            return;
        }

        foreach (var domainEvent in events)
        {
            await DispatchAsync(domainEvent, cancellationToken);
        }
    }

    private async Task InvokeHandlerAsync(object handler, IDomainEvent domainEvent, Type eventType, CancellationToken cancellationToken)
    {
        try
        {
            var handleMethod = handler.GetType().GetMethod("HandleAsync");
            if (handleMethod == null)
            {
                _logger.LogWarning("Handler {HandlerType} does not have HandleAsync method", handler.GetType().Name);
                return;
            }

            var task = (Task)handleMethod.Invoke(handler, [domainEvent, cancellationToken])!;
            await task;

            _logger.LogDebug("Successfully handled event {EventType} with handler {HandlerType}", 
                eventType.Name, handler.GetType().Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling event {EventType} with handler {HandlerType}", 
                eventType.Name, handler.GetType().Name);
            throw;
        }
    }
}