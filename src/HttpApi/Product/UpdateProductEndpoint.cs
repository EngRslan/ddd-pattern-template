using Engrslan.Sample.Dtos;
using Engrslan.Sample.Services;
using FastEndpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Engrslan.Product;

public class UpdateProductEndpoint : Endpoint<UpdateProductDto, ProductDto>
{
    private readonly IProductAppService _productAppService;

    public UpdateProductEndpoint(IProductAppService productAppService)
    {
        _productAppService = productAppService;
    }

    public override void Configure()
    {
        Put("/products/{id}");
        Options(x => x.WithTags("Products"));
        Summary(s =>
        {
            s.Summary = "Update a product";
            s.Description = "Updates an existing product";
            s.Responses[200] = "Success - Returns the updated product";
            s.Responses[400] = "Bad Request - Invalid input";
            s.Responses[404] = "Not Found - Product not found";
            s.Responses[409] = "Conflict - SKU already exists";
        });
        Description(b => b
            .Produces<ProductDto>(200, "application/json")
            .Produces(400)
            .Produces(404)
            .Produces(409)
            .WithName("UpdateProduct")
            .WithSummary("Update a product")
            .WithDescription("Updates an existing product")
            .WithTags("Products"));
    }

    public override async Task HandleAsync(UpdateProductDto req, CancellationToken ct)
    {
        var result = await _productAppService.UpdateAsync(req, ct);
        await Send.OkAsync(result, ct);
    }
}