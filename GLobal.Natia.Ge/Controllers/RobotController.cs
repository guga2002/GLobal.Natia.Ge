using Common.Domain.SignalR;
using Common.Persistance.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace GlobalManagment.Controllers;

[ApiController]
[Route("[controller]")]
public class RobotController : ControllerBase
{
    private readonly IRedisService _redisService;
    private readonly IHubContext<UniteMonitoringHub> _hubContext;
    private const string _sharedMessage = "ShearedKeyForRobot";

    public RobotController(IRedisService redisService, IHubContext<UniteMonitoringHub> hubContext)
    {
        _redisService = redisService;
        _hubContext = hubContext;
    }

    [HttpGet("GetRobotMessage")]
    public JsonResult GetRobotMessage()
    {
        var currentMessage = Task.Run(() => _redisService.GetAsync<string>(_sharedMessage))
                                  .GetAwaiter().GetResult() ?? "";

        if (currentMessage == _sharedMessage|| string.IsNullOrEmpty(currentMessage))
        {
            Task.Run(() => _redisService.SetAsync(_sharedMessage, "Empty", TimeSpan.FromMinutes(5)))
                .GetAwaiter().GetResult();
        }

        return new JsonResult(new { message = currentMessage });
    }


    [HttpGet("start")]
    public async Task<IActionResult> Start([FromQuery] string sentence)
    {
        var currentMessage = await _redisService.GetAsync<string>(_sharedMessage) ?? "";

        if (currentMessage == sentence)
        {
            return Ok("Same message already exists. No update performed.");
        }

        await _redisService.SetAsync(_sharedMessage, sentence, TimeSpan.FromMinutes(5));

        await _hubContext.Clients.All.SendAsync("robotSay", sentence);

        return Ok("New message recorded and broadcasted.");
    }


    [HttpGet]
    public async Task<string?> GetNow()
    {
        var res=await _redisService.GetAsync<string>(_sharedMessage);
        if(!string.IsNullOrEmpty(res))
        {
            await _redisService.DeleteAsync(_sharedMessage);
        }
        return res;
    }
}
