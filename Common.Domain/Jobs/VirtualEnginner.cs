using Common.Domain.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Common.Domain.Jobs;

public class VirtualEnginner : BackgroundService
{

    private readonly IServiceProvider _serviceProvider;
    private static DateTime LastCheckHealth = DateTime.MinValue;
    public VirtualEnginner(ILogger<VirtualEnginner> logger, IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            while (true)
            {
                var date = new DateTime();
                using (var scope = _serviceProvider.CreateScope())
                {
                    var regionChecker = scope.ServiceProvider.GetRequiredService<RegionChecker>();
                    await regionChecker.GetRegionsWhereAlarmIsOn();

                    await regionChecker.TakeBackups();

                    if ((date - LastCheckHealth).TotalSeconds > 60 * 3)
                    {
                        LastCheckHealth = date;
                        await regionChecker.CheckSystemHealth();
                    }
                    await regionChecker.CheckRegion();
                    await regionChecker.CheckAnnomalies();
                    await regionChecker.CheckRegionAndSentMessage();
                    await regionChecker.CheckRobot();
                }
                await Task.Delay(TimeSpan.FromSeconds(2));
            }
        }
        catch (Exception ex)
        {
            await Console.Out.WriteLineAsync(ex.Message);
        }
    }
}
