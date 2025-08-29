using Engrslan.Dtos;

namespace Engrslan.Sample.Dtos;

public class ProductDto : FullAuditedEntityDto<Guid>
{
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string Sku { get; set; } = null!;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public string Category { get; set; } = null!;
    public bool IsActive { get; set; }
}

public class CreateProductDto
{
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string Sku { get; set; } = null!;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public string Category { get; set; } = null!;
    public bool IsActive { get; set; } = true;
}

public class UpdateProductDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string Sku { get; set; } = null!;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public string Category { get; set; } = null!;
    public bool IsActive { get; set; }
}

public class ProductListDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Sku { get; set; } = null!;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public string Category { get; set; } = null!;
    public bool IsActive { get; set; }
}