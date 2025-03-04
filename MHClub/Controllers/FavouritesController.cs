using System.Text.Json;
using MHClub.Domain;
using MHClub.Domain.Models;
using MHClub.Models.Ads;
using MHClub.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MHClub.Controllers;

[Authorize]
[Controller]
[Route("[controller]")]
public class FavouritesController : BaseController
{
    private readonly ILogger<FavouritesController> _logger;
    private readonly ApplicationDbContext _dbContext;
    private readonly MediaService _mediaService;

    public FavouritesController(ILogger<FavouritesController> logger, ApplicationDbContext dbContext,
        MediaService mediaService)
    {
        _logger = logger;
        _dbContext = dbContext;
        _mediaService = mediaService;
    }

    [HttpGet]
    [Route("Index")]
    public async Task<IActionResult> Index()
    {
        try
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "id");
            if (userIdClaim is { Value: null } || !int.TryParse(userIdClaim?.Value, out var userId))
                return Unauthorized();
            var user = await _dbContext.Users
                .Include(u => u.Favourites)!
                .ThenInclude(f => f.Ad)
                .ThenInclude(ad => ad.Medias)
                .FirstOrDefaultAsync(u => u.Id == userId);
            if (user is null)
                return NotFound();

            var ads = user.Favourites?
                .Select(f => f.Ad)
                .ToList();

            ViewBag.Ads = ads.Select(ad => new AdsIndexViewModel(ad)
            {
                Images = ad.Medias.Select(m => m.Path).ToList()
            }).ToList();

            return View(new AdsIndexDto());
        }
        catch (Exception exception)
        {
            return RedirectToAction("Index", "Errors", new { error = exception.Message });
        }
    }

    [Authorize]
    [HttpPost]
    public async Task<string> SetFavourite(int adId, bool isFavourite)
    {
        try
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "id");
            if (userIdClaim is { Value: null } || !int.TryParse(userIdClaim?.Value, out var userId))
                return "";
            var user = await _dbContext.Users
                .Include(u => u.Favourites)!
                .ThenInclude(f => f.Ad)
                .ThenInclude(ad => ad.Medias)
                .FirstOrDefaultAsync(u => u.Id == userId);
            if (user is null)
                return "";

            var ad = await _dbContext.Ads
                .FirstOrDefaultAsync(a => a.Id == adId);

            if (ad is not null)
            {
                if(ad.SellerId == userId)
                    return JsonSerializer.Serialize(false);
                if (isFavourite)
                {
                    var fav = new Favourite()
                    {
                        AdId = ad.Id,
                        Ad = ad,
                        UserId = userId,
                        User = user
                    };
                    await _dbContext.Favourites.AddAsync(fav);
                    await _dbContext.SaveChangesAsync();

                    return JsonSerializer.Serialize(true);
                }
                else
                {
                    var fav = await _dbContext.Favourites.FirstOrDefaultAsync(f =>
                        f.AdId == adId && f.UserId == userId);
                    if (fav is not null)
                    {
                        _dbContext.Favourites.Remove(fav);
                        await _dbContext.SaveChangesAsync();
                        return JsonSerializer.Serialize(false);
                    }
                }
            }
        }
        catch
        {
            return JsonSerializer.Serialize(false);
        }
        return JsonSerializer.Serialize(false);
    }
}