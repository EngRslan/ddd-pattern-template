using Engrslan.Sample.Dtos;
using Engrslan.Sample.Services;
using FastEndpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Engrslan.Features.Sample.Product;

public class GetProductByIdRequest
{
    public Guid Id { get; set; }
}

public class GetProductByIdEndpoint : Endpoint<GetProductByIdRequest, ProductDto>
{
    private readonly IProductAppService _productAppService;

    public GetProductByIdEndpoint(IProductAppService productAppService)
    {
        _productAppService = productAppService;
    }

    public override void Configure()
    {
        Get("/products/{id}");
        Options(x => x.WithTags("Products"));
        Summary(s =>
        {
            s.Summary = "Get product by ID";
            s.Description = "Gets a single product by its ID";
            s.Responses[200] = "Success - Returns the product";
            s.Responses[404] = "Not Found - Product not found";
        });
        Description(b => b
            .Produces<ProductDto>(200, "application/json")
            .Produces(404)
            .WithName("GetProductById")
            .WithSummary("Get product by ID")
            .WithDescription("Gets a single product by its ID")
            .WithTags("Products"));
    }

    public override async Task HandleAsync(GetProductByIdRequest req, CancellationToken ct)
    {
        var result = await _productAppService.GetByIdAsync(req.Id, ct);
        
        if (result == null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }
        
        await Send.OkAsync(result, ct);
    }
}