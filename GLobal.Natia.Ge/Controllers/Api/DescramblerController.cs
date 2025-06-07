using Common.Persistance.Interface;
using Microsoft.AspNetCore.Mvc;

namespace GLobal.Natia.Ge.Controllers.Api;

[Route("api/[controller]")]
[ApiController]
public class DescramblerController : ControllerBase
{
    private readonly IEmrServices _emrServices;
    private static readonly object _checkLock = new();
    private static DateTime _lastCheck = DateTime.MinValue;

    public DescramblerController(IEmrServices emrServices)
    {
        _emrServices = emrServices;
    }

    [HttpGet("CardsThatNeedToBeActivated")]
    public async Task<IActionResult> FetchAbnormalCards()
    {
        try
        {
            if (ShouldRun(out DateTime currentTime))
            {
                var emrCodes = new[] { "40", "80" };
                var cardTasks = emrCodes.Select(code => _emrServices.GetCardsWhichNeedToBeActivate(code));
                var results = await Task.WhenAll(cardTasks);

                var cardsToActivate = results
                    .SelectMany(cards => cards)
                    .Select(card =>
                    {
                        var slot = card.Port == "A" ? "ა!" : "ბი";
                        return $"ბარათი გვაქვს გასააქტიურებელი, გადაამოწმე იემერ {card.Emr}, ქარდ {card.Card}, სლოტ {slot}!";
                    })
                    .ToList();

                return Ok(cardsToActivate);
            }

            return Ok(new List<string>());
        }
        catch (Exception ex)
        {
            return StatusCode(500, new List<string>
            {
                $"შეცდომა ბარათების სტატუსის წამოღებისას! {ex.Message}"
            });
        }
    }

    private static bool ShouldRun(out DateTime currentTime)
    {
        currentTime = DateTime.Now;

        lock (_checkLock)
        {
            if ((currentTime - _lastCheck).TotalMinutes >= 9)
            {
                _lastCheck = currentTime;
                return true;
            }
        }

        return false;
    }
}
