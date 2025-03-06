using System.Security.Claims;
using MHClub.Domain;
using MHClub.Domain.Models;
using MHClub.Models;
using MHClub.Models.Ads;
using MHClub.Models.User;
using MHClub.Services;
using MHClub.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace MHClub.Controllers;

[AllowAnonymous]
[Controller]
[Route("[controller]")]
public class ProfileController : BaseController
{
    private readonly ILogger<AuthController> _logger;
    private readonly ApplicationDbContext _dbContext;
    private readonly MediaService _mediaService;
    private readonly PasswordHasher<User> _passwordHasher;

    public ProfileController(ILogger<AuthController> logger, ApplicationDbContext dbContext, MediaService mediaService, PasswordHasher<User> passwordHasher)
    {
        _logger = logger;
        _dbContext = dbContext;
        _mediaService = mediaService;
        _passwordHasher = passwordHasher;
    }
    
    [Authorize]
    [HttpGet]
    [Route("Edit")]
    public async Task<IActionResult> Edit(string? returnUrl = null)
    {
        ViewBag.ReturnUrl = returnUrl ?? Request.Headers.Referer!;
        
        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "id");
        if (userIdClaim is { Value: null } || !int.TryParse(userIdClaim?.Value, out var userId))
            return Unauthorized();
        ViewBag.Id = userId;
        var user = await _dbContext.Users
            .Include(u => u.Medias)
            .FirstOrDefaultAsync(u => u.Id == userId);
        if (user is null)
            return NotFound();

        var userPhoto = user.Medias?.FirstOrDefault();

        var editedUser = new UserEditDto(user, userPhoto?.Path ?? "");
        
