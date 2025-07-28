using Common.Domain.Interface;
using Common.Domain.Models.ViewModels;
using Common.Domain.Models;
using Common.Domain.SignalR;
using Common.Persistance.Interface;
using Common.Persistance.Extensions;
using Microsoft.AspNetCore.SignalR;

public class RefreshDataEnginner : BackgroundService
{
    private readonly ILogger<RefreshDataEnginner> _logger;
    private readonly IServiceProvider _provider;
    private readonly IHubContext<UniteMonitoringHub> _hub;

    private string? _lastTemperature;
    private string? _lastHumidity;
    private List<int> _lastPorts = new();
    private Dictionary<int, string> _lastNames = new();
    private List<SatteliteViewMonitoring> _lastSatellite = new();

    public RefreshDataEnginner(
        ILogger<RefreshDataEnginner> logger,
        IServiceProvider provider,
        IHubContext<UniteMonitoringHub> hub)
    {
        _logger = logger;
        _provider = provider;
        _hub = hub;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("🚀 RefreshDataEnginner started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _provider.CreateScope();
                _logger.LogDebug("Creating service scope for data refresh cycle...");

                var freqService = scope.ServiceProvider.GetRequiredService<ISatteliteFrequencyService>();
                var chanellService = scope.ServiceProvider.GetRequiredService<IService>();
                var tempService = scope.ServiceProvider.GetRequiredService<ITemperatureService>();

                _logger.LogDebug("Fetching monitoring data (frequencies, channels, temperature)...");

                var freq = await freqService.GetMonitoringFrequencies();
                var names = await chanellService.GetChanellNames();
                var ports = await chanellService.GetPortsWhereAlarmsIsOn();
                var temp = await tempService.GetCUrrentTemperature();

                _logger.LogDebug("Fetched: {FreqCount} satellites, {ChannelCount} channels, Temp={Temp}, Humidity={Humidity}",
                    freq.Count(), names.Count, temp.temperature, temp.humidity);

                try
                {
                    var portsregion = await freqService.GetAllarmsFromRegion();
                    ports.AddRange(portsregion);
                    _logger.LogDebug("Merged regional alarms: {Count} ports.", portsregion.Count);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "⚠ Failed to fetch regional alarms. Using fallback ports [370,371,372,373].");
                    ports.AddRange(new List<int> { 370, 371, 372, 373 });
                }

                // Special port handling logic
                if (ports.Contains(144))
                {
                    if (!ports.Contains(145))
                    {
                        _logger.LogInformation("Port 144 detected without 145, swapping to 1025.");
                        ports.Add(1025);
                        ports.Remove(144);
                    }
                }

                if (ports.Contains(145))
                {
                    try
                    {
                        _logger.LogInformation("Port 145 detected, checking internet connectivity via Google...");
                        using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(3) };
                        using var response = await client.GetAsync("https://www.google.com");

                        if (response.IsSuccessStatusCode)
                        {
                            _logger.LogInformation("Internet OK. Replacing port 145 with 1024.");
                            ports.Add(1024);
                            ports.Remove(145);
                        }
                        else
                        {
                            _logger.LogWarning("Internet check failed (non-success status).");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "❌ Internet connectivity check failed for port 145.");
                        throw new ArgumentException("გუგა შეცდომა გვაქვს ინტერნეტის შემოწმებისას");
                    }
                }

                // Mark frequency errors based on active ports
                foreach (var sat in freq)
                {
                    foreach (var d in sat?.details ?? new List<SatteliteFrequencyModel>())
                        d.HaveError = ports.Contains(d.PortIn250);
                }

                // Push updates only when changes are detected
                if (_lastTemperature != temp.temperature || _lastHumidity != temp.humidity)
                {
                    _lastTemperature = temp.temperature;
                    _lastHumidity = temp.humidity;

                    await _hub.Clients.All.SendAsync("temperatureUpdate", new
                    {
                        temp.temperature,
                        temp.humidity
                    }, stoppingToken);

                    _logger.LogInformation("🌡 Temperature/Humidity updated: Temp={Temp}, Humidity={Humidity}.",
                        temp.temperature, temp.humidity);
                }

                if (!ports.SequenceEqual(_lastPorts) || !names.OrderBy(k => k.Key).SequenceEqual(_lastNames.OrderBy(k => k.Key)))
                {
                    _lastPorts = ports.ToList();
                    _lastNames = new Dictionary<int, string>(names);

                    await _hub.Clients.All.SendAsync("channelStatusUpdate", new
                    {
                        ports,
                        names
                    }, stoppingToken);

                    _logger.LogInformation("📺 Channel status updated. Ports={PortCount}, Channels={ChannelCount}.",
                        ports.Count, names.Count);
                }

                if (!CompareSatellite(freq.ToList(), _lastSatellite))
                {
                    _lastSatellite = freq.ToList();

                    await _hub.Clients.All.SendAsync("satelliteMonitoringUpdate", _lastSatellite, stoppingToken);
                    _logger.LogInformation("🛰 Satellite monitoring updated. Satellites={SatCount}.", _lastSatellite.Count);
                }
            }
            catch (Exception exp)
            {
                exp.InformGuga();
                _logger.LogError(exp, "❌ Unhandled error during SignalR update cycle.");
            }

            await Task.Delay(TimeSpan.FromSeconds(3), stoppingToken);
        }

        _logger.LogInformation("🛑 RefreshDataEnginner stopping.");
    }

    private bool CompareSatellite(List<SatteliteViewMonitoring> current, List<SatteliteViewMonitoring> previous)
    {
        if (current.Count != previous.Count)
            return false;

        for (int i = 0; i < current.Count; i++)
        {
            var curr = current[i];
            var prev = previous[i];

            if (curr.Degree != prev.Degree || curr.details?.Count != prev?.details?.Count)
                return false;

            for (int j = 0; j < curr?.details?.Count; j++)
            {
                var cd = curr.details[j];
                var pd = prev.details[j];

                if (cd.Frequency != pd.Frequency || cd.Polarisation != pd.Polarisation ||
                    cd.SymbolRate != pd.SymbolRate || cd.PortIn250 != pd.PortIn250 ||
                    cd.HaveError != pd.HaveError)
                    return false;
            }
        }

        return true;
    }
}
