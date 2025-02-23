using MHClub.Domain;
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
}