using CertManager.Domain;
using CertManager.Domain.Identity;
using CertManager.EfCore;
using CertManager.Identity.IdentityHandlers;
using CertManager.Identity.Services;
using CertManager.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Server;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddRazorPages().AddRazorRuntimeCompilation();
}
else
{
    builder.Services.AddRazorPages();
}

builder.Services.AddDbContext<IdentityDataContext>(conf =>
{
    conf.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
    conf.UseOpenIddict();
});

builder.Services.AddIdentity<User, Role>()
    .AddEntityFrameworkStores<IdentityDataContext>()
    .AddDefaultTokenProviders();

builder.Services.AddDomain();
builder.Services.AddInfrastructureServices();

builder.Services.AddOpenIddict()
    .AddCore(idCore =>
{
    idCore.UseEntityFrameworkCore().UseDbContext<IdentityDataContext>();
}).AddServer(idServer =>
{
    idServer.SetTokenEndpointUris("/connect/token")
        .SetAuthorizationEndpointUris("/connect/authorize")
        .SetIntrospectionEndpointUris("/connect/introspect")
        .SetEndSessionEndpointUris("/connect/endsession")
        .SetUserInfoEndpointUris("/connect/userinfo")
        .AllowAuthorizationCodeFlow()
        .AllowRefreshTokenFlow()
        .AllowClientCredentialsFlow()
        .AllowPasswordFlow();

    idServer.AddDevelopmentEncryptionCertificate();
    idServer.AddDevelopmentSigningCertificate();
    
    idServer.AddEventHandler<OpenIddictServerEvents.HandleAuthorizationRequestContext>(x => x.UseScopedHandler<AuthorizeRequestHandler>());
    idServer.AddEventHandler<OpenIddictServerEvents.HandleTokenRequestContext>(x => x.UseScopedHandler<TokenRequestHandler>());
    idServer.AddEventHandler<OpenIddictServerEvents.HandleEndSessionRequestContext>(x =>
        x.UseScopedHandler<LogoutRequestHandler>());
    idServer.AddEventHandler<OpenIddictServerEvents.HandleUserInfoRequestContext>(x =>
        x.UseScopedHandler<UserInfoRequestHandler>());

    idServer.UseAspNetCore();

});

// Register the OpenIddict data seeder as a hosted service
builder.Services.AddHostedService<OpenIddictDataSeeder>();

var app = builder.Build();

if(app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();
app.Run();