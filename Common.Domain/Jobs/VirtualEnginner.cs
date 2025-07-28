using Common.Domain.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Common.Domain.Jobs;

public class VirtualEnginner : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<VirtualEnginner> _logger;
    private static DateTime LastCheckHealth = DateTime.MinValue;

    public VirtualEnginner(ILogger<VirtualEnginner> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("VirtualEnginner service started.");

        try
        {
            while (true)
            {
                var date = new DateTime();
                using (var scope = _serviceProvider.CreateScope())
                {
                    var regionChecker = scope.ServiceProvider.GetRequiredService<RegionChecker>();

                    _logger.LogDebug("Running region alarm check...");
                    await regionChecker.GetRegionsWhereAlarmIsOn();

                    _logger.LogDebug("Taking backups...");
                    await regionChecker.TakeBackups();

                    if ((date - LastCheckHealth).TotalSeconds > 60 * 3)
                    {
                        _logger.LogInformation("Performing system health check (every 3 minutes).");
                        LastCheckHealth = date;
                        await regionChecker.CheckSystemHealth();
                    }

                    _logger.LogDebug("Checking region status...");
                    await regionChecker.CheckRegion();

                    _logger.LogDebug("Checking system anomalies...");
                    await regionChecker.CheckAnnomalies();

                    _logger.LogDebug("Checking region messages...");
                    await regionChecker.CheckRegionAndSentMessage();

                    _logger.LogDebug("Checking robot state...");
                    await regionChecker.CheckRobot();

                    _logger.LogInformation("VirtualEnginner cycle completed.");
                }
                await Task.Delay(TimeSpan.FromSeconds(2));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "VirtualEnginner encountered an error.");
        }
    }
}
