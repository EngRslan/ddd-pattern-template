using Engrslan.Sample.Dtos;
using Engrslan.Sample.Entities;

namespace Engrslan.Sample.Mappers;

public static class ProductMapper
{
    public static ProductDto ToDto(this Product product)
    {
        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Sku = product.Sku,
            Price = product.Price,
            StockQuantity = product.StockQuantity,
            Category = product.Category,
            IsActive = product.IsActive,
            CreatedBy = product.CreatedBy ?? Guid.Empty,
            CreatedAt = product.CreatedAt,
            ModifiedBy = product.ModifiedBy,
            ModifiedAt = product.ModifiedAt,
            DeletedBy = product.DeletedBy,
            DeletedAt = product.DeletedAt,
            IsDeleted = product.IsDeleted
        };
    }

    public static ProductListDto ToListDto(this Product product)
    {
        return new ProductListDto
        {
            Id = product.Id,
            Name = product.Name,
            Sku = product.Sku,
            Price = product.Price,
            StockQuantity = product.StockQuantity,
            Category = product.Category,
            IsActive = product.IsActive
        };
    }
}