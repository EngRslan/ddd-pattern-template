using Microsoft.AspNetCore.Identity;

namespace Engrslan.Domain.Identity;

public class User : IdentityUser<Guid>
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateTime? CreatedDate { get; set; }
    public DateTime? LastUpdated { get; set; }
}