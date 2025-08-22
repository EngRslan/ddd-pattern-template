using System.Reflection;
using Engrslan.Domain.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Engrslan.EfCore;

public class IdentityDataContext : IdentityDbContext<User,Role,Guid>
{
    public IdentityDataContext(DbContextOptions<IdentityDataContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(builder);
    }
}