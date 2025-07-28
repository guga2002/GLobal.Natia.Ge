using Common.Domain.Interface;
using Common.Domain.Models;
using Common.Domain.Models.ViewModels;
using Common.Domain.SignalR;
using Common.Persistance.Extensions;
using Common.Persistance.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace GLobal.Natia.Ge.Controllers;

public class UniteController : Controller
{
    private readonly ISatteliteFrequencyService ser;
    private readonly IService chanells;
    private readonly ITemperatureService temperature;
    private readonly IHubContext<UniteMonitoringHub> _hub;
    private readonly ILogger<UniteController> _logger;

    public UniteController(ISatteliteFrequencyService ser, IService chanells, IService seq,
        ITemperatureService temperature, IHubContext<UniteMonitoringHub> hub,
        ILogger<UniteController> logger)
    {
        this.ser = ser;
        this.chanells = chanells;
        this.temperature = temperature;
        _hub = hub;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        _logger.LogInformation("Loading UniteController Index view with live monitoring data...");

        var model = new UniteModel();

        try
        {
            var freqTask = ser.GetMonitoringFrequencies();
            var portsTask = chanells.GetPortsWhereAlarmsIsOn();
            var ipChannelsTask = chanells.GetIpChanellsChannelInfoAsync();
            var namesTask = chanells.GetChanellNames();
            var tempTask = temperature.GetCUrrentTemperature();

            var ports = await portsTask;

            try
            {
                var portsRegion = await ser.GetAllarmsFromRegion();
                ports.AddRange(portsRegion);
                _logger.LogInformation("Fetched {Count} additional region alarm ports.", portsRegion.Count);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to fetch region alarms. Using fallback ports.");
                ex.InformGuga();
                ports.AddRange(new List<int> { 370, 371, 372, 373 });
            }

            await Task.WhenAll(freqTask, ipChannelsTask, namesTask, tempTask);

            var frequencies = freqTask.Result;
            var ipChannels = ipChannelsTask.Result;
            var channelNames = namesTask.Result;
            var tempData = tempTask.Result;

            _logger.LogInformation("Fetched monitoring data: {FreqCount} frequencies, {IpCount} IP channels, {ChanCount} names, Temp={Temp}, Humidity={Humidity}",
                frequencies.Count(), ipChannels.Count, channelNames.Count, tempData.temperature, tempData.humidity);

            model.IpChanellsThatHaveProblem = ipChannels
                .Where(i =>
                    ((i.Bitrate1Mbps >= 0 && i.Bitrate1Mbps < 0.5) || i.Bitrate1Mbps > 13) &&
                    !i.Name?.ToLower().Contains("test") == true &&
                    !i.Name?.ToLower().Contains("udp") == true)
                .ToList();

            foreach (var item in frequencies)
            {
                foreach (var detail in item?.details ?? Enumerable.Empty<SatteliteFrequencyModel>())
                    detail.HaveError = ports.Contains(detail.PortIn250);
            }

            if (ports.Contains(144) && !ports.Contains(145))
            {
                _logger.LogWarning("Port 144 detected without 145, replacing with 1025.");
                ports.Remove(144);
                ports.Add(1025);
            }

            if (ports.Contains(145))
            {
                try
                {
                    using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(3) };
                    var response = await client.GetAsync("https://www.google.com");
                    if (response.IsSuccessStatusCode)
                    {
                        _logger.LogInformation("Internet check passed. Replacing port 145 with 1024.");
                        ports.Remove(145);
                        ports.Add(1024);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Internet check failed while validating port 145.");
                    throw new ArgumentException("გუგა შეცდომა გვაქვს ინტერნეტის შემოწმებისას");
                }
            }

            model.satelliteview = frequencies.ToList();
            model.chyanellnameandalarm = new UniteChanellNamesAndAlarms
            {
                namees = channelNames,
                ports = ports
            };
            model.temperature = tempData.temperature;
            model.Humidity = tempData.humidity;

            await Task.WhenAll(
                _hub.Clients.All.SendAsync("temperatureUpdate", new { tempData.temperature, tempData.humidity }),
                _hub.Clients.All.SendAsync("channelStatusUpdate", new { ports, channelNames }),
                _hub.Clients.All.SendAsync("satelliteMonitoringUpdate", model.satelliteview)
            );

            _logger.LogInformation("Pushed updates via SignalR to all clients.");
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception while building Index view.");
            ex.InformGuga();
            return View(new UniteModel());
        }
    }

    [HttpGet("[controller]/api/GetDataForUI")]
    public async Task<IActionResult> GetData()
    {
        _logger.LogInformation("Fetching live monitoring data for API (GetDataForUI).");

        var model = new UnitModelForApi();

        try
        {
            var freqTask = ser.GetMonitoringFrequencies();
            var portsTask = chanells.GetPortsWhereAlarmsIsOn();
            var namesTask = chanells.GetChanellNames();
            var tempTask = temperature.GetCUrrentTemperature();

            var ports = await portsTask;

            try
            {
                var portsRegion = await ser.GetAllarmsFromRegion();
                ports.AddRange(portsRegion);
                _logger.LogInformation("Added {Count} region ports to monitoring data.", portsRegion.Count);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to fetch region alarms. Using fallback ports.");
                ex.InformGuga();
                await Console.Out.WriteLineAsync($"Failed to fetch region alarms: {ex.Message}");
                ports.AddRange(new[] { 370, 371, 372, 373 });
            }

            await Task.WhenAll(freqTask, namesTask, tempTask);

            var frequencies = await freqTask;
            var channelNames = await namesTask;
            var tempData = await tempTask;

            foreach (var freq in frequencies)
            {
                if (freq?.details == null) continue;

                foreach (var detail in freq.details)
                    detail.HaveError = ports.Contains(detail.PortIn250);
            }

            if (ports.Contains(144) && !ports.Contains(145))
            {
                _logger.LogWarning("Port 144 present without 145. Replacing with 1025.");
                ports.Remove(144);
                ports.Add(1025);
            }

            if (ports.Contains(145))
            {
                try
                {
                    using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(3) };
                    var response = await client.GetAsync("https://www.google.com");
                    if (response.IsSuccessStatusCode)
                    {
                        _logger.LogInformation("Internet OK. Replacing port 145 with 1024.");
                        ports.Remove(145);
                        ports.Add(1024);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Internet check failed while validating port 145 (API).");
                    throw new ArgumentException("გუგა შეცდომა გვაქვს ინტერნეტის შემოწმებისას", ex);
                }
            }

            model.SatelliteView = frequencies.ToList();
            model.ChanellInfo = channelNames.Select(c => new ChanellInfoModel
            {
                ChanellName = c.Value,
                IsDIsable = c.Value.Equals("test", StringComparison.OrdinalIgnoreCase),
                Order = c.Key,
                HaveError = ports.Contains(c.Key)
            }).ToList();

            model.TemperatureInfo = new TemperatureView
            {
                humidity = tempData.humidity,
                temperature = tempData.temperature
            };

            _logger.LogInformation("Built API model with {FreqCount} frequencies and {ChanCount} channels.",
                model.SatelliteView.Count, model.ChanellInfo.Count);

            return Ok(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception in GetDataForUI.");
            ex.InformGuga();
            return Ok(new UniteModel());
        }
    }
}
