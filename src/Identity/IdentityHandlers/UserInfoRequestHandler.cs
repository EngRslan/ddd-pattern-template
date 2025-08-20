using CertManager.Domain.Identity;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using OpenIddict.Abstractions;
using OpenIddict.Server;
using OpenIddict.Server.AspNetCore;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace CertManager.Identity.IdentityHandlers;

public class UserInfoRequestHandler : IOpenIddictServerHandler<OpenIddictServerEvents.HandleUserInfoRequestContext>
{
    private readonly UserManager<User> _userManager;

    public UserInfoRequestHandler(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    public async ValueTask HandleAsync(OpenIddictServerEvents.HandleUserInfoRequestContext context)
    {
        var request = context.Transaction.GetHttpRequest() ?? 
            throw new InvalidOperationException("The HTTP request cannot be retrieved.");
        
        // Retrieve the user principal from the access token
        var result = await request.HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        
        if (!result.Succeeded || result.Principal == null)
        {
            context.Reject(
                error: Errors.InvalidToken,
                description: "The access token is not valid.");
            return;
        }

        // Extract the subject claim from the access token
        var subject = result.Principal.GetClaim(Claims.Subject);
        if (string.IsNullOrEmpty(subject))
        {
            context.Reject(
                error: Errors.InvalidToken,
                description: "The access token is missing the subject claim.");
            return;
        }

        // Retrieve the user corresponding to the subject claim
        var user = await _userManager.FindByIdAsync(subject);
        if (user == null)
        {
            context.Reject(
                error: Errors.InvalidToken,
                description: "The user associated with the access token no longer exists.");
            return;
        }

        // Create the userinfo response
        var claims = new Dictionary<string, object>(StringComparer.Ordinal)
        {
            // Always include the subject claim
            [Claims.Subject] = await _userManager.GetUserIdAsync(user)
        };

        // Add claims based on the granted scopes
        var scopes = result.Principal.GetScopes();

        // Profile scope claims
        if (scopes.Contains(Scopes.Profile))
        {
            claims[Claims.Name] = await _userManager.GetUserNameAsync(user) ?? string.Empty;
            claims[Claims.PreferredUsername] = await _userManager.GetUserNameAsync(user) ?? string.Empty;
            claims[Claims.UpdatedAt] = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            
            // TODO: Add additional profile claims when User class is extended with these properties
            // For now, we only include the basic claims available from IdentityUser
        }

        // Email scope claims
        if (scopes.Contains(Scopes.Email))
        {
            var email = await _userManager.GetEmailAsync(user);
            if (!string.IsNullOrEmpty(email))
            {
                claims[Claims.Email] = email;
                claims[Claims.EmailVerified] = await _userManager.IsEmailConfirmedAsync(user);
            }
        }

        // Phone scope claims
        if (scopes.Contains(Scopes.Phone))
        {
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (!string.IsNullOrEmpty(phoneNumber))
            {
                claims[Claims.PhoneNumber] = phoneNumber;
                claims[Claims.PhoneNumberVerified] = await _userManager.IsPhoneNumberConfirmedAsync(user);
            }
        }

        // Address scope claims
        // TODO: Implement address claims when User class is extended with address properties
        if (scopes.Contains(Scopes.Address))
        {
            // Address claims not yet implemented
        }

        // Roles scope claims
        if (scopes.Contains(Scopes.Roles))
        {
            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Any())
            {
                claims[Claims.Role] = roles.ToArray();
            }
        }

        // Add any custom claims that were included in the access token
        foreach (var claim in result.Principal.Claims)
        {
            // Skip standard OpenID Connect claims that we've already handled
            if (IsStandardClaim(claim.Type))
                continue;

            // Add custom claims with a prefix to avoid conflicts
            if (claim.Type.StartsWith("custom:") || claim.Type.StartsWith("app:"))
            {
                if (!claims.ContainsKey(claim.Type))
                {
                    // Handle multiple values for the same claim type
                    var values = result.Principal.Claims
                        .Where(c => c.Type == claim.Type)
                        .Select(c => c.Value)
                        .ToArray();
                    
                    claims[claim.Type] = values.Length == 1 ? values[0] : values;
                }
            }
        }

        // Set the userinfo response claims
        foreach (var claim in claims)
        {
            var parameterValue = claim.Value switch
            {
                bool boolValue => new OpenIddictParameter(boolValue),
                int intValue => new OpenIddictParameter(intValue),
                long longValue => new OpenIddictParameter(longValue),
                string stringValue => new OpenIddictParameter(stringValue),
                string[] stringArray => new OpenIddictParameter([..stringArray]),
                _ => new OpenIddictParameter(claim.Value.ToString())
            };
            
            context.Claims.Add(claim.Key, parameterValue);
        }
        
        context.HandleRequest();
    }

    private static bool IsStandardClaim(string claimType)
    {
        return claimType switch
        {
            Claims.Subject => true,
            Claims.Name => true,
            Claims.GivenName => true,
            Claims.FamilyName => true,
            Claims.MiddleName => true,
            Claims.Nickname => true,
            Claims.PreferredUsername => true,
            Claims.Profile => true,
            Claims.Picture => true,
            Claims.Website => true,
            Claims.Email => true,
            Claims.EmailVerified => true,
            Claims.Gender => true,
            Claims.Birthdate => true,
            Claims.Zoneinfo => true,
            Claims.Locale => true,
            Claims.PhoneNumber => true,
            Claims.PhoneNumberVerified => true,
            Claims.Address => true,
            Claims.UpdatedAt => true,
            Claims.Role => true,
            _ => false
        };
    }
}