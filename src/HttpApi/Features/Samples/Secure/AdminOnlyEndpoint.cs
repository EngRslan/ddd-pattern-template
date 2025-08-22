using FastEndpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace Engrslan.HttpApi.Features.Samples;

/// <summary>
/// Admin-only endpoint demonstrating role-based authorization
/// </summary>
public class AdminOnlyEndpoint : EndpointWithoutRequest<AdminResponse>
{
    public override void Configure()
    {
        Get("/secure/admin");
        Roles("Administrator");
        Options(x=>x.WithTags("Samples"));
        Summary(s =>
        {
            s.Summary = "Admin-only endpoint";
            s.Description = "Requires Administrator role to access";
            s.Responses[200] = "Success - Admin access granted";
            s.Responses[401] = "Unauthorized - Invalid or missing token";
            s.Responses[403] = "Forbidden - User lacks Administrator role";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var response = new AdminResponse
        {
            Message = "Welcome Admin!",
            AccessTime = DateTime.UtcNow,
            Username = HttpContext.User.Identity?.Name ?? "Unknown"
        };

        await Send.OkAsync(response, ct);
    }
}

public class AdminResponse
{
    public string Message { get; set; } = string.Empty;
    public DateTime AccessTime { get; set; }
    public string Username { get; set; } = string.Empty;
}