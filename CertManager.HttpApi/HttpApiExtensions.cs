using System.Reflection;
using CertManager.Domain.Shared.DependencyInjection;
using CertManager.HttpApi.Middleware;
using FastEndpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CertManager.HttpApi;

public static class HttpApiExtensions
{
    private const string DefaultCorsPolicy = "DefaultPolicy";
    
    public static IServiceCollection AddHttpApi(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddFastEndpoints();
        services.AddHttpContextAccessor();
        
        // Configure CORS
        var allowedOrigins = configuration.GetSection("AllowedOrigins").Get<string[]>() ?? ["*"];
        
        services.AddCors(options =>
        {
            options.AddPolicy(DefaultCorsPolicy, builder =>
            {
                if (allowedOrigins.Contains("*"))
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader();
                }
                else
                {
                    builder.WithOrigins(allowedOrigins)
                           .AllowAnyMethod()
                           .AllowAnyHeader()
                           .AllowCredentials();
                }
            });
        });
        
        // Auto-register services with marker interfaces
        services.AddServicesFromAssembly(Assembly.GetExecutingAssembly());
        
        return services;
    }

    public static IApplicationBuilder UseHttpApi(this IApplicationBuilder app)
    {
        app.UseMiddleware<GlobalExceptionHandlingMiddleware>();
        app.UseCors(DefaultCorsPolicy);
        app.UseFastEndpoints(conf => { conf.Endpoints.RoutePrefix = "/api"; });
        return app;
    }
}