namespace Engrslan.Interfaces;

public interface ICurrentUser
{
    string? Id { get; }
    string? UserName { get; }
    string? Email { get; }
    string[] Roles { get; }
    bool IsAuthenticated { get; }
    bool IsInRole(string role);
    string? GetClaim(string claimType);
}