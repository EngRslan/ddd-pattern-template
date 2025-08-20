using System.Collections.Immutable;
using System.Security.Claims;
using CertManager.Domain.Identity;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server;
using OpenIddict.Server.AspNetCore;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace CertManager.Identity.IdentityHandlers;

public class TokenRequestHandler : IOpenIddictServerHandler<OpenIddictServerEvents.HandleTokenRequestContext>
{
    private readonly IOpenIddictApplicationManager _applicationManager;
    private readonly IOpenIddictAuthorizationManager _authorizationManager;
    private readonly IOpenIddictScopeManager _scopeManager;
    private readonly SignInManager<User> _signInManager;
    private readonly UserManager<User> _userManager;

    public TokenRequestHandler(
        IOpenIddictApplicationManager applicationManager,
        IOpenIddictAuthorizationManager authorizationManager,
        IOpenIddictScopeManager scopeManager,
        SignInManager<User> signInManager,
        UserManager<User> userManager)
    {
        _applicationManager = applicationManager;
        _authorizationManager = authorizationManager;
        _scopeManager = scopeManager;
        _signInManager = signInManager;
        _userManager = userManager;
    }

    public async ValueTask HandleAsync(OpenIddictServerEvents.HandleTokenRequestContext context)
    {
        var request = context.Request;
        
        // Handle the different grant types
        if (request.IsAuthorizationCodeGrantType())
        {
            await HandleAuthorizationCodeFlow(context);
        }
        else if (request.IsRefreshTokenGrantType())
        {
            await HandleRefreshTokenFlow(context);
        }
        else if (request.IsPasswordGrantType())
        {
            await HandlePasswordFlow(context);
        }
        else if (request.IsClientCredentialsGrantType())
        {
            await HandleClientCredentialsFlow(context);
        }
        else
        {
            context.Reject(
                error: Errors.UnsupportedGrantType,
                description: "The specified grant type is not supported.");
        }
    }

    private async Task HandleAuthorizationCodeFlow(OpenIddictServerEvents.HandleTokenRequestContext context)
    {
        var request = context.Transaction.GetHttpRequest() ?? 
            throw new InvalidOperationException("The HTTP request cannot be retrieved.");
        
        // Retrieve the claims principal stored in the authorization code
        var principal = (await request.HttpContext.AuthenticateAsync(
            OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)).Principal;

        if (principal == null)
        {
            context.Reject(
                error: Errors.InvalidGrant,
                description: "The authorization code is no longer valid.");
            return;
        }

        // Retrieve the user profile corresponding to the authorization code
        var user = await _userManager.FindByIdAsync(principal.GetClaim(Claims.Subject) ?? string.Empty);
        if (user == null)
        {
            context.Reject(
                error: Errors.InvalidGrant,
                description: "The authorization code is no longer valid.");
            return;
        }

        // Ensure the user is still allowed to sign in
        if (!await _signInManager.CanSignInAsync(user))
        {
            context.Reject(
                error: Errors.InvalidGrant,
                description: "The user is no longer allowed to sign in.");
            return;
        }

        // Create a new identity with the necessary claims and scopes
        var identity = new ClaimsIdentity(principal.Claims,
            authenticationType: TokenValidationParameters.DefaultAuthenticationType,
            nameType: Claims.Name,
            roleType: Claims.Role);

        // Override the user claims with fresh data from the database
        identity.SetClaim(Claims.Subject, await _userManager.GetUserIdAsync(user))
                .SetClaim(Claims.Email, await _userManager.GetEmailAsync(user))
                .SetClaim(Claims.Name, await _userManager.GetUserNameAsync(user))
                .SetClaim(Claims.PreferredUsername, await _userManager.GetUserNameAsync(user));

        // Add user roles
        var roles = await _userManager.GetRolesAsync(user);
        identity.SetClaims(Claims.Role, roles.ToImmutableArray());

        identity.SetDestinations(GetDestinations);
        
        // Return the signing in result with the new principal
        context.SignIn(new ClaimsPrincipal(identity));
    }

