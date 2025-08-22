using System.Reflection;
using Engrslan.Domain.Shared.DependencyInjection;
using Engrslan.Domain.Shared.Events;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Engrslan.Application;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMemoryCache();
        
        // Auto-register services with marker interfaces
        services.AddServicesFromAssembly(Assembly.GetExecutingAssembly());
        
        // Register validators
        services.AddValidatorsFromAssembly(Assembly.Load("Engrslan.Application.Contracts"));
        
        // Register domain events infrastructure
        services.AddDomainEvents();
        
        // Auto-register all event handlers from this assembly
        services.AddEventHandlersFromAssembly(Assembly.GetExecutingAssembly());
        
        return services;
    }
}