using Engrslan.Dtos;
using Engrslan.Sample.Dtos;

namespace Engrslan.Sample.Services;

public interface IProductAppService
{
    Task<ProductDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<ProductListDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<PagedResultDto<ProductListDto>> GetPagedAsync(int pageNumber, int pageSize, string? searchTerm = null, string? category = null, bool? isActive = null, CancellationToken cancellationToken = default);
    Task<ProductDto> CreateAsync(CreateProductDto input, CancellationToken cancellationToken = default);
    Task<ProductDto> UpdateAsync(UpdateProductDto input, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsBySkuAsync(string sku, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task<int> GetStockQuantityAsync(Guid id, CancellationToken cancellationToken = default);
    Task UpdateStockAsync(Guid id, int quantity, CancellationToken cancellationToken = default);
}