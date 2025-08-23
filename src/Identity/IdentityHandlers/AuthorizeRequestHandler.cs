using System.Security.Claims;
using Engrslan.Identity;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Engrslan.IdentityHandlers;

public class AuthorizeRequestHandler : IOpenIddictServerHandler<OpenIddictServerEvents.HandleAuthorizationRequestContext>
{
    private readonly IOpenIddictApplicationManager _applicationManager;
    private readonly IOpenIddictAuthorizationManager _authorizationManager;
    private readonly IOpenIddictScopeManager _scopeManager;
    private readonly UserManager<User> _userManager;

    public AuthorizeRequestHandler(
        IOpenIddictApplicationManager applicationManager,
        IOpenIddictAuthorizationManager authorizationManager,
        IOpenIddictScopeManager scopeManager,
        UserManager<User> userManager)
    {
        _applicationManager = applicationManager;
        _authorizationManager = authorizationManager;
        _scopeManager = scopeManager;
        _userManager = userManager;
    }

    public async ValueTask HandleAsync(OpenIddictServerEvents.HandleAuthorizationRequestContext context)
    {
        var request = context.Transaction.GetHttpRequest() ?? 
            throw new InvalidOperationException("The HTTP request cannot be retrieved.");

        // Retrieve the user principal stored in the authentication cookie.
        var result = await request.HttpContext.AuthenticateAsync(IdentityConstants.ApplicationScheme);

        // If the user is not authenticated, challenge them to log in
        if (!result.Succeeded || result.Principal == null)
        {
            // If the client application requested promptless authentication,
            // return an error indicating that the user is not logged in.
            if (context.Request.HasPromptValue(PromptValues.None))
            {
                context.Reject(
                    error: OpenIddictConstants.Errors.LoginRequired,
                    description: "The user is not logged in.");
                return;
            }

            // To avoid endless login -> authorization redirects, directly return an error
            // if the user can't be extracted from the authorization request.
            if (string.IsNullOrEmpty(context.Request.LoginHint))
            {
                await request.HttpContext.ChallengeAsync(
                    IdentityConstants.ApplicationScheme,
                    new AuthenticationProperties
                    {
                        RedirectUri = request.HttpContext.Request.PathBase + request.HttpContext.Request.Path + 
                                    QueryString.Create(request.HttpContext.Request.Query)
                    });
                context.HandleRequest();
                return;
            }

            context.Reject(
                error: OpenIddictConstants.Errors.LoginRequired,
                description: "The user is not logged in.");
            return;
        }

        // Retrieve the profile of the logged in user.
        var user = await _userManager.GetUserAsync(result.Principal) ??
            throw new InvalidOperationException("The user details cannot be retrieved.");

        // Retrieve the application details from the database.
        var application = await _applicationManager.FindByClientIdAsync(context.Request.ClientId ?? string.Empty) ??
            throw new InvalidOperationException("Details concerning the calling client application cannot be found.");

        // Retrieve the permanent authorizations associated with the user and the calling client application.
        var authorizationsEnumerable = _authorizationManager.FindAsync(
            subject: await _userManager.GetUserIdAsync(user),
            client: await _applicationManager.GetIdAsync(application),
            status: Statuses.Valid,
            type: AuthorizationTypes.Permanent,
            scopes: context.Request.GetScopes());
        
        var authorizations = new List<object>();
        await foreach (var authorization in authorizationsEnumerable)
        {
            authorizations.Add(authorization);
        }

        switch (await _applicationManager.GetConsentTypeAsync(application))
        {
            // If the consent is external (e.g, when authorizations are granted by a sysadmin),
            // immediately return an error if no authorization can be found in the database.
            case ConsentTypes.External when !authorizations.Any():
                context.Reject(
                    error: OpenIddictConstants.Errors.ConsentRequired,
                    description: "The logged in user is not allowed to access this client application.");
                return;

            // If the consent is implicit or if an authorization was found,
            // return an authorization response without displaying the consent form.
            case ConsentTypes.Implicit:
            case ConsentTypes.External when authorizations.Count != 0:
            case ConsentTypes.Explicit when authorizations.Count != 0 && !context.Request.HasPromptValue(PromptValues.Consent):
                // Create the claims-based identity that will be used by OpenIddict to generate tokens.
                var identity = new ClaimsIdentity(
                    authenticationType: TokenValidationParameters.DefaultAuthenticationType,
                    nameType: Claims.Name,
                    roleType: Claims.Role);

                // Add the claims that will be persisted in the tokens.
                identity.SetClaim(Claims.Subject, await _userManager.GetUserIdAsync(user))
                        .SetClaim(Claims.Email, await _userManager.GetEmailAsync(user))
                        .SetClaim(Claims.Name, await _userManager.GetUserNameAsync(user))
                        .SetClaim(Claims.PreferredUsername, await _userManager.GetUserNameAsync(user));

                // Add the user roles as claims
                var roles = await _userManager.GetRolesAsync(user);
                identity.SetClaims(Claims.Role, [..roles]);

                // Note: in this sample, the granted scopes match the requested scope
                // but you may want to allow the user to uncheck specific scopes.
                // For that, simply restrict the list of scopes before calling SetScopes.
                identity.SetScopes(context.Request.GetScopes());
                
                var resourcesEnumerable = _scopeManager.ListResourcesAsync(identity.GetScopes());
                var resources = new List<string>();
                await foreach (var resource in resourcesEnumerable)
                {
                    resources.Add(resource);
                }
                identity.SetResources(resources);

                // Automatically create a permanent authorization to avoid requiring explicit consent
                // for future authorization or token requests containing the same scopes.
                var authorization = authorizations.LastOrDefault();
                authorization ??= await _authorizationManager.CreateAsync(
                    identity: identity,
                    subject: await _userManager.GetUserIdAsync(user),
                    client: await _applicationManager.GetIdAsync(application) ?? throw new InvalidOperationException("Application ID cannot be null"),
                    type: AuthorizationTypes.Permanent,
                    scopes: identity.GetScopes());

                identity.SetAuthorizationId(await _authorizationManager.GetIdAsync(authorization));
                identity.SetDestinations(GetDestinations);

                context.SignIn(new ClaimsPrincipal(identity));
                return;

            // At this point, no authorization was found in the database and an error must be returned
            // if the client application specified prompt=none in the authorization request.
            case ConsentTypes.Explicit when context.Request.HasPromptValue(PromptValues.None):
            case ConsentTypes.Systematic when context.Request.HasPromptValue(PromptValues.None):
                context.Reject(
                    error: OpenIddictConstants.Errors.ConsentRequired,
                    description: "Interactive user consent is required.");
                return;

            // In every other case, render the consent form.
            default:
                // For now, reject and implement consent page later
                context.Reject(
                    error: OpenIddictConstants.Errors.ConsentRequired,
                    description: "User consent is required but consent page is not implemented yet.");
                return;
        }
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
            case "AspNet.Identity.SecurityStamp": yield break;

            default:
                yield return Destinations.AccessToken;
                yield break;
        }
    }
}