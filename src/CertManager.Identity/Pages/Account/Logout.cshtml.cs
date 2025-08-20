using CertManager.Domain.Identity;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;

namespace CertManager.Identity.Pages.Account;

[AllowAnonymous]
public class Logout : PageModel
{
    private readonly SignInManager<User> _signInManager;
    private readonly ILogger<Logout> _logger;
    private readonly IOpenIddictApplicationManager _applicationManager;

    public Logout(
        SignInManager<User> signInManager, 
        ILogger<Logout> logger,
        IOpenIddictApplicationManager applicationManager)
    {
        _signInManager = signInManager;
        _logger = logger;
        _applicationManager = applicationManager;
    }

    public bool ShowLogoutConfirmation { get; set; } = true;
    
    public string? PostLogoutRedirectUri { get; set; }
    
    public string? ClientName { get; set; }
    
    public string? LogoutId { get; set; }

    public async Task<IActionResult> OnGetAsync(string? logoutId = null, bool logoutCompleted = false)
    {
        LogoutId = logoutId;
        
        // If logout has been completed, show the confirmation page
        if (logoutCompleted)
        {
            ShowLogoutConfirmation = false;
            // Preserve the PostLogoutRedirectUri if it was passed through TempData
            if (TempData.ContainsKey("PostLogoutRedirectUri"))
            {
                PostLogoutRedirectUri = TempData["PostLogoutRedirectUri"]?.ToString();
            }
            return Page();
        }
        
        // For the initial logout request, the user must be authenticated
        if (!User.Identity?.IsAuthenticated ?? true)
        {
            return RedirectToPage("/Index");
        }
        
        // Check if this is an OpenIddict logout request
        var context = HttpContext.GetOpenIddictServerRequest();
        if (context != null)
        {
            PostLogoutRedirectUri = context.PostLogoutRedirectUri;
            
            // Get client name if available
            if (!string.IsNullOrEmpty(context.ClientId))
            {
                var application = await _applicationManager.FindByClientIdAsync(context.ClientId);
                if (application != null)
                {
                    var displayName = await _applicationManager.GetDisplayNameAsync(application);
                    ClientName = displayName ?? context.ClientId;
                }
            }
        }
        
        // If we're coming from an external provider, we might want to auto-logout
        var externalAuthenticationSchemes = await _signInManager.GetExternalAuthenticationSchemesAsync();
        if (externalAuthenticationSchemes.Any())
        {
            // Check if the user is logged in via an external provider
            var externalProvider = (await HttpContext.AuthenticateAsync()).Properties?.Items[".AuthScheme"];
            if (!string.IsNullOrEmpty(externalProvider))
            {
                // We might want to show a different UI for external provider logout
                ViewData["ExternalProvider"] = externalProvider;
            }
        }
        
        // For development, you might want to skip confirmation
        if (HttpContext.Request.Query.ContainsKey("skip_confirmation") && 
            Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
        {
            return await OnPostLogoutAsync();
        }

        return Page();
    }

    public async Task<IActionResult> OnPostLogoutAsync()
    {
        // Store the PostLogoutRedirectUri before signing out
        var postLogoutRedirectUri = Request.Form["postLogoutRedirectUri"].FirstOrDefault();
        
        // Sign out of the application
        await _signInManager.SignOutAsync();
        
        // Sign out of the external provider if applicable
        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
        
        // Clear the authentication cookie
        await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
        
        _logger.LogInformation("User logged out.");
        
        // Handle OpenIddict logout
        var context = HttpContext.GetOpenIddictServerRequest();
        if (context != null && !string.IsNullOrEmpty(context.PostLogoutRedirectUri))
        {
            // SignOut will be handled by OpenIddict's logout handler
            await HttpContext.SignOutAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            
            // Set properties for post-logout redirect
            postLogoutRedirectUri = context.PostLogoutRedirectUri;
        }
        
        // Store the PostLogoutRedirectUri in TempData to preserve it after redirect
        if (!string.IsNullOrEmpty(postLogoutRedirectUri))
        {
            TempData["PostLogoutRedirectUri"] = postLogoutRedirectUri;
        }
        
        ShowLogoutConfirmation = false;
        
        // Redirect to the same page to show the logged out confirmation
        // This ensures the page reloads and the user no longer appears logged in
        return RedirectToPage("./Logout", new { logoutCompleted = true });
    }

    // Alternative method for direct logout (e.g., from a logout link)
    public async Task<IActionResult> OnGetLogoutAsync()
    {
        await _signInManager.SignOutAsync();
        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
        await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
        
        _logger.LogInformation("User logged out directly.");
        
        // Redirect to home page or login page
        return RedirectToPage("/Index");
    }
}