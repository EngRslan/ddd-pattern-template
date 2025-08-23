using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Engrslan.DependencyInjection;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Automatically registers all services implementing marker interfaces
    /// </summary>
    public static IServiceCollection AddServicesFromAssembly(this IServiceCollection services, Assembly assembly)
    {
        RegisterServices(services, assembly, typeof(ITransientService), ServiceLifetime.Transient);
        RegisterServices(services, assembly, typeof(IScopedService), ServiceLifetime.Scoped);
        RegisterServices(services, assembly, typeof(ISingletonService), ServiceLifetime.Singleton);
        
        return services;
    }
    
    /// <summary>
    /// Automatically registers all services from multiple assemblies
    /// </summary>
    public static IServiceCollection AddServicesFromAssemblies(this IServiceCollection services, params Assembly[] assemblies)
    {
        foreach (var assembly in assemblies)
        {
            services.AddServicesFromAssembly(assembly);
        }
        
        return services;
    }

    private static void RegisterServices(IServiceCollection services, Assembly assembly, Type markerInterface, ServiceLifetime lifetime)
    {
        var types = assembly.GetExportedTypes()
            .Where(t => t.IsClass && !t.IsAbstract && markerInterface.IsAssignableFrom(t))
            .ToList();

        foreach (var implementationType in types)
        {
            // Get all interfaces except marker interfaces
            var serviceTypes = implementationType.GetInterfaces()
                .Where(i => !IsMarkerInterface(i))
                .ToList();

            if (serviceTypes.Any())
            {
                // Register with all non-marker interfaces
                foreach (var serviceType in serviceTypes)
                {
                    RegisterService(services, serviceType, implementationType, lifetime);
                }
            }
            else
            {
                // Register as self if no other interfaces
                RegisterService(services, implementationType, implementationType, lifetime);
            }
        }
    }

    private static void RegisterService(IServiceCollection services, Type serviceType, Type implementationType, ServiceLifetime lifetime)
    {
        // Check if already registered to avoid duplicates
        if (services.Any(s => s.ServiceType == serviceType && s.ImplementationType == implementationType))
            return;

        switch (lifetime)
        {
            case ServiceLifetime.Transient:
                services.AddTransient(serviceType, implementationType);
                break;
            case ServiceLifetime.Scoped:
                services.AddScoped(serviceType, implementationType);
                break;
            case ServiceLifetime.Singleton:
                services.AddSingleton(serviceType, implementationType);
                break;
        }
    }

    private static bool IsMarkerInterface(Type type)
    {
        return type == typeof(ITransientService) ||
               type == typeof(IScopedService) ||
               type == typeof(ISingletonService);
    }
}