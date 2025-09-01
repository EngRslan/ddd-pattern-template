using System.Reflection;
using Engrslan.Binders;
using Engrslan.DependencyInjection;
using Engrslan.Middleware;
using Engrslan.Services;
using Engrslan.Swagger;
using Engrslan.Types;
using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NSwag;
using NSwag.Generation.AspNetCore;
using OpenIddict.Validation.AspNetCore;
using Scalar.AspNetCore;
//#if UseIdentity
//#endif

namespace Engrslan;

public static class HttpApiExtensions
{
    private const string DefaultCorsPolicy = "DefaultPolicy";
    
    public static IServiceCollection AddHttpApi(this IServiceCollection services, IConfiguration configuration)
    {
//#if(UseIdentity)
        ConfigureOpenIddictValidation(services, configuration);
        ConfigureAuthentication(services);
//#endif
        ConfigureFastEndpoints(services);
        ConfigureSwaggerDocuments(services, configuration);
        ConfigureCors(services, configuration);
        RegisterServices(services);
        return services;
    }

    public static IApplicationBuilder UseHttpApi(this IApplicationBuilder app)
    {
        ConfigureMiddlewarePipeline(app);
        ConfigureEndpoints(app);
        ConfigureOpenApiGeneration(app);
        
        return app;
    }
//#if UseIdentity
    private static void ConfigureOpenIddictValidation(IServiceCollection services, IConfiguration configuration)
    {
        services.AddOpenIddict()
            .AddValidation(options =>
            {
                // Configure the issuer (Identity Server URL)
                var identityServerUrl = configuration["OpenIddict:Issuer"] 
                    ?? throw new InvalidOperationException("The OpenIddict:Issuer configuration value is missing.");
                options.SetIssuer(identityServerUrl);
                
                // Configure introspection using the credentials from the seeder
                var clientId = configuration["OpenIddict:ClientId"] 
                    ?? throw new InvalidOperationException("The OpenIddict:ClientId configuration value is missing.");
                var clientSecret = configuration["OpenIddict:ClientSecret"] 
                    ?? throw new InvalidOperationException("The OpenIddict:ClientSecret configuration value is missing.");
                    
                options.UseIntrospection()
                    .SetClientId(clientId)
                    .SetClientSecret(clientSecret);
                
                // Use System.Net.Http for introspection requests
                options.UseSystemNetHttp();
                
                // Register the ASP.NET Core host
                options.UseAspNetCore();
            });
    }

    private static void ConfigureAuthentication(IServiceCollection services)
    {
        services.AddAuthentication(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);
        services.AddAuthorization();
    }
//#endif
    private static void ConfigureFastEndpoints(IServiceCollection services)
    {
        
        services.AddFastEndpoints();
        services.AddHttpContextAccessor();
    }

    private static void ConfigureSwaggerDocuments(IServiceCollection services, IConfiguration configuration)
    {
        // Configure v1 API documentation
        ConfigureSwaggerDocumentV1(services, configuration);
        
        // Configure v2 API documentation
        ConfigureSwaggerDocumentV2(services, configuration);
    }

    private static void ConfigureSwaggerDocumentV1(IServiceCollection services, IConfiguration configuration)
    {
        services.SwaggerDocument(o =>
        {
            o.DocumentSettings = s =>
            {
                s.Title = "Engrslan API";
                s.Version = "v1";
                s.Description = "Domain-Driven Design Template API Documentation";
                s.DocumentName = "v1";
                s.SchemaSettings.SchemaProcessors.Add(new DefaultSchemaProcessor());
                //#if(UseIdentity)
                AddOAuth2SecurityScheme(s, configuration);
                //#endif
            };
            
            o.EnableJWTBearerAuth = false; // We're using OAuth2 instead
            o.TagDescriptions = t =>
            {
                t["Home"] = "Server information endpoints";
            };
            
            o.AutoTagPathSegmentIndex = 0; // Auto-tag by first path segment after prefix
        });
    }

