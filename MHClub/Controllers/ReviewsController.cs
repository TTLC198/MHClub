using System.Text.Json;
using MHClub.Domain;
using MHClub.Domain.Models;
using MHClub.Models.Ads;
using MHClub.Models.User;
using MHClub.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MHClub.Controllers;

[Authorize]
[Controller]
[Route("[controller]")]
public class ReviewsController : BaseController
{
    private readonly ILogger<ReviewsController> _logger;
    private readonly ApplicationDbContext _dbContext;

    public ReviewsController(ILogger<ReviewsController> logger, ApplicationDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }
    
    [Authorize]
    [HttpGet]
    [Route("CreateReview")]
    public async Task<IActionResult> CreateReview(int sellerId, string? returnUrl = null)
    {
        try
        {
            ViewBag.ReturnUrl = returnUrl ?? Request.Headers.Referer!;
            ViewBag.Id = sellerId;
            var user = await _dbContext.Users
                .Include(u => u.Medias)
                .FirstOrDefaultAsync(u => u.Id == sellerId);
            if (user is null)
                return NotFound();

            var ads = await _dbContext.Ads
                .Where(a => a.SellerId == user.Id && !a.Status)
                .Include(ad => ad.Medias)
                .ToListAsync();

            ViewBag.Ads = ads.Select(ad => new AdsIndexViewModel(ad)
            {
                Images = ad.Medias.Select(m => m.Path).ToList()
            }).ToList();

            var photo = user.Medias?.FirstOrDefault();

            return View("SelectAdForReview", await GetUserProfileAsync(user, photo?.Path ?? ""));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return BadRequest();
        }
    }

    [Authorize]
    [HttpGet]
    [Route("Create")]
    public async Task<IActionResult> Create(int adId, int sellerId, string? returnUrl = null)
    {
        ViewBag.AdId = adId;
        ViewBag.Id = sellerId;
        ViewBag.ReturnUrl = returnUrl;
        
        var user = await _dbContext.Users
            .Include(u => u.Medias)
            .FirstOrDefaultAsync(u => u.Id == sellerId);
        if (user is null)
            return NotFound();
        
        var photo = user.Medias?.FirstOrDefault();
        
        return View(await GetUserProfileAsync(user, photo?.Path ?? ""));
    }

    [Authorize]
    [HttpPost]
    [Route("Create")]
    public async Task<IActionResult> Create(UserProfileDto model, string? returnUrl = null)
    {
        try
        {
            ViewBag.Id = model.Id ?? 0;
            
            var user = await _dbContext.Users
                .Include(u => u.Medias)
                .FirstOrDefaultAsync(u => u.Id == model.Id);
            if (user is null)
                return NotFound();
        
            var photo = user.Medias?.FirstOrDefault();
            var userProfile = await GetUserProfileAsync(user, photo?.Path ?? "");
            userProfile.ReviewToCreate = model.ReviewToCreate;

            ModelState.Remove("Name");
            ModelState.Remove("Email");
            ModelState.Remove("Phone");
            ModelState.Remove("ImageUrl");
            ModelState.Remove("Password");
            
            if (!ModelState.IsValid)
                return View(await GetUserProfileAsync(user, photo?.Path ?? ""));

            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "id");
            if (userIdClaim is { Value: null } || !int.TryParse(userIdClaim?.Value, out var ownUserId))
                return Unauthorized();
            
            var ownUser = await _dbContext.Users
                .Include(u => u.Medias)
                .FirstOrDefaultAsync(u => u.Id == ownUserId);
            
            if (ownUser is null)
            {
                ModelState.AddModelError(string.Empty, "Пользователь не найден");
                return View(model);
            }

            model.ReviewToCreate.UserId = ownUserId;

            await _dbContext.Reviews.AddAsync(model.ReviewToCreate);
            await _dbContext.SaveChangesAsync();

            return Redirect(returnUrl);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return BadRequest();
        }
    }
    
    private async Task<UserProfileDto> GetUserProfileAsync(User user, string userPhoto)
    {
        var ads = _dbContext.Ads.Where(a => a.SellerId == user.Id);
        var adsCount = await ads.CountAsync();
        var reviewsByAds = _dbContext.Reviews.Join(ads, r => r.AdId, r => r.Id, (r, ad) => r);
        var reviewsCount = await reviewsByAds.CountAsync();
        var ratings = reviewsByAds?.Select(x => x.Estimation);
        double? rating = ratings?.Any() == true ? ratings.Average() : null;
        
        return new UserProfileDto(user, rating, reviewsCount, adsCount, userPhoto);
    }
}