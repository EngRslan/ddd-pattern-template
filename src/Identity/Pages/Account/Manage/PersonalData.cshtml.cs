using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using CertManager.Domain.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CertManager.Identity.Pages.Account.Manage;

public class PersonalData : PageModel
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly ILogger<PersonalData> _logger;

    public PersonalData(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        ILogger<PersonalData> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
    }

    public int ExternalLoginsCount { get; set; }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        var logins = await _userManager.GetLoginsAsync(user);
        ExternalLoginsCount = logins.Count;

        return Page();
    }

    public async Task<IActionResult> OnPostDownloadPersonalDataAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        _logger.LogInformation("User with ID '{UserId}' asked for their personal data.", _userManager.GetUserId(User));

        // Only include personal data for download
        var personalData = new Dictionary<string, object>();
        var personalDataProps = typeof(User).GetProperties().Where(
            prop => Attribute.IsDefined(prop, typeof(PersonalDataAttribute)));
        
        foreach (var p in personalDataProps)
        {
            var value = p.GetValue(user);
            if (value != null)
            {
                personalData.Add(p.Name, value);
            }
        }

        // Add additional data
        personalData.Add("Id", await _userManager.GetUserIdAsync(user));
        personalData.Add("UserName", user.UserName ?? string.Empty);
        personalData.Add("Email", user.Email ?? string.Empty);
        personalData.Add("EmailConfirmed", user.EmailConfirmed);
        personalData.Add("PhoneNumber", user.PhoneNumber ?? string.Empty);
        personalData.Add("PhoneNumberConfirmed", user.PhoneNumberConfirmed);
        personalData.Add("TwoFactorEnabled", user.TwoFactorEnabled);

        var logins = await _userManager.GetLoginsAsync(user);
        if (logins.Any())
        {
            personalData.Add("ExternalLogins", logins.Select(l => new { l.LoginProvider, l.ProviderDisplayName }).ToList());
        }

        // Add 2FA authenticator key if it exists
        var authenticatorKey = await _userManager.GetAuthenticatorKeyAsync(user);
        if (!string.IsNullOrEmpty(authenticatorKey))
        {
            personalData.Add("AuthenticatorKey", authenticatorKey);
        }

        Response.Headers.Append("Content-Disposition", "attachment; filename=PersonalData.json");
        return new FileContentResult(JsonSerializer.SerializeToUtf8Bytes(personalData, new JsonSerializerOptions { WriteIndented = true }), "application/json");
    }

    public async Task<IActionResult> OnPostDeleteAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        if (!ModelState.IsValid)
        {
            await OnGetAsync();
            return Page();
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, Input.Password, false);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, "Incorrect password.");
            await OnGetAsync();
            return Page();
        }

        var userId = await _userManager.GetUserIdAsync(user);
        
        // Sign out before deleting
        await _signInManager.SignOutAsync();
        
        // Delete the user
        var deleteResult = await _userManager.DeleteAsync(user);
        if (!deleteResult.Succeeded)
        {
            throw new InvalidOperationException($"Unexpected error occurred deleting user with ID '{userId}'.");
        }

        _logger.LogInformation("User with ID '{UserId}' deleted themselves.", userId);

        return Redirect("~/");
    }
}