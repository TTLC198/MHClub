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

        var ads = _dbContext.Ads.Where(a => a.SellerId == userId);
        var adsCount = await ads.CountAsync();
        var reviewsByAds = _dbContext.Reviews.Join(ads, r => r.AdId, r => r.Id, (r, ad) => r);
        var reviewsCount = await reviewsByAds.CountAsync();
        var ratings = reviewsByAds?.Select(x => x.Estimation);
        double? rating = ratings?.Any() == true ? ratings.Average() : null;
        
        var userProfileDto = new UserProfileDto(user, rating, reviewsCount, adsCount);
        return View(userProfileDto);
    }
}