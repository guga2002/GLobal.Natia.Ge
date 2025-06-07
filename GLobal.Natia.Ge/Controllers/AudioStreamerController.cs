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
    public async Task<IActionResult> PushAudio([FromBody] byte[] wavBytes)
    {
        if (wavBytes == null || wavBytes.Length < 100)
            return BadRequest("Invalid audio stream");

        var base64Audio = Convert.ToBase64String(wavBytes);

        await _hub.Clients.All.SendAsync("robotAudioStream", base64Audio);

        return Ok("Audio pushed to clients.");
    }
}
