using FastEndpoints;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Engrslan.HttpApi.Features.Samples;

/// <summary>
/// Sample protected endpoint demonstrating OpenIddict authentication
/// </summary>
public class GetUserInfoEndpoint : EndpointWithoutRequest<UserInfoResponse>
{
    public override void Configure()
    {
        Get("/secure/userinfo");
        Roles(); // Requires authentication but no specific role
        Options(x=>x.WithTags("Samples"));
        Summary(s =>
        {
            s.Summary = "Get current user information";
            s.Description = "Returns authenticated user's claims and information";
            s.Responses[200] = "User information retrieved successfully";
            s.Responses[401] = "Unauthorized - Invalid or missing token";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var user = HttpContext.User;
        
        var response = new UserInfoResponse
        {
            IsAuthenticated = user.Identity?.IsAuthenticated ?? false,
            Username = user.Identity?.Name ?? "Unknown",
            Claims = user.Claims.Select(c => new ClaimInfo
            {
                Type = c.Type,
                Value = c.Value
            }).ToList(),
            Roles = user.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList()
        };

        await Send.OkAsync(response, ct);
    }
}

public class UserInfoResponse
{
    public bool IsAuthenticated { get; set; }
    public string Username { get; set; } = string.Empty;
    public List<ClaimInfo> Claims { get; set; } = new();
    public List<string> Roles { get; set; } = new();
}

public class ClaimInfo
{
    public string Type { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}