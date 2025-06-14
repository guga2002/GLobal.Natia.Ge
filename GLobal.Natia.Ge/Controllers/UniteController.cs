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
        UniteModel mod = new UniteModel();
        try
        {
            var res = await ser.GetMonitoringFrequencies();
            var ports = await chanells.GetPortsWhereAlarmsIsOn();
            try
            {
                var portsregion = await ser.GetAllarmsFromRegion();
                ports.AddRange(portsregion);
            }
            catch (Exception exp)
            {
                await Console.Out.WriteLineAsync(exp.Message);
                exp.InformGuga();
                ports.AddRange(new List<int> { 370, 371, 372, 373 });
            }

            foreach (var item in res)
            {
                foreach (var it in item?.details ?? new List<SatteliteFrequencyModel>())
                {
                    if (ports.Contains(it.PortIn250))
                    {
                        it.HaveError = true;
                    }
                    else
                    {
                        it.HaveError = false;
                    }
                }
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

            mod.satelliteview = res.ToList();
            UniteChanellNamesAndAlarms data = new UniteChanellNamesAndAlarms()
            {
                namees = await chanells.GetChanellNames(),
                ports = await chanells.GetPortsWhereAlarmsIsOn(),
            };
            mod.chyanellnameandalarm = data;
            var result = await temperature.GetCUrrentTemperature();
            mod.temperature = result.temperature;
            mod.Humidity = result.humidity;

            await _hub.Clients.All.SendAsync("temperatureUpdate", new
            {
                result.temperature,
                result.humidity
            });

            await _hub.Clients.All.SendAsync("channelStatusUpdate", new
            {
                data.ports,
                data.namees
            });

            await _hub.Clients.All.SendAsync("satelliteMonitoringUpdate", mod?.satelliteview);

            return View(mod);
        }
        catch (Exception exp)
        {
            exp.InformGuga();
            return View(new UniteModel());
        }
    }
}
