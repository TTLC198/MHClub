using Microsoft.AspNetCore.Mvc;

namespace MHClub.Controllers;

[Controller]
[Route("[controller]")]
public class ErrorsController : Controller
{
    [HttpGet]
    public IActionResult Index(string error)
    {
        ViewBag.Error = error;
        return View("Error");
    }
}