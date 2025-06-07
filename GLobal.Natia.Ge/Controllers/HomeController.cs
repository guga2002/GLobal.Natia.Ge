using Microsoft.AspNetCore.Mvc;

namespace GLobal.Natia.Ge.Controllers;

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