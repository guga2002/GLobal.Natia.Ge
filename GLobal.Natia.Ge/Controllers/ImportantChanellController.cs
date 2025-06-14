using Common.Domain.Interface;
using Common.Domain.Services;
using Common.Persistance.Services;
using Microsoft.AspNetCore.Mvc;

namespace GLobal.Natia.Ge.Controllers;

public class ImportantChanellController : Controller
{
    private readonly IRedisService redisService;

    public ImportantChanellController(IRedisService redisService)
    {
        this.redisService= redisService;
    }

    public async Task<IActionResult> Index()
    {
        var redis = await redisService.GetAsync<List<MulticastAnalysisResult>>("SystemStreamInfo");

        foreach (var item in redis)
        {
            await Console.Out.WriteLineAsync(item.Ip);
        }
        return View(redis);
    }
}
