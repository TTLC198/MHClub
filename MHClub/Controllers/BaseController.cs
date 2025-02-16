using Microsoft.AspNetCore.Mvc;

namespace MHClub.Controllers;

public class BaseController : Controller
{
    public string GetCurrentUrl() =>
        HttpContext.Request.Path.ToString();
}