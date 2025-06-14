using System.Net.NetworkInformation;
using Common.Domain.Interface;
using Common.Domain.Models;
using Common.Domain.Models.Region;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Common.Domain.Helpers;

public class RegionChecker
{
    private readonly IRegionsServices _ser;
    private readonly IRedisService _redis;
    private readonly HttpClient _client;
    private readonly IGetEmrDataServices RegionEmr;
    private readonly INatiaHealthCheck _health;
    private readonly IConfiguration _config;
    private readonly ILogger<RegionChecker> _logger;
    private readonly IBackupService _backupService;

    private static DateTime LastSentTimeTelavi = DateTime.MinValue;
    private static DateTime LastSentTimeGori = DateTime.MinValue;
    private static DateTime LastSentTimeKutaisi = DateTime.MinValue;
    private static DateTime LastSentTimeFoti = DateTime.MinValue;

    public RegionChecker(
        IRegionsServices ser,
        IRedisService redis,
        HttpClient client,
        IGetEmrDataServices RegionEmr,
        INatiaHealthCheck health,
        IConfiguration config,
        ILogger<RegionChecker> logger,
        IBackupService backupService)
    {
        _client = client;
        this.RegionEmr = RegionEmr;
        _health = health;
        _config = config;
        _ser = ser;
        _redis = redis;
        _logger = logger;
        _backupService = backupService;
    }

    public async Task CheckRobot()
    {
        _logger.LogInformation("Checking robot health...");
        await _health.CheckHealth();
    }

    public async Task TakeBackups()
    {
        _logger.LogInformation("Checking if it's time to take backup...");
        if (_backupService.IsTimeForBackup())
        {
            _logger.LogInformation("Backup time reached, starting backup...");
            await _backupService.Start();
        }
        else
        {
            _logger.LogWarning("Backup not triggered: No backup time");
        }
    }

    public async Task CheckRegion()
    {
        List<string> lst = new List<string>();
        try
        {
            _logger.LogInformation("Checking optic connection and alarm thresholds...");

            if (!await _ser.OpticIsOn("192.168.15.10")) lst.Add("1510");
            if (!await _ser.OpticIsOn("192.168.15.20")) lst.Add("1510");
            if (!await _ser.OpticIsOn("192.168.15.30")) lst.Add("1510");

            if (!await _ser.OpticIsOn("192.168.14.10")) lst.Add("1410");
            if (!await _ser.OpticIsOn("192.168.14.20")) lst.Add("1410");

            if (!await _ser.OpticIsOn("192.168.25.10")) lst.Add("2510");
            if (!await _ser.OpticIsOn("192.168.25.20")) lst.Add("2510");

            if (!await _ser.OpticIsOn("192.168.13.10")) lst.Add("1310");
            if (!await _ser.OpticIsOn("192.168.13.20")) lst.Add("1310");

            var qutaisi = await _ser.GetCount("15.10");
            var qutaisi2 = await _ser.GetCount("15.20");
            var qutaisi3 = await _ser.GetCount("15.30");

            var foti = await _ser.GetCount("14.10");
            var foti1 = await _ser.GetCount("14.20");

            var telavi = await _ser.GetCount("25.10");
            var telavi2 = await _ser.GetCount("25.20");

            var gori = await _ser.GetCount("13.10");
            var gori1 = await _ser.GetCount("13.20");

            if (qutaisi >= 2 || qutaisi2 >= 2 || qutaisi3 >= 2) lst.Add("333");
            if (foti >= 2 || foti1 >= 2) lst.Add("444");

            if (telavi >= 2 || telavi2 >= 2) lst.Add("555");

            if (gori >= 2 || gori1 >= 2) lst.Add("666");

            if (lst.Any())
            {
                await _redis.SetAsync("RegionInfo", lst, TimeSpan.FromSeconds(30));
                _logger.LogInformation("Region issues detected and cached: {@lst}", lst);
            }
        }
        catch (Exception exp)
        {
            _logger.LogError(exp, "Exception while checking region");
        }
    }

