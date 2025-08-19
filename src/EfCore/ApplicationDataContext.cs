using Microsoft.EntityFrameworkCore;

namespace CertManager.EfCore;

public class ApplicationDataContext : DbContext
{
    public ApplicationDataContext(DbContextOptions<ApplicationDataContext> options) : base(options)
    {
        
    }
    
}