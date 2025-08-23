using System.Reflection;
using Engrslan.DependencyInjection;
using Engrslan.Interceptors;
using Engrslan.Interfaces;
using Engrslan.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Engrslan;

public static class EfCoreServiceExtensions
{
    public static IServiceCollection AddEfCore(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<AuditableEntitySaveChangesInterceptor>();
        services.AddScoped<DomainEventDispatcherInterceptor>();
        
        services.AddDbContext<ApplicationDataContext>((serviceProvider, options) =>
        {
            var auditInterceptor = serviceProvider.GetRequiredService<AuditableEntitySaveChangesInterceptor>();
            var eventInterceptor = serviceProvider.GetRequiredService<DomainEventDispatcherInterceptor>();
            
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"))
                   .AddInterceptors(auditInterceptor, eventInterceptor);
        });
        
        // Register generic repositories
        services.AddScoped(typeof(IRepository<,>), typeof(Repository<,>));
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        
        // Auto-register services with marker interfaces
        services.AddServicesFromAssembly(Assembly.GetExecutingAssembly());
        
        return services;
    }
}