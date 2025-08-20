using System.Text;
using CertManager.Domain.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace CertManager.Identity.Pages.Account;

[AllowAnonymous]
public class ConfirmEmail : PageModel
{
    private readonly UserManager<User> _userManager;
    private readonly ILogger<ConfirmEmail> _logger;

    public ConfirmEmail(UserManager<User> userManager, ILogger<ConfirmEmail> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    [TempData]
    public string? StatusMessage { get; set; }

    public bool IsSuccess { get; set; }

    public async Task<IActionResult> OnGetAsync(string? userId, string? code)
    {
        if (userId == null || code == null)
        {
            return RedirectToPage("/Index");
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{userId}'.");
        }

        code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
        var result = await _userManager.ConfirmEmailAsync(user, code);
        
        if (result.Succeeded)
        {
            _logger.LogInformation("User with ID '{UserId}' confirmed their email.", userId);
            IsSuccess = true;
            StatusMessage = "Thank you for confirming your email. You can now sign in to your account.";
        }
        else
        {
            IsSuccess = false;
            StatusMessage = "Error confirming your email. The link may have expired or is invalid.";
        }
        
        return Page();
    }
}