using System.ComponentModel.DataAnnotations;
using Engrslan.Domain.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Engrslan.Identity.Pages.Account.Manage;

public class Index : PageModel
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly ILogger<Index> _logger;

    public Index(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        ILogger<Index> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
    }

    public string Username { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public bool IsEmailConfirmed { get; set; }
    public bool IsPhoneNumberConfirmed { get; set; }
    public bool IsTwoFactorEnabled { get; set; }
    public int ExternalLoginsCount { get; set; }
    public DateTime? CreatedDate { get; set; }
    public DateTime? LastUpdated { get; set; }

    [TempData]
    public string? StatusMessage { get; set; }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Phone]
        [Display(Name = "Phone number")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "First Name")]
        [StringLength(100, ErrorMessage = "The {0} must be at max {1} characters long.")]
        public string? FirstName { get; set; }

        [Display(Name = "Last Name")]
        [StringLength(100, ErrorMessage = "The {0} must be at max {1} characters long.")]
        public string? LastName { get; set; }
    }

    private async Task LoadAsync(User user)
    {
        var userName = await _userManager.GetUserNameAsync(user);
        var email = await _userManager.GetEmailAsync(user);
        var phoneNumber = await _userManager.GetPhoneNumberAsync(user);

        Username = userName ?? string.Empty;
        UserId = await _userManager.GetUserIdAsync(user);
        IsEmailConfirmed = await _userManager.IsEmailConfirmedAsync(user);
        IsPhoneNumberConfirmed = await _userManager.IsPhoneNumberConfirmedAsync(user);
        IsTwoFactorEnabled = await _userManager.GetTwoFactorEnabledAsync(user);
        
        var logins = await _userManager.GetLoginsAsync(user);
        ExternalLoginsCount = logins.Count;

        // These would need to be added to your User entity
        // CreatedDate = user.CreatedDate;
        // LastUpdated = user.LastUpdated;

        Input = new InputModel
        {
            Email = email ?? string.Empty,
            PhoneNumber = phoneNumber,
            // FirstName = user.FirstName,
            // LastName = user.LastName
        };
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        await LoadAsync(user);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        if (!ModelState.IsValid)
        {
            await LoadAsync(user);
            return Page();
        }

        var email = await _userManager.GetEmailAsync(user);
        if (Input.Email != email)
        {
            var setEmailResult = await _userManager.SetEmailAsync(user, Input.Email);
            if (!setEmailResult.Succeeded)
            {
                StatusMessage = "Error: Failed to set email.";
                return RedirectToPage();
            }
            
            // Also update username if it's the same as email
            if (user.UserName == email)
            {
                var setUserNameResult = await _userManager.SetUserNameAsync(user, Input.Email);
                if (!setUserNameResult.Succeeded)
                {
                    StatusMessage = "Error: Failed to update username.";
                    return RedirectToPage();
                }
            }
        }

        var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
        if (Input.PhoneNumber != phoneNumber)
        {
            var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
            if (!setPhoneResult.Succeeded)
            {
                StatusMessage = "Error: Failed to set phone number.";
                return RedirectToPage();
            }
        }

        // Update additional properties if they exist on your User entity
        // user.FirstName = Input.FirstName;
        // user.LastName = Input.LastName;
        // user.LastUpdated = DateTime.UtcNow;
        // await _userManager.UpdateAsync(user);

        await _signInManager.RefreshSignInAsync(user);
        StatusMessage = "Your profile has been updated";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostSendVerificationEmailAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        if (!ModelState.IsValid)
        {
            await LoadAsync(user);
            return Page();
        }

        var userId = await _userManager.GetUserIdAsync(user);
        var email = await _userManager.GetEmailAsync(user);
        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var callbackUrl = Url.Page(
            "/Account/ConfirmEmail",
            pageHandler: null,
            values: new { area = "", userId = userId, code = code },
            protocol: Request.Scheme) ?? string.Empty;

        // In a real application, send email here
        _logger.LogInformation("Email verification link: {CallbackUrl}", callbackUrl);
        
        StatusMessage = "Verification email sent. Please check your email.";
        return RedirectToPage();
    }
}