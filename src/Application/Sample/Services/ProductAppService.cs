using Engrslan.DependencyInjection;
using Engrslan.Dtos;
using Engrslan.Events;
using Engrslan.Mappers;
using Engrslan.Sample.Dtos;
using Engrslan.Sample.Entities;
using Engrslan.Sample.Events;
using Engrslan.Sample.Interfaces;
using Engrslan.Sample.Mappers;
using Engrslan.Services;
using Microsoft.Extensions.Logging;

namespace Engrslan.Sample.Services;

public class ProductAppService : IProductAppService, IScopedService
{
    private readonly IProductRepository _repository;
    private readonly IDateTimeService _dateTimeService;
    private readonly IEventDispatcher _eventDispatcher;
    private readonly ILogger<ProductAppService> _logger;

    public ProductAppService(IProductRepository repository, IDateTimeService dateTimeService, IEventDispatcher eventDispatcher, ILogger<ProductAppService> logger)
    {
        _repository = repository;
        _dateTimeService = dateTimeService;
        _eventDispatcher = eventDispatcher;
        _logger = logger;
    }

    public async Task<ProductDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await _repository.GetByIdAsync(id, cancellationToken);
        return product?.ToDto();
    }

    public async Task<IEnumerable<ProductListDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var products = await _repository.GetAllAsync(cancellationToken);
        return products.Select(ProductMapper.ToListDto).ToList();
    }

    public async Task<PagedResultDto<ProductListDto>> GetPagedAsync(int pageNumber, int pageSize, string? searchTerm = null, 
        string? category = null, bool? isActive = null, CancellationToken cancellationToken = default)
    {
        var query = _repository.Query();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(p => p.Name.Contains(searchTerm) || p.Sku.Contains(searchTerm) || p.Description.Contains(searchTerm));
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(p => p.Category == category);
        }

        if (isActive.HasValue)
        {
            query = query.Where(p => p.IsActive == isActive.Value);
        }

        var result = await _repository.GetPagedAsync(pageNumber, pageSize, 
            query.Any() ? p => query.Any(q => q.Id == p.Id) : null, 
            p => p.Name, true, cancellationToken);

        return result.ToPagedResultDto(ProductMapper.ToListDto);
    }

    public async Task<ProductDto> CreateAsync(CreateProductDto input, CancellationToken cancellationToken = default)
    {
        if (await ExistsBySkuAsync(input.Sku, null, cancellationToken))
        {
            throw new InvalidOperationException($"Product with SKU '{input.Sku}' already exists.");
        }

        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = input.Name,
            Description = input.Description,
            Sku = input.Sku,
            Price = input.Price,
            StockQuantity = input.StockQuantity,
            Category = input.Category,
            IsActive = input.IsActive,
            CreatedAt = _dateTimeService.Now
        };

        var created = await _repository.InsertAsync(product,true, cancellationToken);
        
        await _eventDispatcher.DispatchAsync(new ProductCreatedEvent(created.Id, created.Name, created.Sku), cancellationToken);
        _logger.LogInformation("Product created: {ProductId} - {ProductName}", created.Id, created.Name);

        return created.ToDto();
    }

    public async Task<ProductDto> UpdateAsync(UpdateProductDto input, CancellationToken cancellationToken = default)
    {
        var product = await _repository.GetByIdAsync(input.Id, cancellationToken);
        if (product == null)
        {
            throw new InvalidOperationException($"Product with ID '{input.Id}' not found.");
        }

        if (product.Sku != input.Sku && await ExistsBySkuAsync(input.Sku, input.Id, cancellationToken))
        {
            throw new InvalidOperationException($"Product with SKU '{input.Sku}' already exists.");
        }

        product.Name = input.Name;
        product.Description = input.Description;
        product.Sku = input.Sku;
        product.Price = input.Price;
        product.StockQuantity = input.StockQuantity;
        product.Category = input.Category;
        product.IsActive = input.IsActive;
        product.ModifiedAt = _dateTimeService.Now;

        var updated = await _repository.UpdateAsync(product,true, cancellationToken);
        
        await _eventDispatcher.DispatchAsync(new ProductUpdatedEvent(updated.Id, updated.Name), cancellationToken);
        _logger.LogInformation("Product updated: {ProductId} - {ProductName}", updated.Id, updated.Name);

        return updated.ToDto();
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await _repository.GetByIdAsync(id, cancellationToken);
        if (product == null)
        {
            throw new InvalidOperationException($"Product with ID '{id}' not found.");
        }

        product.IsDeleted = true;
        product.DeletedAt = _dateTimeService.Now;
        await _repository.UpdateAsync(product,true, cancellationToken);
        
        await _eventDispatcher.DispatchAsync(new ProductDeletedEvent(id), cancellationToken);
        _logger.LogInformation("Product deleted: {ProductId}", id);
    }

    public async Task<bool> ExistsBySkuAsync(string sku, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        return await _repository.ExistsAsync(p => p.Sku == sku && (!excludeId.HasValue || p.Id != excludeId.Value), cancellationToken);
    }

    public async Task<int> GetStockQuantityAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await _repository.GetByIdAsync(id, cancellationToken);
        return product?.StockQuantity ?? 0;
    }

    public async Task UpdateStockAsync(Guid id, int quantity, CancellationToken cancellationToken = default)
    {
        var product = await _repository.GetByIdAsync(id, cancellationToken);
        if (product == null)
        {
            throw new InvalidOperationException($"Product with ID '{id}' not found.");
        }

        var previousQuantity = product.StockQuantity;
        product.StockQuantity = quantity;
        product.ModifiedAt = _dateTimeService.Now;

        await _repository.UpdateAsync(product,true, cancellationToken);
        
        await _eventDispatcher.DispatchAsync(new ProductStockUpdatedEvent(id, previousQuantity, quantity), cancellationToken);
        _logger.LogInformation("Product stock updated: {ProductId} from {PreviousQuantity} to {NewQuantity}", id, previousQuantity, quantity);
    }
}




