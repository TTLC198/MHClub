using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MHClub.Controllers;

[AllowAnonymous]
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