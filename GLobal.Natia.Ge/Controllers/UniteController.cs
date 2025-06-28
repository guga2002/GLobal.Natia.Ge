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
    public UniteController(ISatteliteFrequencyService ser, IService chanells, IService seq, ITemperatureService temperature, IHubContext<UniteMonitoringHub> hub)
    {
        this.ser = ser;
        this.chanells = chanells;
        this.temperature = temperature;
        _hub=hub;
    }

    public async Task<IActionResult> Index()
    {
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
            }
            catch (Exception ex)
            {
                await Console.Out.WriteLineAsync(ex.Message);
                ex.InformGuga();
                ports.AddRange(new List<int> { 370, 371, 372, 373 });
            }

            await Task.WhenAll(freqTask, ipChannelsTask, namesTask, tempTask);

            var frequencies = freqTask.Result;
            var ipChannels = ipChannelsTask.Result;
            var channelNames = namesTask.Result;
            var tempData = tempTask.Result;

            model.IpChanellsThatHaveProblem = ipChannels
                .Where(i =>
                    ((i.Bitrate1Mbps >= 0 && i.Bitrate1Mbps < 0.5) || i.Bitrate1Mbps > 13) &&
                    !i.Name?.ToLower().Contains("test") == true &&
                    !i.Name?.ToLower().Contains("udp") == true)
                .ToList();

            foreach (var item in frequencies)
            {
                foreach (var detail in item?.details ?? Enumerable.Empty<SatteliteFrequencyModel>())
                {
                    detail.HaveError = ports.Contains(detail.PortIn250);
                }
            }

            if (ports.Contains(144) && !ports.Contains(145))
            {
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
                        ports.Remove(145);
                        ports.Add(1024);
                    }
                }
                catch
                {
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
                _hub.Clients.All.SendAsync("temperatureUpdate", new
                {
                    tempData.temperature,
                    tempData.humidity
                }),
                _hub.Clients.All.SendAsync("channelStatusUpdate", new
                {
                    ports,
                    channelNames
                }),
                _hub.Clients.All.SendAsync("satelliteMonitoringUpdate", model.satelliteview)
            );

            return View(model);
        }
        catch (Exception ex)
        {
            ex.InformGuga();
            return View(new UniteModel());
        }
    }

}
