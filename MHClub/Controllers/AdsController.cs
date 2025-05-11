using MHClub.Domain;
using MHClub.Models.Ads;
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
            
            var ads = await _dbContext.Ads
                .AsNoTracking()
                .Where(a => a.SellerId != userId)
                .Where(a => a.Cost > model.MinPrice && a.Cost < model.MaxPrice)
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
}