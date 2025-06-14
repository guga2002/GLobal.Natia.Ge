using Common.Persistance.Services;
using Microsoft.AspNetCore.Mvc;

namespace GLobal.Natia.Ge.Controllers;

public class ImportantChanellController : Controller
{

    public ImportantChanellController()
    {
    }

    public async Task<IActionResult> Index()
    {
        var url = new Uri("https://192.168.0.79:2000/api/Controll/checkrobot/SystemStreamInfo");

        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) => true
        };
        var client = new HttpClient(handler);

        var response = await client.GetAsync(url);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadAsStringAsync();

            var actres = System.Text.Json.JsonSerializer.Deserialize<List<MulticastAnalysisResult>>(result);
            return View(actres);
        }

        return View(new List<MulticastAnalysisResult>());
    }
}
