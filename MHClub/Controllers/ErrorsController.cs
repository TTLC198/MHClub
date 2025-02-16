using Microsoft.AspNetCore.Mvc;

namespace MHClub.Controllers;

[Controller]
[Route("[controller]")]
public class ErrorsController : BaseController
{
    [HttpGet]
    public IActionResult Index(string error)
    {
        ViewBag.Error = error;
        return View("Error");
    }
}