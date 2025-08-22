using CertManager.Domain.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CertManager.EfCore.Schemas;

public class UserDbSchema : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.Property(x => x.FirstName).HasMaxLength(150);
        builder.Property(x => x.LastName).HasMaxLength(150);
        builder.Property(x=>x.CreatedDate).IsRequired().HasDefaultValueSql("GETDATE()");
    }
}