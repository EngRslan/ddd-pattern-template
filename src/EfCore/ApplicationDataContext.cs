using System.Reflection;
using Engrslan.Identity;
using Engrslan.Sample.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
//#if (UseIdentity)
//#endif

namespace Engrslan;

public class ApplicationDataContext 
#if (DisableIdentity)
    : DbContext
#else
    : IdentityDbContext<User,Role,Guid>
#endif
{
    //#if (IncludeSampleCode)
    public DbSet<Product> Products { get; set; } = null!;
    //#endif
    public ApplicationDataContext(DbContextOptions<ApplicationDataContext> options) : base(options)
    {
        
    }
    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(builder);
    }
    
}