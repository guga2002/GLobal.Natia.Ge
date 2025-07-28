using Common.Domain.Interface;
using Microsoft.AspNetCore.Mvc;

namespace GLobal.Natia.Ge.Controllers.Api;

[Route("api/[controller]")]
[ApiController]
public class CheckAnommaliesOverSystemController : ControllerBase
{
    private readonly IRedisService _redis;
    private readonly ILogger<CheckAnommaliesOverSystemController> _logger;

    private const string AnomalyKey = "Annomalies";
    private const string LastShownKey = "Annomalies:LastShown";

    public CheckAnommaliesOverSystemController(IRedisService redis, ILogger<CheckAnommaliesOverSystemController> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    [HttpGet("CheckForAnomalies")]
    public async Task<IActionResult> Check()
    {
        _logger.LogDebug("🔍 Checking for system anomalies in Redis...");

        var anomalyMessage = await _redis.GetAsync<string>(AnomalyKey);
        if (string.IsNullOrEmpty(anomalyMessage))
        {
            _logger.LogInformation("No anomalies found.");
            return NoContent();
        }

        var lastShown = await _redis.GetAsync<DateTime?>(LastShownKey);
        var now = DateTime.UtcNow;

        if (lastShown.HasValue && now - lastShown.Value < TimeSpan.FromMinutes(10))
        {
            _logger.LogInformation("Anomaly suppressed (last shown {LastShown}, within 10 minutes).", lastShown);
            return NoContent();
        }

        await _redis.SetAsync(LastShownKey, now, TimeSpan.FromHours(1));
        await _redis.DeleteAsync(AnomalyKey);

        _logger.LogWarning("⚠ Anomaly detected and returned to client: {Message}", anomalyMessage);

        return Ok(anomalyMessage);
    }
}
