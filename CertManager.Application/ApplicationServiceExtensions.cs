using System.Reflection;
using CertManager.Domain.Shared.DependencyInjection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace CertManager.Application;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMemoryCache();
        
        // Auto-register services with marker interfaces
        services.AddServicesFromAssembly(Assembly.GetExecutingAssembly());
        
        // Register validators
        services.AddValidatorsFromAssembly(Assembly.Load("CertManager.Application.Contracts"));
        
        return services;
    }
}