using System.Reflection;
using CertManager.Domain.Shared.DependencyInjection;
using CertManager.HttpApi.Middleware;
using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Scalar.AspNetCore;

namespace CertManager.HttpApi;

public static class HttpApiExtensions
{
    private const string DefaultCorsPolicy = "DefaultPolicy";
    
    public static IServiceCollection AddHttpApi(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddFastEndpoints();
        services.AddHttpContextAccessor();
        
        // Configure OpenAPI/Swagger document generation
        services.SwaggerDocument(o =>
        {
            o.DocumentSettings = s =>
            {
                s.Title = "CertManager API";
                s.Version = "v1";
                s.Description = "Domain-Driven Design Template API Documentation";
                s.DocumentName = "v1";
            };
            
            o.EnableJWTBearerAuth = false; // Set to true if using JWT
            o.TagDescriptions = t =>
            {
                t["Home"] = "Server information endpoints";
            };
            
            o.AutoTagPathSegmentIndex = 0; // Auto-tag by first path segment after prefix
        });
        
        // Add additional API versions if needed
        services.SwaggerDocument(o =>
        {
            o.DocumentSettings = s =>
            {
                s.Title = "CertManager API";
                s.Version = "v2";
                s.DocumentName = "v2";
            };
            o.MaxEndpointVersion = 2;
            o.AutoTagPathSegmentIndex = 0;
        });
        
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
        app.UseRouting();
        app.UseEndpoints(e =>
        {
            e.MapFastEndpoints(conf =>
            {
                conf.Endpoints.RoutePrefix = "api";
            });
            e.MapScalarApiReference("/docs", r =>
            {
                r.WithTitle("CertManager API Documentation");
                r.WithTheme(ScalarTheme.Moon);
                r.WithOpenApiRoutePattern("swagger/{documentName}/swagger.json");
            });
        });
        
        // OpenAPI document generation endpoints
        app.UseSwaggerGen(); 
        return app;
    }
}