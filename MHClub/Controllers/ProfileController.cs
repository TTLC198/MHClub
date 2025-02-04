using MHClub.Domain;
using MHClub.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace MHClub.Controllers;

[Authorize]
[Controller]
[Route("[controller]")]
public class ProfileController : Controller
{
    private readonly ILogger<AuthController> _logger;
    private readonly ApplicationDbContext _dbContext;

    public ProfileController(ILogger<AuthController> logger, ApplicationDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }
    
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "id");
        if (userIdClaim is { Value: null } || !int.TryParse(userIdClaim?.Value, out var userId))
            return Unauthorized();
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user is null)
            return NotFound();

        var ads = _dbContext.Ads.Where(a => a.)
        var rating = _dbContext.Reviews.Where(r => r.)
        
        var userProfileDto = new UserProfileDto(user);
        return View(userProfileDto);
    }
}