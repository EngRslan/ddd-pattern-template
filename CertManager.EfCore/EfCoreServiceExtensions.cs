using System.Reflection;
using CertManager.Domain.Interfaces;
using CertManager.Domain.Shared.DependencyInjection;
using CertManager.EfCore.Interceptors;
using CertManager.EfCore.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CertManager.EfCore;

public static class EfCoreServiceExtensions
{
    public static IServiceCollection AddEfCore(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<AuditableEntitySaveChangesInterceptor>();
        
        services.AddDbContext<ApplicationDataContext>((serviceProvider, options) =>
        {
            var interceptor = serviceProvider.GetRequiredService<AuditableEntitySaveChangesInterceptor>();
            
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"))
                   .AddInterceptors(interceptor);
        });
        
        // Register generic repositories
        services.AddScoped(typeof(IRepository<,>), typeof(Repository<,>));
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        
        // Auto-register services with marker interfaces
        services.AddServicesFromAssembly(Assembly.GetExecutingAssembly());
        
        return services;
    }
}