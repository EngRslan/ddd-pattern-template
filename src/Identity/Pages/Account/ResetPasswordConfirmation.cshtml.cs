using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Engrslan.Pages.Account;

[AllowAnonymous]
public class ResetPasswordConfirmation : PageModel
{
    public void OnGet()
    {
    }
}