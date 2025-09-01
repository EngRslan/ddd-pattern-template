using Engrslan.Dtos;
using Engrslan.Sample.Dtos;
using Engrslan.Sample.Services;
using FastEndpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Engrslan.Product;

public class GetPagedProductsRequest : PagedRequestDto
{
    public string? SearchTerm { get; set; }
    public string? Category { get; set; }
    public bool? IsActive { get; set; }
}

public class GetPagedProductsEndpoint : Endpoint<GetPagedProductsRequest, PagedResultDto<ProductListDto>>
{
    private readonly IProductAppService _productAppService;

    public GetPagedProductsEndpoint(IProductAppService productAppService)
    {
        _productAppService = productAppService;
    }

    public override void Configure()
    {
        Get("/products");
        Options(x => x.WithTags("Products"));
        Summary(s =>
        {
            s.Summary = "Get paged products";
            s.Description = "Gets a paginated list of products with optional filtering";
            s.Responses[200] = "Success - Returns paged products";
        });
        Description(b => b
            .Produces<PagedResultDto<ProductListDto>>(200, "application/json")
            .WithName("GetPagedProducts")
            .WithSummary("Get paged products")
            .WithDescription("Gets a paginated list of products with optional filtering")
            .WithTags("Products"));
    }

    public override async Task HandleAsync(GetPagedProductsRequest req, CancellationToken ct)
    {
        var result = await _productAppService.GetPagedAsync(
            req.Page, 
            req.PageSize, 
            req.SearchTerm, 
            req.Category, 
            req.IsActive, 
            ct);
        
        await Send.OkAsync(result, ct);
    }
}