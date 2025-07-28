using Common.Persistance.Interface;
using Microsoft.AspNetCore.Mvc;

namespace GLobal.Natia.Ge.Controllers.Api;

[Route("api/[controller]")]
[ApiController]
public class DescramblerController : ControllerBase
{
    private readonly IEmrServices _emrServices;
    private readonly ILogger<DescramblerController> _logger;

    private static readonly object _checkLock = new();
    private static DateTime _lastCheck = DateTime.MinValue;

    public DescramblerController(IEmrServices emrServices, ILogger<DescramblerController> logger)
    {
        _emrServices = emrServices;
        _logger = logger;
    }

    [HttpGet("CardsThatNeedToBeActivated")]
    public async Task<IActionResult> FetchAbnormalCards()
    {
        try
        {
            _logger.LogDebug("Checking if descrambler card status check should run...");

            if (ShouldRun(out DateTime currentTime))
            {
                _logger.LogInformation("Running descrambler card check at {Time}.", currentTime);

                var emrCodes = new[] { "40", "80" };
                _logger.LogDebug("Fetching cards that need activation for EMR codes: {Codes}", string.Join(",", emrCodes));

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

                if (cardsToActivate.Count > 0)
                {
                    _logger.LogWarning("Found {Count} cards needing activation.", cardsToActivate.Count);
                }
                else
                {
                    _logger.LogInformation("No cards currently need activation.");
                }

                return Ok(cardsToActivate);
            }

            _logger.LogDebug("Skipping descrambler card check (ran too recently).");
            return Ok(new List<string>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error while fetching cards that need activation.");
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
