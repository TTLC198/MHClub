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
public class AdsController : BaseController
{
    private readonly ILogger<AdsController> _logger;
    private readonly ApplicationDbContext _dbContext;
    private readonly MediaService _mediaService;
    private readonly RestCountriesService _restCountriesService;

    public AdsController(ILogger<AdsController> logger, ApplicationDbContext dbContext, MediaService mediaService, RestCountriesService restCountriesService)
    {
        _logger = logger;
        _dbContext = dbContext;
        _mediaService = mediaService;
        _restCountriesService = restCountriesService;
    }

    [HttpGet]
    [Route("Index")]
    public async Task<IActionResult> Index()
    {
        var ads = await _dbContext.Ads
            .Include(a => a.Medias)
            .AsNoTracking()
            .ToListAsync();

        ViewBag.Ads = ads.Select(ad => new AdsIndexViewModel(ad)
        {
            Images = ad.Medias.Select(m => m.Path).ToList()
        }).ToList();

        return View(new AdsIndexDto());
    }
    
    [HttpGet]
    [Route("{id}")]
    public async Task<IActionResult> Single(int id, string? returnUrl = null)
    {
        ViewBag.ReturnUrl = returnUrl ?? Request.Headers.Referer!;
        
        var ad = await _dbContext.Ads
            .AsNoTracking()
            .Include(a => a.Medias)
            .FirstOrDefaultAsync(a => a.Id == id);
        
        if (ad is null)
            return RedirectToAction("Index", "Errors", new { error = "Объявление не найдено" });

        return View(new AdsIndexViewModel(ad)
        {
            Images = ad.Medias.Select(m => m.Path).ToList()
        });
    }

    [HttpGet]
    [Route("Create")]
    public async Task<IActionResult> Create(string? returnUrl = null)
    {
        ViewBag.ReturnUrl = returnUrl ?? Request.Headers.Referer!;
        ViewBag.CountriesSelect = await _restCountriesService.GetAllForSelect();

        return View(new AdsCreateViewModel());
    }
    
    [HttpPost]
    [Route("Create")]
    public async Task<IActionResult> Create(AdsCreateViewModel model, string? returnUrl = null)
    {
        ViewBag.ReturnUrl = returnUrl ?? Request.Headers.Referer!;

        return View();
    }
}