using System.Reflection;
using Engrslan.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Engrslan;

public static class DomainExtensions
{
    public static IServiceCollection AddDomain(this IServiceCollection services)
    {
        services.AddServicesFromAssembly(Assembly.GetExecutingAssembly());
        return services;
    }
}