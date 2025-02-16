using MHClub.Domain;
using Microsoft.AspNetCore.Mvc;

namespace MHClub.Controllers;

[Controller]
[Route("[controller]")]
public class HomeController : BaseController
{
    private readonly ILogger<AuthController> _logger;
    private readonly ApplicationDbContext _dbContext;

    public HomeController(ILogger<AuthController> logger, ApplicationDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    [Route("/")]
    public IActionResult Index()
    {
        return View("Index");
    }
}