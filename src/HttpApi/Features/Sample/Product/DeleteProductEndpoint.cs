using Engrslan.Sample.Services;
using FastEndpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Engrslan.Features.Sample.Product;

public class DeleteProductRequest
{
    public Guid Id { get; set; }
}

public class DeleteProductEndpoint : Endpoint<DeleteProductRequest>
{
    private readonly IProductAppService _productAppService;

    public DeleteProductEndpoint(IProductAppService productAppService)
    {
        _productAppService = productAppService;
    }

    public override void Configure()
    {
        Delete("/products/{id}");
        Options(x => x.WithTags("Products"));
        Summary(s =>
        {
            s.Summary = "Delete a product";
            s.Description = "Deletes a product by ID (soft delete)";
            s.Responses[204] = "Success - Product deleted";
            s.Responses[404] = "Not Found - Product not found";
        });
        Description(b => b
            .Produces(204)
            .WithName("DeleteProduct")
            .WithSummary("Delete a product")
            .WithDescription("Deletes a product by ID (soft delete)")
            .WithTags("Products"));
    }

    public override async Task HandleAsync(DeleteProductRequest req, CancellationToken ct)
    {
        await _productAppService.DeleteAsync(req.Id, ct);
        await Send.NoContentAsync(ct);
    }
}