    private async Task HandleRefreshTokenFlow(OpenIddictServerEvents.HandleTokenRequestContext context)
    {
        var request = context.Transaction.GetHttpRequest() ?? 
            throw new InvalidOperationException("The HTTP request cannot be retrieved.");
        
        // Retrieve the claims principal stored in the refresh token
        var principal = (await request.HttpContext.AuthenticateAsync(
            OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)).Principal;

        if (principal == null)
        {
            context.Reject(
                error: Errors.InvalidGrant,
                description: "The refresh token is no longer valid.");
            return;
        }

        // Retrieve the user profile corresponding to the refresh token
        var user = await _userManager.FindByIdAsync(principal.GetClaim(Claims.Subject) ?? string.Empty);
        if (user == null)
        {
            context.Reject(
                error: Errors.InvalidGrant,
                description: "The refresh token is no longer valid.");
            return;
        }

        // Ensure the user is still allowed to sign in
        if (!await _signInManager.CanSignInAsync(user))
        {
            context.Reject(
                error: Errors.InvalidGrant,
                description: "The user is no longer allowed to sign in.");
            return;
        }

        // Create a new identity with updated claims
        var identity = new ClaimsIdentity(principal.Claims,
            authenticationType: TokenValidationParameters.DefaultAuthenticationType,
            nameType: Claims.Name,
            roleType: Claims.Role);

        // Override the user claims with fresh data from the database
        identity.SetClaim(Claims.Subject, await _userManager.GetUserIdAsync(user))
                .SetClaim(Claims.Email, await _userManager.GetEmailAsync(user))
                .SetClaim(Claims.Name, await _userManager.GetUserNameAsync(user))
                .SetClaim(Claims.PreferredUsername, await _userManager.GetUserNameAsync(user));

        // Add updated user roles
        var roles = await _userManager.GetRolesAsync(user);
        identity.SetClaims(Claims.Role, roles.ToImmutableArray());

        identity.SetDestinations(GetDestinations);

        context.SignIn(new ClaimsPrincipal(identity));
    }

    private async Task HandlePasswordFlow(OpenIddictServerEvents.HandleTokenRequestContext context)
    {
        var request = context.Request;
        
        // Validate the username and password
        var user = await _userManager.FindByNameAsync(request.Username ?? string.Empty);
        if (user == null)
        {
            context.Reject(
                error: Errors.InvalidGrant,
                description: "The username/password couple is invalid.");
            return;
        }

        // Validate the password
        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password ?? string.Empty, lockoutOnFailure: true);
        if (!result.Succeeded)
        {
            if (result.IsLockedOut)
            {
                context.Reject(
                    error: Errors.InvalidGrant,
                    description: "The account is locked out.");
            }
            else if (result.IsNotAllowed)
            {
                context.Reject(
                    error: Errors.InvalidGrant,
                    description: "The user is not allowed to sign in.");
            }
            else
            {
                context.Reject(
                    error: Errors.InvalidGrant,
                    description: "The username/password couple is invalid.");
            }
            return;
        }

        // Create the claims-based identity
        var identity = new ClaimsIdentity(
            authenticationType: TokenValidationParameters.DefaultAuthenticationType,
            nameType: Claims.Name,
            roleType: Claims.Role);

        // Add the claims that will be persisted in the tokens
        identity.SetClaim(Claims.Subject, await _userManager.GetUserIdAsync(user))
                .SetClaim(Claims.Email, await _userManager.GetEmailAsync(user))
                .SetClaim(Claims.Name, await _userManager.GetUserNameAsync(user))
                .SetClaim(Claims.PreferredUsername, await _userManager.GetUserNameAsync(user));

        // Add user roles
        var roles = await _userManager.GetRolesAsync(user);
        identity.SetClaims(Claims.Role, roles.ToImmutableArray());

        // Set the scopes
        identity.SetScopes(request.GetScopes());
        
