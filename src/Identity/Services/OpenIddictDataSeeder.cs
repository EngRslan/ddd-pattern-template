using CertManager.Domain.Identity;
using Microsoft.AspNetCore.Identity;
using OpenIddict.Abstractions;

namespace CertManager.Identity.Services;

public class OpenIddictDataSeeder : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;
    private readonly ILogger<OpenIddictDataSeeder> _logger;

    public OpenIddictDataSeeder(
        IServiceProvider serviceProvider,
        IConfiguration configuration,
        ILogger<OpenIddictDataSeeder> logger)
    {
        _serviceProvider = serviceProvider;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        
        try
        {
            _logger.LogInformation("Starting OpenIddict data seeding...");
            
            // Seed OpenIddict applications
            await SeedApplicationsAsync(scope.ServiceProvider, cancellationToken);
            
            // Seed OpenIddict scopes
            await SeedScopesAsync(scope.ServiceProvider, cancellationToken);
            
            // Seed roles
            await SeedRolesAsync(scope.ServiceProvider, cancellationToken);
            
            // Seed users
            await SeedUsersAsync(scope.ServiceProvider, cancellationToken);
            
            _logger.LogInformation("OpenIddict data seeding completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding OpenIddict data");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private async Task SeedApplicationsAsync(IServiceProvider services, CancellationToken cancellationToken)
    {
        var applicationManager = services.GetRequiredService<IOpenIddictApplicationManager>();
        
        // Empty list of applications to seed - add your applications here
        var applications = new List<OpenIddictApplicationDescriptor>
        {
            // Example:
            // new OpenIddictApplicationDescriptor
            // {
            //     ClientId = "my-client-id",
            //     ClientSecret = "my-client-secret",
            //     DisplayName = "My Application",
            //     Type = ClientTypes.Confidential,
            //     // ... other properties
            // }
        };

        foreach (var applicationDescriptor in applications)
        {
            var existingApp = await applicationManager.FindByClientIdAsync(applicationDescriptor.ClientId!, cancellationToken);
            
            if (existingApp == null)
            {
                await applicationManager.CreateAsync(applicationDescriptor, cancellationToken);
                _logger.LogInformation("Created application: {ClientId}", applicationDescriptor.ClientId);
            }
            else
            {
                await applicationManager.UpdateAsync(existingApp, applicationDescriptor, cancellationToken);
                _logger.LogInformation("Updated application: {ClientId}", applicationDescriptor.ClientId);
            }
        }

        if (applications.Count == 0)
        {
            _logger.LogInformation("No applications to seed");
        }
    }

    private async Task SeedScopesAsync(IServiceProvider services, CancellationToken cancellationToken)
    {
        var scopeManager = services.GetRequiredService<IOpenIddictScopeManager>();
        
        // Empty list of scopes to seed - add your scopes here
        var scopes = new List<OpenIddictScopeDescriptor>
        {
            // Example:
            // new OpenIddictScopeDescriptor
            // {
            //     Name = "api",
            //     DisplayName = "API Access",
            //     Description = "Allows access to the API",
            //     Resources = { "resource-server" }
            // }
        };

        foreach (var scopeDescriptor in scopes)
        {
            var existingScope = await scopeManager.FindByNameAsync(scopeDescriptor.Name!, cancellationToken);
            
            if (existingScope == null)
            {
                await scopeManager.CreateAsync(scopeDescriptor, cancellationToken);
                _logger.LogInformation("Created scope: {ScopeName}", scopeDescriptor.Name);
            }
            else
            {
                await scopeManager.UpdateAsync(existingScope, scopeDescriptor, cancellationToken);
                _logger.LogInformation("Updated scope: {ScopeName}", scopeDescriptor.Name);
            }
        }

        if (scopes.Count == 0)
        {
            _logger.LogInformation("No scopes to seed");
        }
    }

    private async Task SeedRolesAsync(IServiceProvider services, CancellationToken cancellationToken)
    {
        var roleManager = services.GetRequiredService<RoleManager<Role>>();
        
        // Empty list of roles to seed - add your roles here
        var roles = new List<string>
        {
            // Example:
            "Administrator",
            // "Manager",
            // "User"
        };

        foreach (var roleName in roles)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                var role = new Role { Name = roleName };
                var result = await roleManager.CreateAsync(role);
                
                if (result.Succeeded)
                {
                    _logger.LogInformation("Created role: {RoleName}", roleName);
                }
                else
                {
                    _logger.LogError("Failed to create role {RoleName}: {Errors}", 
                        roleName, string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
            else
            {
                _logger.LogInformation("Role already exists: {RoleName}", roleName);
            }
        }

        if (roles.Count == 0)
        {
            _logger.LogInformation("No roles to seed");
        }
    }

    private async Task SeedUsersAsync(IServiceProvider services, CancellationToken cancellationToken)
    {
        var userManager = services.GetRequiredService<UserManager<User>>();
        
        // Empty list of users to seed - add your users here
        var users = new List<(User User, string Password, string[] Roles)>
        {
            // Example:
            (
                new User 
                { 
                    UserName = "admin@example.com", 
                    Email = "admin@example.com", 
                    EmailConfirmed = true 
                },
                "Admin@123456",
                ["Administrator"]
            )
        };

        foreach (var (userData, password, userRoles) in users)
        {
            var existingUser = await userManager.FindByEmailAsync(userData.Email!);
            
            if (existingUser == null)
            {
                var result = await userManager.CreateAsync(userData, password);
                
                if (result.Succeeded)
                {
                    _logger.LogInformation("Created user: {Email}", userData.Email);
                    
                    // Add roles to user
                    if (userRoles.Length != 0)
                    {
                        await userManager.AddToRolesAsync(userData, userRoles);
                        _logger.LogInformation("Added roles {Roles} to user: {Email}", 
                            string.Join(", ", userRoles), userData.Email);
                    }
                }
                else
                {
                    _logger.LogError("Failed to create user {Email}: {Errors}", 
                        userData.Email, string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
            else
            {
                _logger.LogInformation("User already exists: {Email}", userData.Email);
                
                // Update roles if needed
                var currentRoles = await userManager.GetRolesAsync(existingUser);
                var rolesToAdd = userRoles.Except(currentRoles).ToList();
                var rolesToRemove = currentRoles.Except(userRoles).ToList();
                
                if (rolesToAdd.Count != 0)
                {
                    await userManager.AddToRolesAsync(existingUser, rolesToAdd);
                    _logger.LogInformation("Added new roles {Roles} to existing user: {Email}", 
                        string.Join(", ", rolesToAdd), existingUser.Email);
                }
                
                if (rolesToRemove.Count != 0)
                {
                    await userManager.RemoveFromRolesAsync(existingUser, rolesToRemove);
                    _logger.LogInformation("Removed roles {Roles} from existing user: {Email}", 
                        string.Join(", ", rolesToRemove), existingUser.Email);
                }
            }
        }

        if (users.Count == 0)
        {
            _logger.LogInformation("No users to seed");
        }
    }
}