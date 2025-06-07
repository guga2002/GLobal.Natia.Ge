using Common.Domain.Interface;
using Microsoft.AspNetCore.Mvc;

namespace GLobal.Natia.Ge.Controllers.Api;

[Route("api/[controller]")]
[ApiController]
public class CheckAnommaliesOverSystemController : ControllerBase
{
    private readonly IRedisService _redis;

    private const string AnomalyKey = "Annomalies";
    private const string LastShownKey = "Annomalies:LastShown";

    public CheckAnommaliesOverSystemController(IRedisService redis)
    {
        _redis = redis;
    }

    [HttpGet("CheckForAnomalies")]
    public async Task<IActionResult> Check()
    {
        var anomalyMessage = await _redis.GetAsync<string>(AnomalyKey);
        if (string.IsNullOrEmpty(anomalyMessage))
            return NoContent();

        var lastShown = await _redis.GetAsync<DateTime?>(LastShownKey);
        var now = DateTime.UtcNow;

        if (lastShown.HasValue && now - lastShown.Value < TimeSpan.FromMinutes(10))
            return NoContent();

        await _redis.SetAsync(LastShownKey, now, TimeSpan.FromHours(1));
        await _redis.DeleteAsync(AnomalyKey);
        return Ok(anomalyMessage);
    }
}
