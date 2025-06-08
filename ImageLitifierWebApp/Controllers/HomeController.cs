using Microsoft.AspNetCore.Mvc;

namespace ImageLitifier.WebApp.Controllers;
public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
