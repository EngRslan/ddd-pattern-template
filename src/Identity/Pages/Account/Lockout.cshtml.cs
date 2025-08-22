using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Engrslan.Identity.Pages.Account;

[AllowAnonymous]
public class Lockout : PageModel
{
    public void OnGet()
    {
        // This page is shown when an account is locked out
        // No additional logic needed for display
    }
}