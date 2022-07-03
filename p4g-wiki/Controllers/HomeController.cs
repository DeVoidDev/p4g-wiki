using Microsoft.AspNetCore.Mvc;

namespace P4GWiki.Controllers;

[Route("/")]
public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View("Index");
    }
}