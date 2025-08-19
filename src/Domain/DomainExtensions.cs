using Microsoft.Extensions.DependencyInjection;

namespace CertManager.Domain;

public static class DomainExtensions
{
    public static IServiceCollection AddDomain(this IServiceCollection services)
    {
        return services;
    }
}