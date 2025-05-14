using MHClub.Domain;
using MHClub.Domain.Models;
using MHClub.Domain.Models.Enums;
using MHClub.Models.Ads;
using MHClub.Models.User;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace MHClub.Controllers;

[Controller]
[Route("[controller]")]
public class AdsController : BaseController
{
    private readonly ILogger<AdsController> _logger;
    private readonly ApplicationDbContext _dbContext;

    public AdsController(ILogger<AdsController> logger, ApplicationDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    [Route("")]
    public async Task<IActionResult> Index()
    {
        try
        {
            return View(new AdsIndexDto());
        }
        catch (Exception exception)
        {
            return RedirectToAction("Index", "Errors", new { error = exception.Message });
        }
    }

    [Route("AdsList")]
    public async Task<IActionResult> AdsList(AdsSearchViewModel model)
    {
        try
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "id");
            if (userIdClaim is { Value: null } || !int.TryParse(userIdClaim?.Value, out var userId))
                userId = 0;
            
            if (Enum.TryParse(typeof(ItemCondition), model.Condition.ToString(), out var condition))
            {
                model.Condition = (ItemCondition)condition;
            }
            
            var ads = await _dbContext.Ads
                .AsNoTracking()
                .Where(a => a.SellerId != userId)
                .Where(a => model.MinPrice == null || a.Cost > model.MinPrice)
                .Where(a => model.MaxPrice == null || a.Cost < model.MaxPrice)
                .Where(a => model.Condition == null || a.ConditionId == (int)(model.Condition ?? ItemCondition.New))
                .Include(a => a.Medias)
                .ToListAsync();

            switch (model.SortBy)
            {
                case "По цене":
                    ads = ads.OrderByDescending(a => a.Cost).ToList();
                    break;
                case "Популярные":
                    ads = ads.OrderByDescending(a => a.Quantity).ToList(); //todo
                    break;
                case "По рейтингу":
                    ads = ads.OrderBy(a => a.Rating).ToList();
                    break;
            }
        
            return PartialView("_AdsPartialView", ads.Select(ad => new AdsIndexViewModel(ad)
            {
                Images = ad.Medias?.Select(m => m.Path).ToList()
            }).ToList());
        }
        catch (Exception exception)
        {
            return RedirectToAction("Index", "Errors", new { error = exception.Message });
        }
    }
    
    [HttpGet]
    [Route("{id}")]
    public async Task<IActionResult> Single(int id, string? returnUrl = null)
    {
        try
        {
            ViewBag.ReturnUrl = returnUrl ?? Request.Headers.Referer!;

            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "id");
            if (userIdClaim is { Value: null } || !int.TryParse(userIdClaim?.Value, out var userId))
                userId = 0;
            
            var ad = await _dbContext.Ads
                .AsNoTracking()
                .Include(a => a.Medias)
                .Include(a => a.Seller)
                .Include(ad => ad.Status)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (ad is null)
                return RedirectToAction("Index", "Errors", new { error = "Объявление не найдено" });
            
            var isFav = await _dbContext.Favourites.AnyAsync(f =>
                f.AdId == ad.Id && f.UserId == userId);
            
            var isOwn = ad.SellerId == userId;

            return View(new AdsIndexViewModel(ad)
            {
                Images = ad.Medias?.Select(m => m.Path).ToList(),
                IsFavourite = isFav,
                IsOwn = isOwn,
                IsArchived = ad.Status?.Id == 2,//todo
                UserProfileDto = await GetUserProfileAsync(ad.Seller!)
            });
        }
        catch (Exception exception)
        {
            return RedirectToAction("Index", "Errors", new { error = exception.Message });
        }
    }
    
    private async Task<UserProfileDto> GetUserProfileAsync(User user)
    {
        var ads = _dbContext.Ads.Where(a => a.SellerId == user.Id);
        var adsCount = await ads.CountAsync();
        var reviewsByAds = _dbContext.Reviews.Join(ads, r => r.AdId, r => r.Id, (r, ad) => r);
        var reviewsCount = await reviewsByAds.CountAsync();
        var ratings = reviewsByAds?.Select(x => x.Estimation);
        double? rating = ratings?.Any() == true ? ratings.Average() : null;
        return new UserProfileDto(user, rating, reviewsCount, adsCount, "");
    }
}