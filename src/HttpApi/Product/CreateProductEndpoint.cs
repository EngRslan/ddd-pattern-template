using Engrslan.Sample.Dtos;
using Engrslan.Sample.Services;
using FastEndpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Engrslan.Product;

public class CreateProductEndpoint : Endpoint<CreateProductDto, ProductDto>
{
    private readonly IProductAppService _productAppService;

    public CreateProductEndpoint(IProductAppService productAppService)
    {
        _productAppService = productAppService;
    }
    public override void Configure()
    {
        Post("/products");
        Options(x=>x.WithTags("Products"));
        Summary(s =>
        {
            s.Summary = "Create a new product";
            s.Description = "Creates a new product";
            s.Responses[200] = "Success - Returns the newly created product";
            s.Responses[400] = "Bad Request - Invalid input";
            s.Responses[409] = "Conflict - Product already exists";
        });
        Description(b => b
            .Produces<ProductDto>(200, "application/json")
            .WithName("CreateProduct")
            .WithSummary("Create a new product")
            .WithDescription("Creates a new product")
            .WithTags("Products"));
    }

    public override async Task HandleAsync(CreateProductDto req, CancellationToken ct)
    {
        var result = await _productAppService.CreateAsync(req, ct);
        await Send.OkAsync(result, ct);
    }
}