    private static void ConfigureSwaggerDocumentV2(IServiceCollection services, IConfiguration configuration)
    {
        services.SwaggerDocument(o =>
        {
            o.DocumentSettings = s =>
            {
                s.Title = "Engrslan API";
                s.Version = "v2";
                s.DocumentName = "v2";
                //#if(UseIdentity)
                AddOAuth2SecurityScheme(s, configuration);
                //#endif
            };
            o.MaxEndpointVersion = 2;
            o.AutoTagPathSegmentIndex = 0;
        });
    }
    //#if(UseIdentity)
    private static void AddOAuth2SecurityScheme(AspNetCoreOpenApiDocumentGeneratorSettings settings, IConfiguration configuration)
    {
        settings.AddAuth("oauth2", new OpenApiSecurityScheme
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
                        ["certmanager-api"] = "Access to Engrslan API"
                    }
                }
            }
        });
    }
    //#endif
    private static void ConfigureCors(IServiceCollection services, IConfiguration configuration)
    {
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
    }

    private static void RegisterServices(IServiceCollection services)
    {
        // Auto-register services with marker interfaces
        services.AddServicesFromAssembly(Assembly.GetExecutingAssembly());
        services.AddExceptionHandler<GlobalExceptionHandler>();
        
        // Register cleanup files background service
        services.AddHostedService<CleanFilesBackgroundService>();
    }

    private static void ConfigureMiddlewarePipeline(IApplicationBuilder app)
    {
        app.UseCors(DefaultCorsPolicy);
        app.UseRouting();
        
        //#if(UseIdentity)
        // Add authentication and authorization middleware
        app.UseAuthentication();
        app.UseAuthorization();
        //#endif
    }

    private static void ConfigureEndpoints(IApplicationBuilder app)
    {
        var environment = app.ApplicationServices.GetRequiredService<IWebHostEnvironment>();
        var configuration = app.ApplicationServices.GetRequiredService<IConfiguration>();
        
        app.UseEndpoints(e =>
        {
            ConfigureFastEndpointsMapping(e);
            ConfigureScalarApiReference(e, environment, configuration);
        });
    }

    private static void ConfigureFastEndpointsMapping(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapFastEndpoints(conf =>
        {
            conf.Endpoints.RoutePrefix = "api";
            var encryptionService = endpoints.ServiceProvider.GetRequiredService<IEncryptionService>();
            conf.Binding.ValueParserFor<EncryptedInt>(Parser.ParseEncryptedInt(encryptionService));
            conf.Binding.ValueParserFor<EncryptedLong>(Parser.ParseEncryptedLong(encryptionService));
            conf.Serializer.Options.Converters.Add(new EncryptedIntJsonConverter(encryptionService));
            conf.Serializer.Options.Converters.Add(new EncryptedLongJsonConverter(encryptionService));
        });
    }

    private static void ConfigureScalarApiReference(IEndpointRouteBuilder endpoints, IWebHostEnvironment environment, IConfiguration configuration)
    {
        var shouldEnableScalar = ShouldEnableScalar(configuration, environment);
        
        if (!shouldEnableScalar) return;
        
        endpoints.MapScalarApiReference("/docs", r =>
        {
            ConfigureScalarBasicSettings(r);
            //#if(UseIdentity)
            ConfigureScalarOAuth2Flow(r, configuration);
            //#endif
        });
    }

    private static void ConfigureScalarBasicSettings(ScalarOptions options)
    {
        options.WithTitle("Engrslan API Documentation");
        options.WithTheme(ScalarTheme.Moon);
        options.WithOpenApiRoutePattern("swagger/{documentName}/swagger.json");
        options.HideModels = true;
    }
//#if(UseIdentity)
    private static void ConfigureScalarOAuth2Flow(ScalarOptions options, IConfiguration configuration)
    {
        var scalarOAuthConfig = configuration.GetSection("ScalarOAuth");
        
        var securitySchemeName = GetRequiredConfigValue(scalarOAuthConfig, "SecuritySchemeName");
        var clientId = GetRequiredConfigValue(scalarOAuthConfig, "ClientId");
        var authorizationUrl = GetRequiredConfigValue(scalarOAuthConfig, "AuthorizationUrl");
        var tokenUrl = GetRequiredConfigValue(scalarOAuthConfig, "TokenUrl");
        var scopes = GetRequiredConfigValue(scalarOAuthConfig, "Scopes");
        
        options.AddPreferredSecuritySchemes(securitySchemeName);
        options.AddAuthorizationCodeFlow(securitySchemeName, flow =>
        {
            flow.ClientId = clientId;
            flow.AuthorizationUrl = authorizationUrl;
            flow.TokenUrl = tokenUrl;
            flow.RefreshUrl = scalarOAuthConfig["RefreshUrl"] ?? tokenUrl; // RefreshUrl is optional
            flow.WithSelectedScopes(ParseScopes(scopes));
            flow.Pkce = Pkce.Sha256;
        });
    }
//#endif
    private static string GetRequiredConfigValue(IConfigurationSection section, string key)
    {
        return section[key] 
            ?? throw new InvalidOperationException($"The ScalarOAuth:{key} configuration value is missing.");
    }

    private static string[] ParseScopes(string scopes)
    {
        return scopes.Split(',').Select(s => s.Trim()).ToArray();
    }

    private static void ConfigureOpenApiGeneration(IApplicationBuilder app)
    {
        var environment = app.ApplicationServices.GetRequiredService<IWebHostEnvironment>();
        var configuration = app.ApplicationServices.GetRequiredService<IConfiguration>();
        
        var shouldEnableSwagger = ShouldEnableSwagger(configuration, environment);
        
        if (shouldEnableSwagger)
        {
            app.UseSwaggerGen();
        }
    }

    private static bool ShouldEnableSwagger(IConfiguration configuration, IWebHostEnvironment environment)
    {
        var enableSwagger = configuration.GetValue<bool?>("OpenApiSettings:EnableSwagger");
        return enableSwagger ?? environment.IsDevelopment();
    }

    private static bool ShouldEnableScalar(IConfiguration configuration, IWebHostEnvironment environment)
    {
        var enableScalar = configuration.GetValue<bool?>("OpenApiSettings:EnableScalar");
        return enableScalar ?? environment.IsDevelopment();
    }
}