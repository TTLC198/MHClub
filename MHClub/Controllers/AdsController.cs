using System.Security.Claims;
using MHClub.Domain;
using MHClub.Domain.Models;
using MHClub.Domain.Models.Enums;
using MHClub.Models;
using MHClub.Models.Ads;
using MHClub.Models.User;
using MHClub.Services;
using MHClub.Utils;
using Microsoft.AspNetCore.Authorization;
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
  private readonly RestCountriesService _restCountriesService;
  private readonly MediaService _mediaService;

  public AdsController(ILogger<AdsController> logger, ApplicationDbContext dbContext,
    RestCountriesService restCountriesService, MediaService mediaService)
  {
    _logger = logger;
    _dbContext = dbContext;
    _restCountriesService = restCountriesService;
    _mediaService = mediaService;
  }

  [Route("")]
  public async Task<IActionResult> Index(string Search = null!)
  {
    try
    {
      var model = new AdsIndexDto
      {
        AdsSearchViewModel =
        {
          SearchText = Search
        }
      };
      ViewBag.SearchText = Search;
      return View(model);
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

      model.SearchText = model.SearchText?.ToLower().Trim();

      var ads = await _dbContext.Ads
        .AsNoTracking()
        .Where(a => a.StatusId == (int)StatusType.Default)
        .Where(a => a.SellerId != userId)
        .Where(a => model.MinPrice == null || a.Cost > model.MinPrice)
        .Where(a => model.MaxPrice == null || a.Cost < model.MaxPrice)
        .Where(a => model.Condition == null || a.ConditionId == (int)(model.Condition ?? ItemCondition.New))
        .Where(a => model.SearchText == null || a.Name.ToLower().Contains(model.SearchText) ||
                    a.Description != null && a.Description.ToLower().Contains(model.SearchText))
        .Where(a => (a.ParentAdId == null && model.Type == ItemType.Main) ||
                    (a.ParentAdId != null && model.Type == ItemType.Decomposed) || model.Type == null)
        .Include(a => a.Medias)
        .ToListAsync();

      switch (model.SortBy)
      {
        case "Цена по возрастанию":
          ads = ads.OrderBy(a => a.Cost).ToList();
          break;
        case "Цена по убыванию":
          ads = ads.OrderByDescending(a => a.Cost).ToList();
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
        .ThenInclude(s => s!.Medias)
        .Include(a => a.Condition)
        .Include(ad => ad.Status)
        .Include(ad => ad.ChildrenAds)!
        .ThenInclude(ad => ad.Medias)
        .Include(ad => ad.ParentAd)!
        .ThenInclude(ads => ads.Medias)
        .FirstOrDefaultAsync(a => a.Id == id);

      if (ad is null)
        return RedirectToAction("Index", "Errors", new { error = "Объявление не найдено" });

      var isFav = await _dbContext.Favourites.AnyAsync(f =>
        f.AdId == ad.Id && f.UserId == userId);

      var isOwn = ad.SellerId == userId;

      var childrenAds = ad.ChildrenAds?
        .Where(a => a.StatusId == 1 || a.StatusId == (int)StatusType.Hidden)
        .Select(a => new AdsIndexViewModel(a)
        {
          Images = a.Medias?.Select(m => m.Path).ToList(),
        }).ToList();

      return View(new AdsIndexViewModel(ad)
      {
        Images = ad.Medias?.Select(m => m.Path).ToList(),
        IsFavourite = isFav,
        IsOwn = isOwn,
        IsArchived = ad.Status?.Id == 2, //todo
        IsDeleted = ad.Status?.Id == 4, //todo
        UserProfileDto = await GetUserProfileAsync(ad.Seller!),
        ChildrenAds = childrenAds,
        ParentAd = ad.ParentAd == null
          ? null
          : new AdsIndexViewModel(ad.ParentAd)
          {
            Images = ad.ParentAd.Medias?.Select(m => m.Path).ToList(),
          }
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
  public async Task<IActionResult> Create(string? returnUrl = null, int parentAdId = -1)
  {
    var model = new AdsCreateViewModel();
    try
    {
      ViewBag.ReturnUrl = returnUrl ?? Request.Headers.Referer!;
      model.CountriesSelect = await _restCountriesService.GetAllForSelect();
      var conditions = await _dbContext.Conditions.ToListAsync();
      ViewBag.Conditions = conditions.Select(c => new SelectListItem(c.Name, c.Id.ToString())).ToList();
      var categories = await _dbContext.Categories.ToListAsync();
      ViewBag.AllCategories = categories.ToList();
      if (parentAdId != -1)
        ViewBag.ParentAdId = parentAdId;
    }
    catch (Exception exception)
    {
      ModelState.AddModelError(string.Empty, exception.Message);
    }

    return View("SelectCategory");
  }

  [Authorize]
  [HttpPost]
  [Route("CreateWithCategories")]
  public async Task<IActionResult> CreateWithCategories([FromForm] int categoryId, string? returnUrl = null,
    int parentAdId = -1)
  {
    if (categoryId == 0)
      return RedirectToAction("Create", "Ads", new { returnUrl });
    var model = new AdsCreateViewModel();
    try
    {
      ViewBag.ReturnUrl = returnUrl ?? Request.Headers.Referer!;
      model.CountriesSelect = await _restCountriesService.GetAllForSelect();
      var conditions = await _dbContext.Conditions.ToListAsync();
      ViewBag.Conditions = conditions.Select(c => new SelectListItem(c.Name, c.Id.ToString())).ToList();
      var selectedCategory = await _dbContext.Categories
        .Include(c => c.ParentCategory)
        .ThenInclude(cc => cc!.ParentCategory)
        .FirstOrDefaultAsync(c => c.Id == categoryId);
      model.Category = selectedCategory;
      model.CategoryId = selectedCategory?.Id ?? 0;
      if (parentAdId != -1)
        ViewBag.ParentAdId = parentAdId;
    }
    catch (Exception exception)
    {
      ModelState.AddModelError(string.Empty, exception.Message);
    }

    return View("Create", model);
  }

  [Authorize]
  [HttpPost]
  [Route("Create")]
  public async Task<IActionResult> Create([FromForm] AdsCreateViewModel model, string? returnUrl = null)
  {
    try
    {
      ViewBag.ReturnUrl = returnUrl ?? Request.Headers.Referer!;
      model.CountriesSelect = await _restCountriesService.GetAllForSelect();
      var conditions = await _dbContext.Conditions.ToListAsync();
      ViewBag.Conditions = conditions.Select(c => new SelectListItem(c.Name, c.Id.ToString())).ToList();
      var selectedCategory = await _dbContext.Categories.FirstOrDefaultAsync(c => c.Id == model.CategoryId);
      model.Category = selectedCategory;
      if (model.ParentAdId != -1)
        ViewBag.ParentAdId = model.ParentAdId;

      model = model.TrimStringProperties();

      if (!ModelState.IsValid)
        return View(model);

      var existedAd = await _dbContext.Ads.FirstOrDefaultAsync(u => u.Name == model.Name && u.StatusId == (int)StatusType.Default);
      if (existedAd is not null)
      {
        ModelState.AddModelError(string.Empty, "Объявление с подобным именем уже существует!");
        return View(model);
      }

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

      var ad = new Ad()
      {
        Cost = model.Cost,
        CategoryId = model.CategoryId,
        ConditionId = model.ConditionId,
        Quantity = model.Quantity,
        Name = model.Name,
        ManufactureCountry = model.ManufactureCountry,
        Description = model.Description,
        SellerId = userId,
        CreationDate = DateTime.Now,
        StatusId = 1,
        ParentAdId = model.ParentAdId,
      };

      var adEntry = await _dbContext.Ads.AddAsync(ad);
      await _dbContext.SaveChangesAsync();

      if (model.IsMainAd)
      {
        var hiddenAd = new Ad()
        {
          Cost = model.Cost,
          CategoryId = model.CategoryId,
          ConditionId = model.ConditionId,
          Quantity = model.Quantity,
          Name = model.Name,
          ManufactureCountry = model.ManufactureCountry,
          Description = model.Description,
          SellerId = userId,
          CreationDate = DateTime.Now,
          StatusId = (int)StatusType.Hidden,
          ParentAdId = adEntry.Entity.Id,
        };
        await _dbContext.Ads.AddAsync(hiddenAd);
        await _dbContext.SaveChangesAsync();
      }
      
      foreach (var mediaCreateDto in model.Images.Select(imageFile => new MediaCreateDto()
               {
                 AdId = adEntry.Entity.Id,
                 Image = imageFile
               }))
      {
        var uploadResult = await _mediaService.UploadImage(mediaCreateDto);
      }

      ViewBag.Success = true;
      return View(model);
    }
    catch (Exception exception)
    {
      ModelState.AddModelError(string.Empty, "Произошла системная ошибка!");
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
      
      if (_dbContext.Ads.Any(a => a.Name == ad.Name || a.StatusId == (int)StatusType.Default))
        return RedirectToAction("Index", "Errors", new { error = "Объявление не может иметь такое же название, как у другого объявления. Сначала измените имя объявления" });

      ad.StatusId = 1;
      await _dbContext.SaveChangesAsync();

      return Redirect(ViewBag.ReturnUrl);
    }
    catch (Exception exception)
    {
      return RedirectToAction("Index", "Errors", new { error = exception.Message });
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

      ad.Category = _dbContext.Categories
        .Include(c => c!.ParentCategory)
        .ThenInclude(cc => cc!.ParentCategory)
        .FirstOrDefault(c => c.Id == ad.CategoryId);

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
  [Route("Edit/{id}")]
  public async Task<IActionResult> Edit(int id, [FromForm] AdsCreateViewModel model, string? returnUrl = null)
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

      model = model.TrimStringProperties();
      
      var existedAd = await _dbContext.Ads.FirstOrDefaultAsync(u => u.Name == model.Name && u.StatusId == (int)StatusType.Default);
      if (existedAd is not null)
      {
        ModelState.AddModelError(string.Empty, "Объявление с подобным именем уже существует!");
        return View(model);
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

      if (adEntry is null)
      {
        ModelState.AddModelError(string.Empty, "Объявление не найдено");
        return View(model);
      }

      adEntry.Name = model.Name;
      adEntry.Cost = model.Cost;
      adEntry.ManufactureCountry = model.ManufactureCountry;
      adEntry.Quantity = model.Quantity;
      adEntry.Description = model.Description;
      adEntry.CategoryId = model.CategoryId;
      adEntry.ConditionId = model.ConditionId;

      foreach (var mediaCreateDto in model.Images.Select(imageFile => new MediaCreateDto()
               {
                 AdId = adEntry.Id,
                 Image = imageFile
               }))
      {
        var uploadResult = await _mediaService.UploadImage(mediaCreateDto);
      }

      await _dbContext.SaveChangesAsync();

      ViewBag.Success = true;
      return View(model);
    }
    catch (Exception exception)
    {
      ModelState.AddModelError(string.Empty, exception.Message);
      return View(model);
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
      var userRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value ?? "User";

      if (!isOwn && userRole != "Admin")
        return RedirectToAction("Index", "Errors", new { error = "Вы не можете удалить чужое объявление" });

      ad.StatusId = userRole == "Admin" ? 4 : 2;

      await _dbContext.SaveChangesAsync();

      return Redirect(ViewBag.ReturnUrl);
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

    var photo = user.Medias?.FirstOrDefault();

    return new UserProfileDto(user, rating, reviewsCount, adsCount, photo?.Path ?? "");
  }
}