using Microsoft.EntityFrameworkCore;

namespace Engrslan.EfCore;

public class ApplicationDataContext : DbContext
{
    public ApplicationDataContext(DbContextOptions<ApplicationDataContext> options) : base(options)
    {
        
    }
    
}