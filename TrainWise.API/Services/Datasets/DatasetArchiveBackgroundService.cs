using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace TrainWise.API.Services.Datasets;

/// <summary>
/// Background service to periodically archive old datasets.
/// Runs daily to clean up old, unused datasets to reduce storage footprint.
/// </summary>
public sealed class DatasetArchiveBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DatasetArchiveBackgroundService> _logger;
    private readonly TimeSpan _runInterval = TimeSpan.FromHours(24);

    public DatasetArchiveBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<DatasetArchiveBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("🕐 Dataset archive background service started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(_runInterval, stoppingToken);

                using var scope = _serviceProvider.CreateScope();
                var storageService = scope.ServiceProvider.GetRequiredService<IDatasetStorageService>();
                var storageOptions = scope.ServiceProvider.GetRequiredService<IOptions<DatasetStorageOptions>>().Value;

                if (storageOptions.AutoArchiveAfterDays > 0)
                {
                    var archived = await storageService.ArchiveOldAsync(
                        storageOptions.AutoArchiveAfterDays,
                        stoppingToken);

                    if (archived > 0)
                    {
                        _logger.LogInformation($"📦 Archived {archived} old datasets.");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("🛑 Dataset archive background service stopped.");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error during dataset archival.");
            }
        }
    }
}