        return View(editedUser);
    }

    [Authorize]
    [HttpPost]
    [Route("Edit")]
    public async Task<IActionResult> Edit([FromForm]UserEditDto inputUser, string? returnUrl = null)
    {
        try
        {
            ViewBag.ReturnUrl = returnUrl ?? Request.Headers.Referer!;

            var passwordEditMode = !string.IsNullOrEmpty(inputUser.OldPassword) ||
                                   !string.IsNullOrEmpty(inputUser.Password) ||
                                   !string.IsNullOrEmpty(inputUser.RepeatPassword);

            if (!passwordEditMode)
            {
                ModelState.Remove(nameof(UserEditDto.OldPassword));
                ModelState.Remove(nameof(UserEditDto.Password));
                ModelState.Remove(nameof(UserEditDto.RepeatPassword));
            }

            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "id");
            if (userIdClaim is { Value: null } || !int.TryParse(userIdClaim?.Value, out var userId))
                return Unauthorized();
            ViewBag.Id = userId;
            var user = await _dbContext.Users
                .Include(u => u.Medias)
                .FirstOrDefaultAsync(u => u.Id == userId);
            if (user is null)
            {
                ModelState.AddModelError(string.Empty, "Пользователь с введенными данными не найден");
                return View(inputUser);
            }

            var userPhoto = user.Medias?.FirstOrDefault();
            inputUser.ImageUrl = userPhoto?.Path ?? "";

            if (ModelState.IsValid)
            {
                return View(inputUser);
            }

            if (passwordEditMode)
            {
                var result = _passwordHasher.VerifyHashedPassword(user, user.Password!, inputUser.OldPassword!);
                if (result == PasswordVerificationResult.Failed)
                {
                    ModelState.AddModelError(nameof(UserEditDto.OldPassword), "Введен неверный пароль");
                    return View(inputUser);
                }

                if (!inputUser.IsPasswordEquals)
                {
                    ModelState.AddModelError(nameof(UserEditDto.RepeatPassword), "Пароли должны совпадать");
                    return View(inputUser);
                }

                user.Password = _passwordHasher.HashPassword(inputUser, inputUser.Password!);
            }

            if (inputUser.Avatar is not null)
            {
                var oldImage = user.Medias?.FirstOrDefault();
                if (oldImage is not null)
                    await _mediaService.Delete(oldImage.Id);
                var mediaCreateDto = new MediaCreateDto()
                {
                    UserId = user.Id,
                    Image = inputUser.Avatar
                };
                var uploadResult = await _mediaService.UploadImage(mediaCreateDto);
                if (!uploadResult.Item1)
                    return RedirectToAction("Index", "Errors", new { error = uploadResult.Item2 });
            }

            user.Email = inputUser.Email;
            user.Phone = inputUser.Phone;
            user.Name = inputUser.Name;

            await _dbContext.SaveChangesAsync();

            return RedirectToAction("OwnProfile");
        }
        catch (Exception exception)
        {
            return RedirectToAction("Index", "Errors", new { error = exception.Message });
        }
    }

    [Authorize]
    [HttpGet]
    [Route("OwnProfile")]
    public IActionResult OwnProfile(string? returnUrl = null)
    {
        ViewBag.ReturnUrl = returnUrl ?? Request.Headers.Referer!;
        
        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "id");
        if (userIdClaim is { Value: null } || !int.TryParse(userIdClaim?.Value, out var userId))
            return Unauthorized();
        ViewBag.Id = userId;
        return RedirectToAction("Profile", "Profile", new { userId });
    }

    [HttpGet]
    [Route("{userId:int}/")]
    public async Task<IActionResult> Profile(int userId, string? returnUrl = null)
    {
        try
        {
            ViewBag.ReturnUrl = returnUrl ?? Request.Headers.Referer!;

            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "id");
            if (userIdClaim is { Value: null } || !int.TryParse(userIdClaim?.Value, out var userIdByClaim))
                userIdByClaim = 0;
            if (userIdByClaim != 0)
                ViewBag.IsAuth = true;
            ViewBag.Id = userId;
            var user = await _dbContext.Users
                .Include(u => u.Medias)
                .FirstOrDefaultAsync(u => u.Id == userId);
            if (user is null)
                return NotFound();

            if (userIdByClaim == userId)
                ViewBag.IsOwn = true;

            var ads = await _dbContext.Ads
                .Include(a => a.Medias)
                .Where(a => a.SellerId == user.Id && a.Status)
                .ToListAsync();

            ViewBag.Ads = ads.Select(ad => new AdsIndexViewModel(ad)
            {
                Images = ad.Medias.Select(m => m.Path).ToList()
            }).ToList();
            
            var photo = user.Medias?.FirstOrDefault();

            ViewBag.Reviews = await _dbContext.Ads
                .Include(a => a.Reviews)
                .Where(a => a.SellerId == user.Id)
                .SelectMany(a => a.Reviews)
                .ToListAsync();

            return View(await GetUserProfileAsync(user, photo?.Path ?? ""));
        }
        catch (Exception exception)
        {
            return RedirectToAction("Index", "Errors", new { error = exception.Message });
        }
    }
    
    [HttpGet]
    [Route("{userId:int}/ArchivedAds")]
    public async Task<IActionResult> ArchivedAds(int userId, string? returnUrl = null)
    {
        try
        {
            ViewBag.ReturnUrl = returnUrl ?? Request.Headers.Referer!;
            ViewBag.Id = userId;
            var user = await _dbContext.Users
                .Include(u => u.Medias)
                .FirstOrDefaultAsync(u => u.Id == userId);
            if (user is null)
                return NotFound();
            
            ViewBag.IsArchivedAds = true;

            var ads = await _dbContext.Ads
                .Where(a => a.SellerId == user.Id && !a.Status)
                .Include(ad => ad.Medias)
                .ToListAsync();

            ViewBag.Ads = ads.Select(ad => new AdsIndexViewModel(ad)
            {
                Images = ad.Medias.Select(m => m.Path).ToList()
            }).ToList();

            var photo = user.Medias?.FirstOrDefault();

            return View("Ads", await GetUserProfileAsync(user, photo?.Path ?? ""));
        }
        catch (Exception exception)
        {
            return RedirectToAction("Index", "Errors", new { error = exception.Message });
        }
    }
    
    [HttpGet]
    [Route("{userId:int}/Reviews")]
    public async Task<IActionResult> Reviews(int userId, string? returnUrl = null)
    {
        try
        {
            ViewBag.ReturnUrl = returnUrl ?? Request.Headers.Referer!;

            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "id");
            if (userIdClaim is { Value: null } || !int.TryParse(userIdClaim?.Value, out var userIdByClaim))
                userIdByClaim = 0;
            if (userIdByClaim != 0)
                ViewBag.IsAuth = true;
            ViewBag.Id = userId;
            var user = await _dbContext.Users
                .Include(u => u.Medias)
                .FirstOrDefaultAsync(u => u.Id == userId);
            if (user is null)
                return NotFound();
            
            if (userIdByClaim == userId)
                ViewBag.IsOwn = true;

            var ads = _dbContext.Ads
                .Include(a => a.Reviews)
                .Where(a => a.SellerId == user.Id);

            ViewBag.Reviews = await ads.SelectMany(a => a.Reviews).ToListAsync();

            var photo = user.Medias?.FirstOrDefault();

            return View(await GetUserProfileAsync(user, photo?.Path ?? ""));
        }
        catch (Exception exception)
        {
            return RedirectToAction("Index", "Errors", new { error = exception.Message });
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