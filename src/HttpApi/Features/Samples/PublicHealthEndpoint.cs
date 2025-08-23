using FastEndpoints;
using Microsoft.AspNetCore.Http;

namespace Engrslan.Features.Samples;

/// <summary>
/// Public health check endpoint (no authentication required)
/// </summary>
public class PublicHealthEndpoint : EndpointWithoutRequest<HealthResponse>
{
    public override void Configure()
    {
        Get("/secure/health");
        AllowAnonymous();
        Options(x=>x.WithTags("Samples"));
        Summary(s =>
        {
            s.Summary = "Public health check";
            s.Description = "No authentication required";
            s.Responses[200] = "Service is healthy";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var response = new HealthResponse
        {
            Status = "Healthy",
            Timestamp = DateTime.UtcNow,
            Service = "Engrslan API"
        };

        await Send.OkAsync(response, ct);
    }
}

public class HealthResponse
{
    public string Status { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string Service { get; set; } = string.Empty;
}