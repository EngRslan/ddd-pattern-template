using CertManager.Domain.Identity;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using OpenIddict.Abstractions;
using OpenIddict.Server;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace CertManager.Identity.IdentityHandlers;

public class LogoutRequestHandler : IOpenIddictServerHandler<OpenIddictServerEvents.HandleEndSessionRequestContext>
{
    private readonly IOpenIddictApplicationManager _applicationManager;
    private readonly IOpenIddictAuthorizationManager _authorizationManager;
    private readonly IOpenIddictTokenManager _tokenManager;
    private readonly SignInManager<User> _signInManager;
    private readonly UserManager<User> _userManager;

    public LogoutRequestHandler(
        IOpenIddictApplicationManager applicationManager,
        IOpenIddictAuthorizationManager authorizationManager,
        IOpenIddictTokenManager tokenManager,
        SignInManager<User> signInManager,
        UserManager<User> userManager)
    {
        _applicationManager = applicationManager;
        _authorizationManager = authorizationManager;
        _tokenManager = tokenManager;
        _signInManager = signInManager;
        _userManager = userManager;
    }

    public async ValueTask HandleAsync(OpenIddictServerEvents.HandleEndSessionRequestContext context)
    {
        var request = context.Transaction.GetHttpRequest() ?? 
            throw new InvalidOperationException("The HTTP request cannot be retrieved.");
        
        // Retrieve the identity of the authenticated user, if available.
        var result = await request.HttpContext.AuthenticateAsync(IdentityConstants.ApplicationScheme);
        
        // If the user is not authenticated, challenge them to log in before logging out
        if (!result.Succeeded || result.Principal == null)
        {
            // If the client requested a prompt= none, logout, reject the request
            if (context.Request.HasPromptValue(PromptValues.None))
            {
                context.Reject(
                    error: Errors.LoginRequired,
                    description: "The user is not logged in.");
                return;
            }

            // Challenge the user to authenticate
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

        // Retrieve the user corresponding to the authenticated principal
        var user = await _userManager.GetUserAsync(result.Principal);
        if (user == null)
        {
            context.Reject(
                error: Errors.ServerError,
                description: "The user details cannot be retrieved.");
            return;
        }

        // If an id_token_hint was provided, validate it
        // TODO: Implement id_token_hint validation when JWT token validation is set up
        if (!string.IsNullOrEmpty(context.Request.IdTokenHint))
        {
            // For now, we skip validation of id_token_hint
            // In production, you would validate the JWT token and ensure it matches the authenticated user
        }

        // Revoke all tokens and authorizations associated with the user and client
        if (!string.IsNullOrEmpty(context.Request.ClientId))
        {
            var application = await _applicationManager.FindByClientIdAsync(context.Request.ClientId);
            if (application != null)
            {
                var subject = await _userManager.GetUserIdAsync(user);
                var client = await _applicationManager.GetIdAsync(application);

                // Revoke all authorizations for this user and client
                var authorizationsEnumerable = _authorizationManager.FindAsync(
                    subject: subject,
                    client: client,
                    status: Statuses.Valid,
                    type: null,
                    scopes: null);

                var authorizations = new List<object>();
                await foreach (var authorization in authorizationsEnumerable)
                {
                    authorizations.Add(authorization);
                }

                foreach (var authorization in authorizations)
                {
                    await _authorizationManager.TryRevokeAsync(authorization);
                }

                // Revoke all tokens for this user and client
                var tokensEnumerable = _tokenManager.FindAsync(
                    subject: subject,
                    client: client,
                    status: Statuses.Valid,
                    type: null);

                var tokens = new List<object>();
                await foreach (var token in tokensEnumerable)
                {
                    tokens.Add(token);
                }

                foreach (var token in tokens)
                {
                    await _tokenManager.TryRevokeAsync(token);
                }
            }
        }

        // Sign the user out of the application
        await _signInManager.SignOutAsync();
        
        // Sign the user out of the external identity provider if applicable
        await request.HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

        // If a post_logout_redirect_uri was provided, validate it
        if (!string.IsNullOrEmpty(context.Request.PostLogoutRedirectUri))
        {
            if (string.IsNullOrEmpty(context.Request.ClientId))
            {
                context.Reject(
                    error: Errors.InvalidRequest,
                    description: "The client_id parameter must be specified when using post_logout_redirect_uri.");
                return;
            }

            var application = await _applicationManager.FindByClientIdAsync(context.Request.ClientId);
            if (application == null)
            {
                context.Reject(
                    error: Errors.InvalidClient,
                    description: "The specified client application was not found.");
                return;
            }

            // Validate the post_logout_redirect_uri against the registered URIs
            var postLogoutRedirectUris = await _applicationManager.GetPostLogoutRedirectUrisAsync(application);
            
            if (!postLogoutRedirectUris.Contains(context.Request.PostLogoutRedirectUri))
            {
                context.Reject(
                    error: Errors.InvalidRequest,
                    description: "The specified post_logout_redirect_uri is not valid for this client application.");
                return;
            }

            // Return the user to the post_logout_redirect_uri
            request.HttpContext.Response.Redirect(context.Request.PostLogoutRedirectUri + 
                (string.IsNullOrEmpty(context.Request.State) ? "" : $"?state={context.Request.State}"));
        }
        else
        {
            // Return the user to a default logout confirmation page
            request.HttpContext.Response.Redirect("/");
        }

        context.HandleRequest();
    }
}