using System.Security.Claims;
using Engrslan.DependencyInjection;
using Engrslan.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Engrslan.Services;

public class CurrentUser : ICurrentUser, IScopedService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? Principal => _httpContextAccessor.HttpContext?.User;

    public string? Id => Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    public string? UserName => Principal?.FindFirst(ClaimTypes.Name)?.Value;

    public string? Email => Principal?.FindFirst(ClaimTypes.Email)?.Value;

    public string[] Roles => Principal?.FindAll(ClaimTypes.Role)
        .Select(c => c.Value)
        .ToArray() ?? [];

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated ?? false;

    public bool IsInRole(string role) => Principal?.IsInRole(role) ?? false;

    public string? GetClaim(string claimType) => Principal?.FindFirst(claimType)?.Value;
}