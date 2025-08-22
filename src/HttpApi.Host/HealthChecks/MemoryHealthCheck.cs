using System.Diagnostics;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace Engrslan.HttpApi.Host.HealthChecks;

/// <summary>
/// Health check for monitoring memory usage
/// </summary>
public class MemoryHealthCheck : IHealthCheck
{
    private readonly ILogger<MemoryHealthCheck> _logger;
    private readonly MemoryHealthCheckOptions _options;

    public MemoryHealthCheck(
        ILogger<MemoryHealthCheck> logger,
        IOptionsMonitor<MemoryHealthCheckOptions> options)
    {
        _logger = logger;
        _options = options.CurrentValue;
    }

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Get current process
            using var process = Process.GetCurrentProcess();
            
            // Get memory info
            var workingSet = process.WorkingSet64;
            var privateMemory = process.PrivateMemorySize64;
            var virtualMemory = process.VirtualMemorySize64;
            var gcMemory = GC.GetTotalMemory(false);
            
            // Convert to MB for readability
            var workingSetMB = workingSet / 1024.0 / 1024.0;
            var privateMemoryMB = privateMemory / 1024.0 / 1024.0;
            var virtualMemoryMB = virtualMemory / 1024.0 / 1024.0;
            var gcMemoryMB = gcMemory / 1024.0 / 1024.0;

            var data = new Dictionary<string, object>
            {
                ["workingSet_MB"] = Math.Round(workingSetMB, 2),
                ["privateMemory_MB"] = Math.Round(privateMemoryMB, 2),
                ["virtualMemory_MB"] = Math.Round(virtualMemoryMB, 2),
                ["gcMemory_MB"] = Math.Round(gcMemoryMB, 2),
                ["gen0_collections"] = GC.CollectionCount(0),
                ["gen1_collections"] = GC.CollectionCount(1),
                ["gen2_collections"] = GC.CollectionCount(2),
                ["totalProcessorTime_seconds"] = process.TotalProcessorTime.TotalSeconds,
                ["threads"] = process.Threads.Count
            };

            // Check memory thresholds
            if (workingSetMB > _options.MaximumWorkingSetMB)
            {
                _logger.LogWarning("Memory usage is high: {WorkingSet} MB", workingSetMB);
                return Task.FromResult(HealthCheckResult.Degraded(
                    $"High memory usage: {workingSetMB:F2} MB (threshold: {_options.MaximumWorkingSetMB} MB)",
                    data: data));
            }

            if (workingSetMB > _options.CriticalWorkingSetMB)
            {
                _logger.LogError("Memory usage is critical: {WorkingSet} MB", workingSetMB);
                return Task.FromResult(HealthCheckResult.Unhealthy(
                    $"Critical memory usage: {workingSetMB:F2} MB (threshold: {_options.CriticalWorkingSetMB} MB)",
                    data: data));
            }

            _logger.LogDebug("Memory health check passed. Working set: {WorkingSet} MB", workingSetMB);
            return Task.FromResult(HealthCheckResult.Healthy(
                $"Memory usage is normal: {workingSetMB:F2} MB",
                data: data));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Memory health check failed");
            
            return Task.FromResult(HealthCheckResult.Unhealthy(
                "Failed to check memory status",
                exception: ex));
        }
    }
}

/// <summary>
/// Options for memory health check
/// </summary>
public class MemoryHealthCheckOptions
{
    /// <summary>
    /// Maximum working set in MB before reporting degraded status
    /// </summary>
    public double MaximumWorkingSetMB { get; set; } = 1024; // 1 GB

    /// <summary>
    /// Critical working set in MB before reporting unhealthy status
    /// </summary>
    public double CriticalWorkingSetMB { get; set; } = 2048; // 2 GB
}