        // Set resources
        var resourcesEnumerable = _scopeManager.ListResourcesAsync(identity.GetScopes());
        var resources = new List<string>();
        await foreach (var resource in resourcesEnumerable)
        {
            resources.Add(resource);
        }
        identity.SetResources(resources.ToImmutableArray());

        // Create authorization if client is provided
        if (!string.IsNullOrEmpty(request.ClientId))
        {
            var application = await _applicationManager.FindByClientIdAsync(request.ClientId);
            if (application != null)
            {
                var authorization = await _authorizationManager.CreateAsync(
                    identity: identity,
                    subject: await _userManager.GetUserIdAsync(user),
                    client: await _applicationManager.GetIdAsync(application) ?? throw new InvalidOperationException("Application ID cannot be null"),
                    type: AuthorizationTypes.AdHoc,
                    scopes: identity.GetScopes());
                    
                identity.SetAuthorizationId(await _authorizationManager.GetIdAsync(authorization));
            }
        }

        identity.SetDestinations(GetDestinations);

        context.SignIn(new ClaimsPrincipal(identity));
    }

    private async Task HandleClientCredentialsFlow(OpenIddictServerEvents.HandleTokenRequestContext context)
    {
        var request = context.Request;
        
        // Note: the client credentials are automatically validated by OpenIddict
        // Client authentication is enforced by OpenIddict before this handler is invoked
        
        if (string.IsNullOrEmpty(request.ClientId))
        {
            context.Reject(
                error: Errors.InvalidClient,
                description: "The client identifier is missing.");
            return;
        }

        var application = await _applicationManager.FindByClientIdAsync(request.ClientId);
        if (application == null)
        {
            context.Reject(
                error: Errors.InvalidClient,
                description: "The client application was not found.");
            return;
        }

        // Create a new identity for the client application
        var identity = new ClaimsIdentity(
            authenticationType: TokenValidationParameters.DefaultAuthenticationType,
            nameType: Claims.Name,
            roleType: Claims.Role);

        // Add the subject claim (using the client_id as the subject for client credentials flow)
        identity.SetClaim(Claims.Subject, request.ClientId)
                .SetClaim(Claims.Name, await _applicationManager.GetDisplayNameAsync(application) ?? request.ClientId);

        // Set the scopes granted to the client
        identity.SetScopes(request.GetScopes());
        
        // Set resources
        var resourcesEnumerable = _scopeManager.ListResourcesAsync(identity.GetScopes());
        var resources = new List<string>();
        await foreach (var resource in resourcesEnumerable)
        {
            resources.Add(resource);
        }
        identity.SetResources(resources.ToImmutableArray());

        identity.SetDestinations(GetDestinations);

        context.SignIn(new ClaimsPrincipal(identity));
    }

    private static IEnumerable<string> GetDestinations(Claim claim)
    {
        // Note: by default, claims are NOT automatically included in the access and identity tokens.
        // To allow OpenIddict to serialize them, you must attach them a destination, that specifies
        // whether they should be included in access tokens, in identity tokens or in both.

        switch (claim.Type)
        {
            case Claims.Name:
            case Claims.PreferredUsername:
                yield return Destinations.AccessToken;

                if (claim.Subject?.HasScope(Scopes.Profile) ?? false)
                    yield return Destinations.IdentityToken;

                yield break;

            case Claims.Email:
                yield return Destinations.AccessToken;

                if (claim.Subject?.HasScope(Scopes.Email) ?? false)
                    yield return Destinations.IdentityToken;

                yield break;

            case Claims.Role:
                yield return Destinations.AccessToken;

                if (claim.Subject?.HasScope(Scopes.Roles) ?? false)
                    yield return Destinations.IdentityToken;

                yield break;

            // Never include the security stamp in the access and identity tokens, as it's a secret value.
            case "AspNet.Identity.SecurityStamp": 
                yield break;

            default:
                yield return Destinations.AccessToken;
                yield break;
        }
    }
}
