using Engrslan.Files.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Engrslan.Services;

public class CleanFilesBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CleanFilesBackgroundService> _logger;
    private readonly TimeSpan _purgeTemporaryFilesInterval;

    public CleanFilesBackgroundService(IServiceProvider serviceProvider, ILogger<CleanFilesBackgroundService> logger, IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _purgeTemporaryFilesInterval = configuration.GetValue<TimeSpan?>("FileUpload:TempFileCleanupInterval") ?? TimeSpan.FromHours(5);
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Cleaning files...");

            using (var scope = _serviceProvider.CreateScope())
            {
                var services = scope.ServiceProvider.GetRequiredService<IFileRecordAppService>();
                var count = await services.Purge(stoppingToken);
                _logger.LogInformation("{count} Files cleaned", count);
            }
            await Task.Delay(_purgeTemporaryFilesInterval, stoppingToken);
        }
    }
}