using Common.Persistance.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace GLobal.Natia.Ge.Controllers;

public class ImportantChanellController : Controller
{

    public ImportantChanellController()
    {
    }

    public async Task<IActionResult> Index()
    {
        var handler = new HttpClientHandler()
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) => true
        };

        var url = new Uri("http://192.168.0.13:9999/api/ExcelData/SystemStreamInfo");

        using var client = new HttpClient(handler);

        var response = await client.GetAsync(url);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadAsStringAsync();

            var actres = JsonConvert.DeserializeObject<List<MulticastAnalysisResult>>(result);
            return View(actres);
        }

        return View(new List<MulticastAnalysisResult>());
    }
}
