using MHClub.Domain;
using Microsoft.AspNetCore.Mvc;

namespace MHClub.Controllers;

[Controller]
[Route("[controller]")]
public class HomeController : BaseController
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _dbContext;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    [Route("/")]
    public IActionResult Index()
    {
        //return View("Index");
        return RedirectToAction("Index", "Ads");
    }
}