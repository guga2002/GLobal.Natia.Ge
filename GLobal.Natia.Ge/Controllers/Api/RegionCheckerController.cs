using System.Net.NetworkInformation;
using Common.Domain.Interface;
using Microsoft.AspNetCore.Mvc;

namespace GLobal.Natia.Ge.Controllers.Api;

[Route("api/[controller]")]
[ApiController]
public class RegionCheckerController : ControllerBase
{
    private readonly IRedisService _redis;
    private readonly IGetEmrDataServices _region;
    private readonly ILogger<RegionCheckerController> _logger;

    private static DateTime LastSentTimeTelavi = DateTime.MinValue;
    private static DateTime LastSentTimeGori = DateTime.MinValue;
    private static DateTime LastSentTimeKutaisi = DateTime.MinValue;
    private static DateTime LastSentTimeFoti = DateTime.MinValue;

    public RegionCheckerController(IRedisService redis, IGetEmrDataServices region, ILogger<RegionCheckerController> logger)
    {
        _redis = redis;
        _region = region;
        _logger = logger;
    }

    [HttpGet("GetRegionsWhereAlarmIsOn")]
    public async Task<IActionResult> GetRegionsWhereAlarmIsOn()
    {
        _logger.LogInformation("Checking regions where alarms are on (Redis key: WhichRegionIsOff).");
        try
        {
            var regions = await _redis.GetAsync<List<int>>("WhichRegionIsOff") ?? new List<int>();
            _logger.LogInformation("Fetched {Count} region(s) with alarms active.", regions.Count);
            return Ok(regions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch regions with active alarms from Redis.");
            return StatusCode(500, new { error = "დაფიქსირდა შეცდომა რეგიონების წამოღებისას", detail = ex.Message });
        }
    }

    [HttpGet("GetRegionData")]
    public async Task<IActionResult> GetRegionData()
    {
        _logger.LogInformation("Fetching full region data via IGetEmrDataServices.");
        try
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

            List<string> regionResult = new List<string>();

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
                            regionResult.Add(text);
                            _logger.LogError("{Region} is offline after 3 retries", region.Value.name);
                            break;
                        }

                        reply = await pingSender.SendPingAsync(region.Key);
                    }
                }
            }

            if (regionResult.Count > 0)
            {
                _logger.LogInformation("Regions with issues: {Regions}", string.Join(", ", regionResult));
                return Ok(string.Join(", ", regionResult));
            }

            var regionsAlarms = await _region.Start();
            _logger.LogInformation("Successfully fetched {Count} region entries.", regionsAlarms?.Count() ?? 0);
            return Ok(regionsAlarms);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch region data via IGetEmrDataServices.");
            return StatusCode(500, new { error = "დაფიქსირდა შეცდომა რეგიონების წამოღებისას", detail = ex.Message });
        }
    }

    [HttpGet("GetRegionWhereOpticIsOff")]
    public async Task<IActionResult> GetRegionWhichIsOff()
    {
        _logger.LogInformation("Pinging all region IPs to determine optic failures...");
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

        var pingTasks = regionIps.Select(async region =>
        {
            using var pinger = new Ping();
            try
            {
                var pingResult = await pinger.SendPingAsync(region.Key, 1000);
                if (pingResult.Status != IPStatus.Success)
                {
                    regions.Add(region.Value);
                    _logger.LogWarning("Region {Region} (IP: {IP}) did not respond to ping.", region.Value, region.Key);
                }
                else
                {
                    _logger.LogDebug("Region {Region} (IP: {IP}) is online.", region.Value, region.Key);
                }
            }
            catch (Exception ex)
            {
                regions.Add(region.Value);
                _logger.LogError(ex, "Error pinging region {Region} (IP: {IP}). Marking as offline.", region.Value, region.Key);
            }
        }).ToList();

        await Task.WhenAll(pingTasks);

        _logger.LogInformation("Completed region ping check. {Count} region(s) offline.", regions.Count);
        return Ok(regions);
    }
}
