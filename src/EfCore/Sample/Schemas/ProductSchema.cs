using Engrslan.Sample.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Engrslan.Sample.Schemas;

public class ProductSchema : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.Config("Products");
        
        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(200);
        
        builder.Property(p => p.Description)
            .IsRequired()
            .HasMaxLength(2000);
        
        builder.Property(p => p.Sku)
            .IsRequired()
            .HasMaxLength(50);
        
        builder.HasIndex(p => p.Sku)
            .IsUnique();
        
        builder.Property(p => p.Price)
            .HasPrecision(18, 2)
            .IsRequired();
        
        builder.Property(p => p.StockQuantity)
            .IsRequired()
            .HasDefaultValue(0);
        
        builder.Property(p => p.Category)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(p => p.IsActive)
            .IsRequired()
            .HasDefaultValue(true);
        
        builder.Property(p => p.Weight)
            .HasPrecision(10, 3)
            .IsRequired();
        
        builder.Property(p => p.Unit)
            .IsRequired()
            .HasMaxLength(20)
            .HasDefaultValue("pcs");
        
        builder.HasIndex(p => p.Category);
        builder.HasIndex(p => p.IsActive);
    }
}