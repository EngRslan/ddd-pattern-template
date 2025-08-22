using System.Reflection;
using Engrslan.Domain.Interfaces;
using Engrslan.Domain.Shared.DependencyInjection;
using Engrslan.EfCore.Interceptors;
using Engrslan.EfCore.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Engrslan.EfCore;

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