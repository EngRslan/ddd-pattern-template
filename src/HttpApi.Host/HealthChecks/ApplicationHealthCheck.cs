using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Engrslan.HttpApi.Host.HealthChecks;

/// <summary>
/// Custom health check for application-specific services and dependencies
/// </summary>
public class ApplicationHealthCheck : IHealthCheck
{
    private readonly ILogger<ApplicationHealthCheck> _logger;
    private readonly IServiceProvider _serviceProvider;

    public ApplicationHealthCheck(
        ILogger<ApplicationHealthCheck> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var data = new Dictionary<string, object>
            {
                ["service"] = "Engrslan API",
                ["version"] = "1.0.0",
                ["environment"] = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production",
                ["timestamp"] = DateTime.UtcNow
            };

            // Check if critical services are registered
            var criticalServices = new[]
            {
                "Engrslan.Domain.Interfaces.IUnitOfWork",
                "Engrslan.Application.Services.ExampleService",
                "Engrslan.HttpApi.Services.CurrentUser"
            };

            var missingServices = new List<string>();

            foreach (var serviceName in criticalServices)
            {
                var serviceType = Type.GetType(serviceName);
                if (serviceType != null)
                {
                    var service = _serviceProvider.GetService(serviceType);
                    if (service == null)
                    {
                        missingServices.Add(serviceName);
                    }
                }
            }

            if (missingServices.Any())
            {
                _logger.LogWarning("Missing critical services: {Services}", string.Join(", ", missingServices));
                return Task.FromResult(HealthCheckResult.Degraded(
                    "Some critical services are not available",
                    data: data));
            }

            // Check if we can access configuration
            var configuration = _serviceProvider.GetService<IConfiguration>();
            if (configuration == null)
            {
                return Task.FromResult(HealthCheckResult.Unhealthy(
                    "Configuration service is not available",
                    data: data));
            }

            data["status"] = "All critical services are operational";
            
            _logger.LogDebug("Application health check passed");
            return Task.FromResult(HealthCheckResult.Healthy(
                "Application is healthy",
                data: data));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Application health check failed");
            
            var data = new Dictionary<string, object>
            {
                ["exception"] = ex.Message,
                ["type"] = ex.GetType().Name
            };

            return Task.FromResult(HealthCheckResult.Unhealthy(
                "Application health check failed",
                exception: ex,
                data: data));
        }
    }
}