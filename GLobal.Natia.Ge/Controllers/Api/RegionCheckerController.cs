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

    public RegionCheckerController(IRedisService redis, IGetEmrDataServices region)
    {
        _redis = redis;
        _region = region;
    }

    [HttpGet("GetRegionsWhereAlarmIsOn")]
    public async Task<IActionResult> GetRegionsWhereAlarmIsOn()
    {
        try
        {
            var regions = await _redis.GetAsync<List<int>>("WhichRegionIsOff") ?? new List<int>();
            return Ok(regions);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "დაფიქსირდა შეცდომა რეგიონების წამოღებისას", detail = ex.Message });
        }
    }

    [HttpGet("GetRegionData")]
    public async Task<IActionResult> GetRegionData()
    {
        try
        {
            var regions = await _region.Start();
            return Ok(regions);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "დაფიქსირდა შეცდომა რეგიონების წამოღებისას", detail = ex.Message });
        }
    }

    [HttpGet("GetRegionWhereOpticIsOff")]
    public async Task<IActionResult> GetRegionWhichIsOff()
    {
        List<int> regions = new List<int>();

        var regionIps = new Dictionary<string, int>
        {
            { "192.168.14.10", 370 },
            { "192.168.25.10", 372 },
            { "192.168.15.10", 371 },
            { "192.168.13.10", 373 }
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
                }
            }
            catch (Exception ex)
            {
                await Console.Out.WriteLineAsync(ex.Message);
            }
        }).ToList();

        await Task.WhenAll(pingTasks);

        return Ok(regions);
    }
}
