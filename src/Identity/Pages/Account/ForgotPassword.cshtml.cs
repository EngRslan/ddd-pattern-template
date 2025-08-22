using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using Engrslan.Domain.Identity;
using Engrslan.Domain.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace Engrslan.Identity.Pages.Account;

[AllowAnonymous]
public class ForgotPassword : PageModel
{
    private readonly UserManager<User> _userManager;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<ForgotPassword> _logger;

    public ForgotPassword(
        UserManager<User> userManager,
        IEmailSender emailSender,
        ILogger<ForgotPassword> logger)
    {
        _userManager = userManager;
        _emailSender = emailSender;
        _logger = logger;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var user = await _userManager.FindByEmailAsync(Input.Email);
        if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
        {
            // Don't reveal that the user does not exist or is not confirmed
            // for security reasons
            return RedirectToPage("./ForgotPasswordConfirmation");
        }

        // Generate password reset token
        var code = await _userManager.GeneratePasswordResetTokenAsync(user);
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
        
        var callbackUrl = Url.Page(
            "/Account/ResetPassword",
            pageHandler: null,
            values: new { area = "", code, email = Input.Email },
            protocol: Request.Scheme);

        // Send email
        var emailBody = $@"
            <h2>Reset Your Password</h2>
            <p>Hello {user.UserName},</p>
            <p>You recently requested to reset your password for your Engrslan account.</p>
            <p>Please reset your password by <a href='{HtmlEncoder.Default.Encode(callbackUrl!)}'>clicking here</a>.</p>
            <p>If you did not request a password reset, please ignore this email or contact support if you have concerns.</p>
            <p>This link will expire in 24 hours.</p>
            <hr>
            <p>Thanks,<br>The Engrslan Team</p>
        ";

        await _emailSender.SendEmailAsync(
            Input.Email,
            "Reset Your Password - Engrslan",
            emailBody);

        _logger.LogInformation("Password reset email sent to {Email}", Input.Email);

        return RedirectToPage("./ForgotPasswordConfirmation");
    }
}