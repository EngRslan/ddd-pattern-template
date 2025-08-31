using System.Reflection;
using Engrslan.DependencyInjection;
using Engrslan.Events;
using Engrslan.Services;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Engrslan;

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
        
        // Add Encryption Service
        services.AddSingleton<IEncryptionService>((c) =>
        {
            var configurations = c.GetRequiredService<IConfiguration>();
            return new EncryptionService(configurations["Encryption:DefaultKey"],configurations["Encryption:DefaultIV"]);
        });
        
        return services;
    }
}