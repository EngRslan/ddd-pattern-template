using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CertManager.Identity.Pages.Account;

[AllowAnonymous]
public class ResetPasswordConfirmation : PageModel
{
    public void OnGet()
    {
    }
}