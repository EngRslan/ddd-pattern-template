using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Engrslan.Events;

public static class EventExtensions
{
    public static IServiceCollection AddDomainEvents(this IServiceCollection services)
    {
        services.TryAddSingleton<IEventDispatcher, EventDispatcher>();
        return services;
    }

    public static IServiceCollection AddEventHandler<TEvent, THandler>(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Scoped)
        where TEvent : IDomainEvent
        where THandler : class, IEventHandler<TEvent>
    {
        services.Add(new ServiceDescriptor(typeof(IEventHandler<TEvent>), typeof(THandler), lifetime));
        return services;
    }

    public static IServiceCollection AddEventHandlersFromAssembly(this IServiceCollection services, Assembly assembly, ServiceLifetime lifetime = ServiceLifetime.Scoped)
    {
        var handlerTypes = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && !t.IsGenericType)
            .SelectMany(t => t.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEventHandler<>))
                .Select(i => new { HandlerType = t, Interface = i }))
            .ToList();

        foreach (var handler in handlerTypes)
        {
            services.Add(new ServiceDescriptor(handler.Interface, handler.HandlerType, lifetime));
        }

        return services;
    }
}