    public async Task GetRegionsWhereAlarmIsOn()
    {
        List<int> regions = new List<int>();
        var regionIps = new Dictionary<string, int>
        { 
            { "192.168.14.10", 370 },
            { "192.168.14.20", 370 },

            { "192.168.13.10", 373 },
            { "192.168.13.20", 373 },

            { "192.168.15.10", 371 },
            { "192.168.15.20", 371 },
            { "192.168.15.30", 371 },

            { "192.168.25.10", 372 },
            { "192.168.25.20", 372 },
        };

        _logger.LogInformation("Pinging regions to detect offline ones...");
        var pingTasks = regionIps.Select(async region =>
        {
            using var pinger = new Ping();
            try
            {
                var pingResult = await pinger.SendPingAsync(region.Key, 1000);
                if (pingResult.Status != IPStatus.Success)
                {
                    regions.Add(region.Value);
                    _logger.LogWarning("Region offline: {Region}", region.Key);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ping failed for region {Region}", region.Key);
            }
        }).ToList();

        await Task.WhenAll(pingTasks);
        await _redis.SetAsync("WhichRegionIsOff", regions, TimeSpan.FromSeconds(30));
    }

    public async Task CheckAnnomalies()
    {
        var datet = DateTime.Now;
        _logger.LogInformation("Checking EMR anomalies at {Time}", datet);

        string result = "";
        var res = _config.GetSection("EmrIps").Get<List<string>>() ?? throw new ArgumentNullException("EmrIps config section missing");

        foreach (var item in res)
        {
            string emr = await Helper(item);
            if (emr != null)
            {
                result += $"იემერ {item.Split('.').Last()}-ს  აქვს შემდეგ ხარვეზი: {emr}, გადაამოწმე.";
            }
        }

        await _redis.SetAsync("Annomalies", result, TimeSpan.FromSeconds(30));
        _logger.LogInformation("Anomalies found: {Result}", result);
    }

    public async Task CheckSystemHealth()
    {
        var res = _config.GetSection("AlertSettings").Get<List<AlertSetting>>() ?? throw new ArgumentNullException("AlertSettings missing in appsettings");
        var stringfrsent = "";

        foreach (var system in res)
        {
            if (!string.IsNullOrEmpty(system.IPAddress) && !await CheckHealth(system.IPAddress))
            {
                _logger.LogWarning("System health check failed for: {IP}", system.IPAddress);
                stringfrsent += system.Message;
            }
        }

        await _redis.SetAsync("SystemHealth", stringfrsent, TimeSpan.FromSeconds(30));
    }

    public async Task CheckRegionAndSentMessage()
    {
        int count = 0;
        var pingSender = new Ping();

        var regions = new Dictionary<string, (string name, DateTime lastSent, Action<DateTime> updateLastSent)>
        {
            ["192.168.25.10"] = ("Telavi", LastSentTimeTelavi, val => LastSentTimeTelavi = val),
            ["192.168.15.10"] = ("Kutaisi", LastSentTimeKutaisi, val => LastSentTimeKutaisi = val),
            ["192.168.13.10"] = ("Gori", LastSentTimeGori, val => LastSentTimeGori = val),
            ["192.168.14.10"] = ("Foti", LastSentTimeFoti, val => LastSentTimeFoti = val)
        };

        foreach (var region in regions)
        {
            var reply = await pingSender.SendPingAsync(region.Key);
            if (region.Value.lastSent.AddMinutes(30) < DateTime.Now && reply.Status != IPStatus.Success)
            {
                while (reply.Status != IPStatus.Success)
                {
                    _logger.LogWarning("{Region} ping failed, retrying...", region.Value.name);
                    count++;
                    await Task.Delay(TimeSpan.FromSeconds(2));

                    if (count >= 3)
                    {
                        region.Value.updateLastSent(DateTime.Now);
                        var text = $"{region.Value.name} Lost , Check Conection , May Optic Interupt!";
                        await _redis.SetAsync("CheckRegionAndSentMessage", new RegionData { Text = text }, TimeSpan.FromSeconds(30));
                        _logger.LogError("{Region} is offline after 3 retries", region.Value.name);
                        break;
                    }

                    reply = await pingSender.SendPingAsync(region.Key);
                }
            }
        }

        if (count == 0)
        {
            var stringforsent = await RegionEmr.Start();
            if (!string.IsNullOrEmpty(stringforsent))
            {
                stringforsent = "Anomalies Detected: " + stringforsent;
                await _redis.SetAsync("CheckRegionAndSentMessage", new RegionData() { Text = stringforsent }, TimeSpan.FromSeconds(30));
                _logger.LogInformation("EMR anomalies detected and sent.");
            }
        }
    }

    private async Task<bool> CheckHealth(string ip)
    {
        using var ping = new Ping();
        for (int retry = 0; retry < 4; retry++)
        {
            var result = await ping.SendPingAsync(ip);
            if (result.Status == IPStatus.Success)
                return true;

            _logger.LogWarning("Ping {Retry}/4 failed for {IP}", retry + 1, ip);
        }
        return false;
    }

    private async Task<string> Helper(string ip)
    {
        var res = await CheckOut(ip);
        if (res.Any())
        {
            return string.Join("", res);
        }
        return null;
    }

    private async Task<List<string>> CheckOut(string Ip)
    {
        List<string> result = new();
        var random = new Random();
        var mainLInk = $"http://{Ip}/goform/formEMR30?type=8&cmd=1&language=0&slotNo=255&alarmSlotNo=7&ran=0.64850{random.Next()}";

        _logger.LogInformation("Checking EMR at {IP}", Ip);
        var res = await _client.GetAsync(mainLInk);
        if (res.IsSuccessStatusCode)
        {
            var str = await res.Content.ReadAsStringAsync();
            var rek = str.Split(new string[] { "<*1*>", "<html>", "</html>", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var item in rek)
            {
                if (!string.IsNullOrEmpty(item))
                {
                    result.Add(item.Split("(C451E):")[1]);
                }
            }
        }
        else
        {
            _logger.LogWarning("Failed to fetch EMR data from {IP}, Status: {StatusCode}", Ip, res.StatusCode);
        }

        return result.Where(io => !io.ToLower().Contains("port")).ToList();
    }
}
