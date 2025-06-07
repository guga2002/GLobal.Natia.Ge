using Microsoft.AspNetCore.Mvc;

namespace GlobalTv.Natia.ge.Controllers;

public class HomeController : Controller
{
    public HomeController()
    {
    }
    public IActionResult Index()
    {
        return RedirectToAction("Index", "Unite");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View();
    }
}