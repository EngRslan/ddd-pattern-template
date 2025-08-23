using Engrslan;
using Engrslan.Identity;
using Engrslan.IdentityHandlers;
using Engrslan.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;
using OpenIddict.Server;

var builder = WebApplication.CreateBuilder(args);

// Configure services
ConfigureCors(builder.Services, builder.Configuration);
ConfigureRazorPages(builder.Services, builder.Environment);
ConfigureDatabase(builder.Services, builder.Configuration);
ConfigureIdentity(builder.Services);
ConfigureDomainAndInfrastructure(builder.Services);
ConfigureOpenIddict(builder.Services);
ConfigureHostedServices(builder.Services);

var app = builder.Build();

// Configure middleware pipeline
ConfigureMiddleware(app);

app.Run();
return;

void ConfigureCors(IServiceCollection services, IConfiguration configuration)
{
    const string defaultCorsPolicy = "DefaultPolicy";
    var allowedOrigins = configuration.GetSection("AllowedOrigins").Get<string[]>() ?? ["*"];

    services.AddCors(options =>
    {
        options.AddPolicy(defaultCorsPolicy, policyBuilder =>
        {
            if (allowedOrigins.Contains("*"))
            {
                policyBuilder.AllowAnyOrigin()
                             .AllowAnyMethod()
                             .AllowAnyHeader();
            }
            else
            {
                policyBuilder.WithOrigins(allowedOrigins)
                             .AllowAnyMethod()
                             .AllowAnyHeader()
                             .AllowCredentials();
            }
        });
    });
}

void ConfigureRazorPages(IServiceCollection services, IWebHostEnvironment environment)
{
    if (environment.IsDevelopment())
    {
        services.AddRazorPages().AddRazorRuntimeCompilation();
    }
    else
    {
        services.AddRazorPages();
    }
}

void ConfigureDatabase(IServiceCollection services, IConfiguration configuration)
{
    services.AddDbContext<ApplicationDataContext>(options =>
    {
        options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
        options.UseOpenIddict();
    });
}

void ConfigureIdentity(IServiceCollection services)
{
    services.AddIdentity<User, Role>()
        .AddEntityFrameworkStores<ApplicationDataContext>()
        .AddDefaultTokenProviders();
}

void ConfigureDomainAndInfrastructure(IServiceCollection services)
{
    services.AddDomain();
    services.AddInfrastructureServices();
}

void ConfigureOpenIddict(IServiceCollection services)
{
    services.AddOpenIddict()
        .AddCore(options =>
        {
            options.UseEntityFrameworkCore()
                   .UseDbContext<ApplicationDataContext>();
        })
        .AddServer(options =>
        {
            ConfigureOpenIddictEndpoints(options);
            ConfigureOpenIddictFlows(options);
            ConfigureOpenIddictCertificates(options);
            ConfigureOpenIddictScopes(options);
            ConfigureOpenIddictEventHandlers(options);
            
            options.UseAspNetCore();
        });
}

void ConfigureOpenIddictEndpoints(OpenIddictServerBuilder options)
{
    options.SetTokenEndpointUris("/connect/token")
           .SetAuthorizationEndpointUris("/connect/authorize")
           .SetIntrospectionEndpointUris("/connect/introspect")
           .SetEndSessionEndpointUris("/connect/endsession")
           .SetUserInfoEndpointUris("/connect/userinfo");
}

void ConfigureOpenIddictFlows(OpenIddictServerBuilder options)
{
    options.AllowAuthorizationCodeFlow()
           .AllowRefreshTokenFlow()
           .AllowClientCredentialsFlow()
           .AllowPasswordFlow();
}

void ConfigureOpenIddictCertificates(OpenIddictServerBuilder options)
{
    options.AddDevelopmentEncryptionCertificate()
           .AddDevelopmentSigningCertificate();
}

void ConfigureOpenIddictScopes(OpenIddictServerBuilder options)
{
    options.RegisterScopes(
        OpenIddictConstants.Scopes.Email,
        OpenIddictConstants.Scopes.Profile,
        OpenIddictConstants.Scopes.Roles
    );
}

void ConfigureOpenIddictEventHandlers(OpenIddictServerBuilder options)
{
    options.AddEventHandler<OpenIddictServerEvents.HandleAuthorizationRequestContext>(x => 
        x.UseScopedHandler<AuthorizeRequestHandler>());
    
    options.AddEventHandler<OpenIddictServerEvents.HandleTokenRequestContext>(x => 
        x.UseScopedHandler<TokenRequestHandler>());
    
    options.AddEventHandler<OpenIddictServerEvents.HandleEndSessionRequestContext>(x =>
        x.UseScopedHandler<LogoutRequestHandler>());
    
    options.AddEventHandler<OpenIddictServerEvents.HandleUserInfoRequestContext>(x =>
        x.UseScopedHandler<UserInfoRequestHandler>());
}

void ConfigureHostedServices(IServiceCollection services)
{
    services.AddHostedService<OpenIddictDataSeeder>();
}

void ConfigureMiddleware(WebApplication app)
{
    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseCors("DefaultPolicy");
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapRazorPages();
}