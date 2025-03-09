using System.Text.Json;
using MHClub.Domain;
using MHClub.Domain.Models;
using MHClub.Models;
using MHClub.Models.Ads;
using MHClub.Models.User;
using MHClub.Services;
using MHClub.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace MHClub.Controllers;

[AllowAnonymous]
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
    [Route("")]
    [Route("/")]
    [Route("Index")]
    public async Task<IActionResult> Index()
    {
        try
        {
            var model = new AdsIndexDto
            {
                AdsSearchViewModel =
                {
                    Categories = _dbContext.Categories
                        .AsNoTracking()
                        .ToList()
                        .Where(x => !string.IsNullOrEmpty(x.Name))
                        .Select(x => new SelectListItem(x.Name, x.Id.ToString()))
                        .ToList()
                }
            };

            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "id");
            if (userIdClaim is { Value: null } || !int.TryParse(userIdClaim?.Value, out var userId))
                userId = 0;
            
            var ads = await _dbContext.Ads
                .Where(a => a.SellerId != userId && a.Status)
                .Include(a => a.Medias)
                .AsNoTracking()
                .ToListAsync();

            ViewBag.Ads = ads.Select(ad => new AdsIndexViewModel(ad)
            {
                Images = ad.Medias.Select(m => m.Path).ToList()
            }).ToList();

            return View(model);
        }
        catch (Exception exception)
        {
            return RedirectToAction("Index", "Errors", new { error = exception.Message });
        }
    }
    
    [HttpGet]
    [Route("Search")]
    public async Task<IActionResult> Search(AdsIndexDto model)
    {
        try
        {
            model.AdsSearchViewModel.Categories = _dbContext.Categories
                .AsNoTracking()
                .ToList()
                .Where(x => !string.IsNullOrEmpty(x.Name))
                .Select(x => new SelectListItem(x.Name, x.Id.ToString()))
                .ToList();
            
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "id");
            if (userIdClaim is { Value: null } || !int.TryParse(userIdClaim?.Value, out var userId))
                userId = 0;
            
            var ads = await _dbContext.Ads
                .AsNoTracking()
                .Where(a => a.SellerId != userId && a.Status)
                .Include(a => a.Medias)
                .ToListAsync();

            if (!string.IsNullOrEmpty(model.AdsSearchViewModel.FilterText))
            {
                ads = ads
                    .Where(a => (a.Description != null && a.Description.Contains(model.AdsSearchViewModel.FilterText, StringComparison.CurrentCultureIgnoreCase)) ||
                                 a.Name.Contains(model.AdsSearchViewModel.FilterText, StringComparison.CurrentCultureIgnoreCase)).ToList();
            }

            if (model.AdsSearchViewModel.SelectedCategoryId is not null)
            {
                ads = ads.Where(a => a.CategoryId == model.AdsSearchViewModel.SelectedCategoryId).ToList();
            }

            ViewBag.Ads = ads.Select(ad => new AdsIndexViewModel(ad)
            {
                Images = ad.Medias.Select(m => m.Path).ToList()
            }).ToList();

            ViewBag.IsAfterSearch = true;

            return View("Index", model);
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
                IsArchived = !ad.Status,
                UserProfileDto = await GetUserProfileAsync(ad.Seller!)
            });
        }
        catch (Exception exception)
        {
            return RedirectToAction("Index", "Errors", new { error = exception.Message });
        }
    }

    [Authorize]
    [HttpGet]
    [Route("Create")]
    public async Task<IActionResult> Create(string? returnUrl = null)
    {
        var model = new AdsCreateViewModel();
        try
        {
            ViewBag.ReturnUrl = returnUrl ?? Request.Headers.Referer!;
            model.CountriesSelect = await _restCountriesService.GetAllForSelect();
            model.CategoriesSelect = _dbContext.Categories
                .AsNoTracking()
                .ToList()
                .Where(x => !string.IsNullOrEmpty(x.Name))
                .Select(x => new SelectListItem(x.Name, x.Id.ToString()))
                .ToList();
            var conditions = await _dbContext.Conditions.ToListAsync();
            ViewBag.Conditions = conditions.Select(c => new SelectListItem(c.Name, c.Id.ToString())).ToList();
            var tariffs = await _dbContext.Tariffs.ToListAsync();
            ViewBag.Tariffs = tariffs.Select(c => new SelectListItem(c.Name, c.Id.ToString())).ToList();
        }
        catch (Exception exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
        }
        return View(model);
    }
    
    [Authorize]
    [HttpPost]
    [Route("Edit/{id}")]
    public async Task<IActionResult> Edit(int id, [FromForm]AdsCreateViewModel model, string? returnUrl = null)
    {
        try
        {
            model.Id = id;
            ViewBag.ReturnUrl = returnUrl ?? Request.Headers.Referer!;
            model.CountriesSelect = await _restCountriesService.GetAllForSelect();
            model.CategoriesSelect = _dbContext.Categories
                .AsNoTracking()
                .ToList()
                .Where(x => !string.IsNullOrEmpty(x.Name))
                .Select(x => new SelectListItem(x.Name, x.Id.ToString()))
                .ToList();
            var conditions = await _dbContext.Conditions.ToListAsync();
            ViewBag.Conditions = conditions.Select(c => new SelectListItem(c.Name, c.Id.ToString())).ToList();
            var tariffs = await _dbContext.Tariffs.ToListAsync();
            ViewBag.Tariffs = tariffs.Select(c => new SelectListItem(c.Name, c.Id.ToString())).ToList();

            model = model.TrimStringProperties();
            
            if (model.OurService)
            {
                ModelState.Remove("TariffId");
            }
            else
            {
                ModelState.Remove("CostDelivery");
                ModelState.Remove("CustomDuty1");
                ModelState.Remove("CustomDuty2");
            }
            
            if (!ModelState.IsValid)
                return View(model);

            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "id");
            if (userIdClaim is { Value: null } || !int.TryParse(userIdClaim?.Value, out var userId))
                return Unauthorized();
            var user = await _dbContext.Users
                .Include(u => u.Medias)
                .FirstOrDefaultAsync(u => u.Id == userId);
            if (user is null)
            {
                ModelState.AddModelError(string.Empty, "Пользователь не найден");
                return View(model);
            }
            var adEntry = await _dbContext.Ads.FirstOrDefaultAsync(x => x.Id == model.Id);
            
            adEntry.TotalCost = model.TotalCost;
            adEntry.Name = model.Name;
            adEntry.Cost = model.Cost;
            adEntry.ManufactureCountry = model.ManufactureCountry;
            adEntry.Quantity = model.Quantity;
            adEntry.Description = model.Description;
            adEntry.StartCountry = model.StartCountry;
            adEntry.CountryOfIntermediateArrival = model.CountryOfIntermediateArrival;
            adEntry.CountryOfDelivery = model.CountryOfDelivery;
            adEntry.Height = model.Height;
            adEntry.Width = model.Width;
            adEntry.Length = model.Length;
            adEntry.Weight = model.Weight;
            adEntry.Volume = model.Volume;
            adEntry.SellerMargin = model.SellerMargin;
            adEntry.OurService = model.OurService;
            adEntry.CostOfDelivery = model.CostOfDelivery;
            adEntry.CustomsDuty1 = model.CustomsDuty1;
            adEntry.CustomsDuty2 = model.CustomsDuty2;
            adEntry.BankCommission = model.BankCommission;
            adEntry.CategoryId = model.CategoryId;
            adEntry.TariffId = model.TariffId;
            adEntry.ConditionId = model.ConditionId;
            adEntry.CreationDate = model.CreationDate;
            
            await _dbContext.SaveChangesAsync();

            return RedirectToAction("Single", new { id = model.Id});
        }
        catch (Exception exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            return View(model);
        }
    }
    
    [Authorize]
    [HttpGet]
    [Route("Edit/{id}")]
    public async Task<IActionResult> Edit(int id, string? returnUrl = null)
    {
        var model = new AdsCreateViewModel();
        try
        {
            ViewBag.ReturnUrl = returnUrl ?? Request.Headers.Referer!;
            model.CountriesSelect = await _restCountriesService.GetAllForSelect();
            model.CategoriesSelect = _dbContext.Categories
                .AsNoTracking()
                .ToList()
                .Where(x => !string.IsNullOrEmpty(x.Name))
                .Select(x => new SelectListItem(x.Name, x.Id.ToString()))
                .ToList();
            var conditions = await _dbContext.Conditions.ToListAsync();
            ViewBag.Conditions = conditions.Select(c => new SelectListItem(c.Name, c.Id.ToString())).ToList();
            var tariffs = await _dbContext.Tariffs.ToListAsync();
            ViewBag.Tariffs = tariffs.Select(c => new SelectListItem(c.Name, c.Id.ToString())).ToList();
            
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "id");
            if (userIdClaim is { Value: null } || !int.TryParse(userIdClaim?.Value, out var userId))
                return Unauthorized();
            
            var ad = await _dbContext.Ads
                .AsNoTracking()
                .Include(a => a.Medias)
                .Include(a => a.Seller)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (ad is null)
                return RedirectToAction("Index", "Errors", new { error = "Объявление не найдено" });
            
            var isOwn = ad.SellerId == userId;
            
            if (!isOwn)
                return RedirectToAction("Index", "Errors", new { error = "Вы не можете редактировать чужое объявление" });

            return View(new AdsCreateViewModel(ad)
            {
                IsOwn = isOwn,
                CountriesSelect = model.CountriesSelect,
                CategoriesSelect = model.CategoriesSelect,
                UserProfileDto = await GetUserProfileAsync(ad.Seller!)
            });
        }
        catch (Exception exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
        }
        return View(model);
    }
    
    [Authorize]
    [HttpPost]
    [Route("Create")]
    public async Task<IActionResult> Create([FromForm]AdsCreateViewModel model, string? returnUrl = null)
    {
        try
        {
            ViewBag.ReturnUrl = returnUrl ?? Request.Headers.Referer!;
            model.CountriesSelect = await _restCountriesService.GetAllForSelect();
            model.CategoriesSelect = _dbContext.Categories
                .AsNoTracking()
                .ToList()
                .Where(x => !string.IsNullOrEmpty(x.Name))
                .Select(x => new SelectListItem(x.Name, x.Id.ToString()))
                .ToList();
            var conditions = await _dbContext.Conditions.ToListAsync();
            ViewBag.Conditions = conditions.Select(c => new SelectListItem(c.Name, c.Id.ToString())).ToList();
            var tariffs = await _dbContext.Tariffs.ToListAsync();
            ViewBag.Tariffs = tariffs.Select(c => new SelectListItem(c.Name, c.Id.ToString())).ToList();
            
            model = model.TrimStringProperties();

            if (model.OurService)
            {
                ModelState.Remove("TariffId");
            }
            else
            {
                ModelState.Remove("CostDelivery");
                ModelState.Remove("CustomDuty1");
                ModelState.Remove("CustomDuty2");
            }
            
            if (!ModelState.IsValid)
                return View(model);

            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "id");
            if (userIdClaim is { Value: null } || !int.TryParse(userIdClaim?.Value, out var userId))
                return Unauthorized();
            var user = await _dbContext.Users
                .Include(u => u.Medias)
                .FirstOrDefaultAsync(u => u.Id == userId);
            if (user is null)
            {
                ModelState.AddModelError(string.Empty, "Пользователь не найден");
                return View(model);
            }

            var ad = new Ad(model);
            
            ad.SellerId = userId;
            ad.CreationDate = DateTime.Now;
            ad.Status = true;
            var adEntry = await _dbContext.Ads.AddAsync(ad);
            await _dbContext.SaveChangesAsync();

            foreach (var imageFile in model.Images)
            {
                var mediaCreateDto = new MediaCreateDto()
                {
                    AdId = adEntry.Entity.Id,
                    Image = imageFile
                };
                var uploadResult = await _mediaService.UploadImage(mediaCreateDto);
            }

            return RedirectToAction("OwnProfile", "Profile");
        }
        catch (Exception exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            return View(model);
        }
    }
    
    [Authorize]
    [HttpPost]
    [Route("{id}/Restore")]
    public async Task<IActionResult> Restore(int id, string? returnUrl = null)
    {
        try
        {
            ViewBag.ReturnUrl = returnUrl ?? Request.Headers.Referer!;

            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "id");
            if (userIdClaim is { Value: null } || !int.TryParse(userIdClaim?.Value, out var userId))
                return Unauthorized();
            
            var ad = await _dbContext.Ads
                .Include(a => a.Medias)
                .Include(a => a.Seller)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (ad is null)
                return RedirectToAction("Index", "Errors", new { error = "Объявление не найдено" });
            
            var isOwn = ad.SellerId == userId;
            
            if (!isOwn)
                return RedirectToAction("Index", "Errors", new { error = "Вы не можете восстановить чужое объявление" });

            ad.Status = true;
            await _dbContext.SaveChangesAsync();
            
            return Redirect(ViewBag.ReturnUrl);
        }
        catch (Exception exception)
        {
            return RedirectToAction("Index", "Errors", new { error = exception.Message });
        }
    }
    
    [Authorize]
    [HttpPost]
    [Route("{id}/Delete")]
    public async Task<IActionResult> Delete(int id, string? returnUrl = null)
    {
        try
        {
            ViewBag.ReturnUrl = returnUrl ?? Request.Headers.Referer!;

            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "id");
            if (userIdClaim is { Value: null } || !int.TryParse(userIdClaim?.Value, out var userId))
                return Unauthorized();
            
            var ad = await _dbContext.Ads
                .Include(a => a.Medias)
                .Include(a => a.Seller)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (ad is null)
                return RedirectToAction("Index", "Errors", new { error = "Объявление не найдено" });
            
            var isOwn = ad.SellerId == userId;
            
            if (!isOwn)
                return RedirectToAction("Index", "Errors", new { error = "Вы не можете удалить чужое объявление" });

            ad.Status = false;
            await _dbContext.SaveChangesAsync();
            
            return Redirect(ViewBag.ReturnUrl);
        }
        catch (Exception exception)
        {
            return RedirectToAction("Index", "Errors", new { error = exception.Message });
        }
    }

    [HttpPost]
    [Route("Calculate")]
    public async Task<string> CalculatePrice([FromForm]AdsCreateViewModel model)
    {
        var totalPrice = 0d;

        try
        {

            totalPrice += model.Cost * (model.Quantity ?? 1);
            var sellerMargin = totalPrice / 100d * (model.SellerMargin ?? 0);
            var bankCommission = totalPrice / 100d * (model.BankCommission ?? 0);
            totalPrice += sellerMargin + bankCommission;

            if (!model.OurService)
            {
                if (model.TariffId.HasValue)
                {
                    var currentTariff = await _dbContext.Tariffs.AsNoTracking()
                        .FirstOrDefaultAsync(x => x.Id == model.TariffId);
                    var deliveryCost =
                        (model.Weight ?? 0) * (model.Quantity ?? 1) + (model.Volume ?? 0) * (model.Quantity ?? 1) *
                        (currentTariff?.Cost ?? 10);
                    totalPrice += deliveryCost;
                }
            }
            else
            {
                totalPrice += (model.CustomsDuty1 ?? 0) + (model.CustomsDuty2 ?? 0) + (model.CostOfDelivery ?? 0);
            }

            return JsonSerializer.Serialize(totalPrice);
        }
        catch (Exception e)
        {
            return JsonSerializer.Serialize(0);
        }
    }
    
    private async Task<UserProfileDto> GetUserProfileAsync(User user)
    {
        var ads = _dbContext.Ads.Where(a => a.SellerId == user.Id && a.Status);
        var adsCount = await ads.CountAsync();
        var reviewsByAds = _dbContext.Reviews.Join(ads, r => r.AdId, r => r.Id, (r, ad) => r);
        var reviewsCount = await reviewsByAds.CountAsync();
        var ratings = reviewsByAds?.Select(x => x.Estimation);
        double? rating = ratings?.Any() == true ? ratings.Average() : null;
        
        return new UserProfileDto(user, rating, reviewsCount, adsCount, "");
    }
}