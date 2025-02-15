using System.Security.Claims;
using MHClub.Domain;
using MHClub.Domain.Models;
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
    [Route("Ads")]
    public async Task<IActionResult> Ads()
    {
        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "id");
        if (userIdClaim is { Value: null } || !int.TryParse(userIdClaim?.Value, out var userId))
            return Unauthorized();
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user is null)
            return NotFound();
        
        ViewBag.Ads = await _dbContext.Ads
            .Include(a => a.Medias)
            .Where(a => a.SellerId == user.Id)
            .ToListAsync();
        
        return View(await GetUserProfileAsync(user));
    }
    
    [HttpGet]
    [Route("ArchivedAds")]
    public async Task<IActionResult> ArchivedAds()
    {
        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "id");
        if (userIdClaim is { Value: null } || !int.TryParse(userIdClaim?.Value, out var userId))
            return Unauthorized();
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user is null)
            return NotFound();
        
        ViewBag.ArchivedAds = await _dbContext.Ads.Where(a => a.SellerId == user.Id && !a.Status).ToListAsync();
        
        return View(await GetUserProfileAsync(user));
    }
    
    [HttpGet]
    [Route("Reviews")]
    public async Task<IActionResult> Reviews()
    {
        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "id");
        if (userIdClaim is { Value: null } || !int.TryParse(userIdClaim?.Value, out var userId))
            return Unauthorized();
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user is null)
            return NotFound();
        
        ViewBag.Reviews = await _dbContext.Reviews.Where(r => r.UserId == user.Id).ToListAsync();
        
        return View(await GetUserProfileAsync(user));
    }

    private async Task<UserProfileDto> GetUserProfileAsync(User user)
    {
        var ads = _dbContext.Ads.Where(a => a.SellerId == user.Id);
        var adsCount = await ads.CountAsync();
        var reviewsByAds = _dbContext.Reviews.Join(ads, r => r.AdId, r => r.Id, (r, ad) => r);
        var reviewsCount = await reviewsByAds.CountAsync();
        var ratings = reviewsByAds?.Select(x => x.Estimation);
        double? rating = ratings?.Any() == true ? ratings.Average() : null;
        
        return new UserProfileDto(user, rating, reviewsCount, adsCount);
    }
}