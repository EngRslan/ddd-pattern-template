using CertManager.Domain.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CertManager.Identity.Pages.Account.Manage;

public class TwoFactorAuthentication : PageModel
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly ILogger<TwoFactorAuthentication> _logger;

    public TwoFactorAuthentication(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        ILogger<TwoFactorAuthentication> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
    }

    public bool HasAuthenticator { get; set; }
    public int RecoveryCodesLeft { get; set; }
    public bool Is2faEnabled { get; set; }
    public bool IsMachineRemembered { get; set; }

    [TempData]
    public string? StatusMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        HasAuthenticator = await _userManager.GetAuthenticatorKeyAsync(user) != null;
        Is2faEnabled = await _userManager.GetTwoFactorEnabledAsync(user);
        IsMachineRemembered = await _signInManager.IsTwoFactorClientRememberedAsync(user);
        RecoveryCodesLeft = await _userManager.CountRecoveryCodesAsync(user);

        return Page();
    }

    public async Task<IActionResult> OnPostDisable2faAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        var disable2faResult = await _userManager.SetTwoFactorEnabledAsync(user, false);
        if (!disable2faResult.Succeeded)
        {
            StatusMessage = "Error: Failed to disable 2FA.";
            return RedirectToPage();
        }

        _logger.LogInformation("User with ID '{UserId}' has disabled 2fa.", _userManager.GetUserId(User));
        StatusMessage = "2FA has been disabled. Your account is now less secure.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostGenerateRecoveryCodesAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        var isTwoFactorEnabled = await _userManager.GetTwoFactorEnabledAsync(user);
        var userId = await _userManager.GetUserIdAsync(user);
        if (!isTwoFactorEnabled)
        {
            StatusMessage = "Error: Cannot generate recovery codes for user because they do not have 2FA enabled.";
            return RedirectToPage();
        }

        return RedirectToPage("./ShowRecoveryCodes");
    }
}