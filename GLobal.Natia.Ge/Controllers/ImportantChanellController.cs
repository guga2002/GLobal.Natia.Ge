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
        var url = new Uri("http://192.168.0.13:9999/api/ExcelData/SystemStreamInfo");

        var client = new HttpClient();

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
