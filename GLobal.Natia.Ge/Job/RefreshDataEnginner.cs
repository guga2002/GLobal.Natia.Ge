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
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _provider.CreateScope();

                var freqService = scope.ServiceProvider.GetRequiredService<ISatteliteFrequencyService>();
                var chanellService = scope.ServiceProvider.GetRequiredService<IService>();
                var tempService = scope.ServiceProvider.GetRequiredService<ITemperatureService>();

                var freq = await freqService.GetMonitoringFrequencies();
                var names = await chanellService.GetChanellNames();
                var ports = await chanellService.GetPortsWhereAlarmsIsOn();
                var temp = await tempService.GetCUrrentTemperature();

                try
                {
                    var portsregion = await freqService.GetAllarmsFromRegion();
                    ports.AddRange(portsregion);
                }
                catch (Exception)
                {
                    ports.AddRange(new List<int> { 370, 371, 372, 373 });
                }

                if (ports.Contains(144))
                {
                    if (!ports.Contains(145))
                    {
                        ports.Add(1025);
                        ports.Remove(144);
                    }
                }

                if (ports.Contains(145))
                {
                    try
                    {
                        using var client = new HttpClient
                        {
                            Timeout = TimeSpan.FromSeconds(3)
                        };

                        using var response = await client.GetAsync("https://www.google.com");
                        if (response.IsSuccessStatusCode)
                        {
                            ports.Add(1024);
                            ports.Remove(145);
                        }
                    }
                    catch (Exception)
                    {
                        throw new ArgumentException("გუგა შეცდომა გვაქვს ინტერნეტის შემოწმებისას");
                    }
                }


                foreach (var sat in freq)
                {
                    foreach (var d in sat?.details??new List<SatteliteFrequencyModel>())
                        d.HaveError = ports.Contains(d.PortIn250);
                }

                if (_lastTemperature != temp.temperature || _lastHumidity != temp.humidity)
                {
                    _lastTemperature = temp.temperature;
                    _lastHumidity = temp.humidity;

                    await _hub.Clients.All.SendAsync("temperatureUpdate", new
                    {
                        temperature = temp.temperature,
                        humidity = temp.humidity
                    }, stoppingToken);
                    _logger.LogInformation("🌡 Temperature/humidity update pushed.");
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
                    _logger.LogInformation("📺 Channel status update pushed.");
                }

                if (!CompareSatellite(freq.ToList(), _lastSatellite))
                {
                    _lastSatellite = freq.ToList();

                    await _hub.Clients.All.SendAsync("satelliteMonitoringUpdate", _lastSatellite, stoppingToken);
                    _logger.LogInformation("🛰 Satellite monitoring update pushed.");
                }
            }
            catch (Exception exp)
            {
                exp.InformGuga();
                _logger.LogError(exp, "❌ Error during SignalR data update.");
            }

            await Task.Delay(TimeSpan.FromSeconds(3), stoppingToken);
        }
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
