using System.Reflection;
using Engrslan.Domain.Shared.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Engrslan.Infrastructure;

/// <summary>
/// Extension methods for registering Infrastructure services
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Infrastructure services to the dependency injection container
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services)
    {

        services.AddServicesFromAssembly(Assembly.GetExecutingAssembly());
        
        // Add other infrastructure services here as needed
        // Examples:
        // services.AddScoped<IFileStorageService, LocalFileStorageService>();
        // services.AddScoped<ICacheService, MemoryCacheService>();
        // services.AddScoped<IQueueService, InMemoryQueueService>();
        
        return services;
    }
}