using Microsoft.AspNetCore.Mvc;

namespace MyApp.Web.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return RedirectToAction("Login", "Account");
    }
}