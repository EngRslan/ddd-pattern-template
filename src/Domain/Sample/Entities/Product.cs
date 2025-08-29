using Engrslan.Entities;

namespace Engrslan.Sample.Entities;

public class Product : FullAuditedAggregateRoot<Guid>
{
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string Sku { get; set; } = null!;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public string Category { get; set; } = null!;
    public bool IsActive { get; set; } = true;
    public decimal Weight { get; set; }
    public string Unit { get; set; } = "pcs";
}