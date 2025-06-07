using Common.Domain.Interface;
using Microsoft.AspNetCore.Mvc;

namespace GLobal.Natia.Ge.Controllers.Api;

[Route("api/[controller]")]
[ApiController]
public class RegionCheckerController : ControllerBase
{
    private readonly IRedisService _redis;

    public RegionCheckerController(IRedisService redis)
    {
        _redis = redis;
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
}
