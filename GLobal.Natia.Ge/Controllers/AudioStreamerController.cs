using Common.Domain.SignalR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace GLobal.Natia.Ge.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AudioStreamerController : ControllerBase
{

    private readonly IHubContext<UniteMonitoringHub> _hub;

    public AudioStreamerController(IHubContext<UniteMonitoringHub> hub)
    {
        _hub = hub;
    }

    [HttpPost("push")]
    public async Task<IActionResult> PushAudio([FromBody] string base64Wav)
    {
        var audioBytes = Convert.FromBase64String(base64Wav);

        await _hub.Clients.All.SendAsync("robotAudioStream", audioBytes);

        return Ok("Audio pushed to clients.");
    }
}
