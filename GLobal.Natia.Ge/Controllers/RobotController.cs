using Common.Domain.Interface;
using Common.Domain.SignalR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace GLobal.Natia.Ge.Controllers;

[ApiController]
[Route("[controller]")]
public class RobotController : ControllerBase
{
    private readonly IRedisService _redisService;
    private readonly IHubContext<UniteMonitoringHub> _hubContext;
    private readonly ILogger<RobotController> _logger;

    private const string _sharedMessage = "ShearedKeyForRobot";

    public RobotController(IRedisService redisService, IHubContext<UniteMonitoringHub> hubContext, ILogger<RobotController> logger)
    {
        _redisService = redisService;
        _hubContext = hubContext;
        _logger = logger;
    }

    [HttpGet("GetRobotMessage")]
    public JsonResult GetRobotMessage()
    {
        _logger.LogInformation("Fetching current robot message from Redis (key: {Key})...", _sharedMessage);

        var currentMessage = Task.Run(() => _redisService.GetAsync<string>(_sharedMessage))
                                  .GetAwaiter().GetResult() ?? "";

        if (currentMessage == _sharedMessage || string.IsNullOrEmpty(currentMessage))
        {
            _logger.LogWarning("Robot message is empty or matches default key. Setting fallback value for 5 minutes.");
            Task.Run(() => _redisService.SetAsync(_sharedMessage, "Empty", TimeSpan.FromMinutes(5)))
                .GetAwaiter().GetResult();
        }
        else
        {
            _logger.LogInformation("Robot message retrieved: {Message}", currentMessage);
        }

        return new JsonResult(new { message = currentMessage });
    }

    [HttpGet("start")]
    public async Task<IActionResult> Start([FromQuery] string sentence)
    {
        _logger.LogInformation("Starting new robot broadcast. Requested sentence: {Sentence}", sentence);

        var currentMessage = await _redisService.GetAsync<string>(_sharedMessage) ?? "";

        if (currentMessage == sentence)
        {
            _logger.LogInformation("New sentence matches existing one. No update broadcasted.");
            return Ok("Same message already exists. No update performed.");
        }

        await _redisService.SetAsync(_sharedMessage, sentence, TimeSpan.FromMinutes(5));
        _logger.LogInformation("Stored new robot message in Redis (expires in 5 minutes). Broadcasting...");

        await _hubContext.Clients.All.SendAsync("robotSay", sentence);
        _logger.LogInformation("Robot broadcast sent successfully.");

        return Ok("New message recorded and broadcasted.");
    }

    [HttpGet]
    public async Task<string?> GetNow()
    {
        _logger.LogInformation("Fetching and clearing current robot message (key: {Key})...", _sharedMessage);

        var res = await _redisService.GetAsync<string>(_sharedMessage);
        if (!string.IsNullOrEmpty(res))
        {
            _logger.LogInformation("Message retrieved: {Message}. Deleting key from Redis.", res);
            await _redisService.DeleteAsync(_sharedMessage);
        }
        else
        {
            _logger.LogInformation("No current message found in Redis to fetch or delete.");
        }
        return res;
    }
}
