using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CertManager.Identity.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(ILogger<IndexModel> logger)
    {
        _logger = logger;
    }

    public void OnGet()
    {
        _logger.LogInformation("User accessed the home page");
    }
}