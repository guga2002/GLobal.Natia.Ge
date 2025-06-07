using Common.Domain.Interface;
using Microsoft.AspNetCore.Mvc;

namespace GLobal.Natia.Ge.Controllers.Api;

[Route("api/[controller]")]
[ApiController]
public class CheckAnommaliesOverSystemController : ControllerBase
{
    private readonly IRedisService _redis;
    private static readonly object _anomalyLock = new();
    private static DateTime _lastUpdate = DateTime.MinValue;

    private const string AnomalyKey = "Annomalies";

    public CheckAnommaliesOverSystemController(IRedisService redis)
    {
        _redis = redis;
    }

    [HttpGet("CheckForAnomalies")]
    public async Task<IActionResult> Check()
    {
        var now = DateTime.Now;

        if (ShouldCheck(ref _lastUpdate, now, seconds: 30, _anomalyLock))
        {
            var anomalyMessage = await _redis.GetAsync<string>(AnomalyKey);

            if (!string.IsNullOrEmpty(anomalyMessage))
            {
                return Ok(anomalyMessage);
            }
        }

        return NoContent();
    }

    private static bool ShouldCheck(ref DateTime lastCheck, DateTime now, int seconds, object lockObj)
    {
        lock (lockObj)
        {
            if ((now - lastCheck).TotalSeconds > seconds)
            {
                lastCheck = now;
                return true;
            }
        }

        return false;
    }
}
