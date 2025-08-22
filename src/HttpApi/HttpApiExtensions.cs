using System.Reflection;
using CertManager.Domain.Shared.DependencyInjection;
using CertManager.HttpApi.Middleware;
using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NSwag;
using OpenIddict.Validation.AspNetCore;
using Scalar.AspNetCore;

namespace CertManager.HttpApi;

public static class HttpApiExtensions
{
    private const string DefaultCorsPolicy = "DefaultPolicy";
    
    public static IServiceCollection AddHttpApi(this IServiceCollection services, IConfiguration configuration)
    {
        // Configure OpenIddict validation for introspection
        services.AddOpenIddict()
            .AddValidation(options =>
            {
                // Configure the issuer (Identity Server URL)
                var identityServerUrl = configuration["OpenIddict:Issuer"] ?? throw new InvalidOperationException("The OpenIddict:Issuer configuration value is missing.");
                options.SetIssuer(identityServerUrl);
                
                // Configure introspection using the credentials from the seeder
                options.UseIntrospection()
                    .SetClientId(configuration["OpenIddict:ClientId"] ?? throw new InvalidOperationException("The OpenIddict:ClientId configuration value is missing."))
                    .SetClientSecret(configuration["OpenIddict:ClientSecret"] ?? throw new InvalidOperationException("The OpenIddict:ClientSecret configuration value is missing."));
                
                // Use System.Net.Http for introspection requests
                options.UseSystemNetHttp();
                
                // Register the ASP.NET Core host
                options.UseAspNetCore();
            });
        
        // Add authentication
        services.AddAuthentication(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);
        services.AddAuthorization();
        
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
                
                // Add OAuth2 security scheme
                s.AddAuth("oauth2", new OpenApiSecurityScheme
                {
                    Type = OpenApiSecuritySchemeType.OAuth2,
                    Description = "OAuth2 Authorization Code Flow",
                    Flows = new OpenApiOAuthFlows
                    {
                        AuthorizationCode = new OpenApiOAuthFlow
                        {
                            AuthorizationUrl = configuration["ScalarOAuth:AuthorizationUrl"],
                            TokenUrl = configuration["ScalarOAuth:TokenUrl"],
                            RefreshUrl = configuration["ScalarOAuth:RefreshUrl"] ?? configuration["ScalarOAuth:TokenUrl"],
                            Scopes = new Dictionary<string, string>
                            {
                                ["openid"] = "OpenID Connect scope",
                                ["profile"] = "User profile information",
                                ["email"] = "User email address",
                                ["roles"] = "User roles",
                                ["certmanager-api"] = "Access to CertManager API"
                            }
                        }
                    }
                });
            };
            
            o.EnableJWTBearerAuth = false; // We're using OAuth2 instead
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
                
                // Add OAuth2 security scheme for v2 as well
                s.AddAuth("oauth2", new OpenApiSecurityScheme
                {
                    Type = OpenApiSecuritySchemeType.OAuth2,
                    Description = "OAuth2 Authorization Code Flow",
                    Flows = new OpenApiOAuthFlows
                    {
                        AuthorizationCode = new OpenApiOAuthFlow
                        {
                            AuthorizationUrl = configuration["ScalarOAuth:AuthorizationUrl"],
                            TokenUrl = configuration["ScalarOAuth:TokenUrl"],
                            RefreshUrl = configuration["ScalarOAuth:RefreshUrl"] ?? configuration["ScalarOAuth:TokenUrl"],
                            Scopes = new Dictionary<string, string>
                            {
                                ["openid"] = "OpenID Connect scope",
                                ["profile"] = "User profile information",
                                ["email"] = "User email address",
                                ["roles"] = "User roles",
                                ["certmanager-api"] = "Access to CertManager API"
                            }
                        }
                    }
                });
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
        
        // Add authentication and authorization middleware
        app.UseAuthentication();
        app.UseAuthorization();
        
        var environment = app.ApplicationServices.GetRequiredService<IWebHostEnvironment>();
        var configuration = app.ApplicationServices.GetRequiredService<IConfiguration>();
        // Get OpenAPI settings from configuration
        var enableSwagger = configuration.GetValue<bool?>("OpenApiSettings:EnableSwagger");
        var enableScalar = configuration.GetValue<bool?>("OpenApiSettings:EnableScalar");
        
        // Determine if Swagger should be enabled
        // If setting is provided, use it; otherwise, enable in Development environment
        var shouldEnableSwagger = enableSwagger ?? environment.IsDevelopment();
        
        // Determine if Scalar should be enabled
        // If setting is provided, use it; otherwise, enable in Development environment
        var shouldEnableScalar = enableScalar ?? environment.IsDevelopment();
        
        app.UseEndpoints(e =>
        {
            e.MapFastEndpoints(conf =>
            {
                conf.Endpoints.RoutePrefix = "api";
            });
            
            if (shouldEnableScalar)
            {
                e.MapScalarApiReference("/docs", r =>
                {
                    r.WithTitle("CertManager API Documentation");
                    r.WithTheme(ScalarTheme.Moon);
                    r.WithOpenApiRoutePattern("swagger/{documentName}/swagger.json");
                    
                    // Configure OAuth2 Authorization Code Flow for Scalar
                    var scalarOAuthConfig = configuration.GetSection("ScalarOAuth");
                    
                    var securitySchemeName = scalarOAuthConfig["SecuritySchemeName"] 
                        ?? throw new InvalidOperationException("The ScalarOAuth:SecuritySchemeName configuration value is missing.");
                    
                    var clientId = scalarOAuthConfig["ClientId"] 
                        ?? throw new InvalidOperationException("The ScalarOAuth:ClientId configuration value is missing.");
                    
                    var authorizationUrl = scalarOAuthConfig["AuthorizationUrl"] 
                        ?? throw new InvalidOperationException("The ScalarOAuth:AuthorizationUrl configuration value is missing.");
                    
                    var tokenUrl = scalarOAuthConfig["TokenUrl"] 
                        ?? throw new InvalidOperationException("The ScalarOAuth:TokenUrl configuration value is missing.");
                    
                    var scopes = scalarOAuthConfig["Scopes"] 
                        ?? throw new InvalidOperationException("The ScalarOAuth:Scopes configuration value is missing.");
                    r.HideModels = true;
                    r.AddPreferredSecuritySchemes(securitySchemeName);
                    r.AddAuthorizationCodeFlow(securitySchemeName, flow =>
                    {
                        flow.ClientId = clientId;
                        flow.AuthorizationUrl = authorizationUrl;
                        flow.TokenUrl = tokenUrl;
                        flow.RefreshUrl = scalarOAuthConfig["RefreshUrl"] ?? tokenUrl; // RefreshUrl is optional, defaults to TokenUrl
                        flow.WithSelectedScopes(scopes.Split(',').Select(s => s.Trim()).ToArray());
                        flow.Pkce = Pkce.Sha256;
                    });
                });
            }
        });
        
        // OpenAPI document generation endpoints
        if (shouldEnableSwagger)
        {
            app.UseSwaggerGen();
        }
        
        return app;
    }
}