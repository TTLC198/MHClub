using System.Security.Claims;
using MHClub.Domain;
using MHClub.Domain.Models;
using MHClub.Models;
using MHClub.Models.User;
using MHClub.Services;
using MHClub.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace MHClub.Controllers;

[Authorize]
[Controller]
[Route("[controller]")]
public class ProfileController : BaseController
{
    private readonly ILogger<AuthController> _logger;
    private readonly ApplicationDbContext _dbContext;
    private readonly MediaService _mediaService;

    public ProfileController(ILogger<AuthController> logger, ApplicationDbContext dbContext, MediaService mediaService)
    {
        _logger = logger;
        _dbContext = dbContext;
        _mediaService = mediaService;
    }
    
    [HttpGet]
    [Route("Edit")]
    public async Task<IActionResult> Edit(string? returnUrl = null)
    {
        ViewBag.ReturnUrl = returnUrl ?? Request.Headers.Referer!;
        
        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "id");
        if (userIdClaim is { Value: null } || !int.TryParse(userIdClaim?.Value, out var userId))
            return Unauthorized();
        var user = await _dbContext.Users
            .Include(u => u.Medias)
            .FirstOrDefaultAsync(u => u.Id == userId);
        if (user is null)
            return NotFound();

        var userPhoto = user.Medias?.FirstOrDefault();

        var editedUser = new UserEditDto(user, userPhoto?.Path ?? "");
        
        return View(editedUser);
    }

    [HttpPost]
    [Route("Edit")]
    public async Task<IActionResult> Edit([FromForm]UserEditDto inputUser, string? returnUrl = null)
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
        
        if (ModelState.IsValid)
        {
            return View(inputUser);
        }
        
        var user = await _dbContext.Users
            .Include(user => user.Role)
            .Include(user => user.Medias)
            .FirstOrDefaultAsync(u => u.Email == inputUser.Email);

        if (user is null)
        {
            ModelState.AddModelError(string.Empty, "Пользователь с введенными данными не найден");
            return View(inputUser);
        }

        if (passwordEditMode)
        {
            var hasher = new PasswordHasher<User>();
            var result = hasher.VerifyHashedPassword(user, user.Password!, inputUser.Password!);
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
            
            user.Password = hasher.HashPassword(user, inputUser.Password!);
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
        
        return RedirectToAction("Ads");
    }
    
    [HttpGet]
    [Route("Ads")]
    public async Task<IActionResult> Ads(string? returnUrl = null)
    {
        ViewBag.ReturnUrl = returnUrl ?? Request.Headers.Referer!;
        
        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "id");
        if (userIdClaim is { Value: null } || !int.TryParse(userIdClaim?.Value, out var userId))
            return Unauthorized();
        var user = await _dbContext.Users
            .Include(u => u.Medias)
            .FirstOrDefaultAsync(u => u.Id == userId);
        if (user is null)
            return NotFound();
        
        ViewBag.Ads = await _dbContext.Ads
            .Include(a => a.Medias)
            .Where(a => a.SellerId == user.Id)
            .ToListAsync();
        
        var photo = user.Medias?.FirstOrDefault();
        
        return View(await GetUserProfileAsync(user, photo?.Path ?? ""));
    }
    
    [HttpGet]
    [Route("ArchivedAds")]
    public async Task<IActionResult> ArchivedAds(string? returnUrl = null)
    {
        ViewBag.ReturnUrl = returnUrl ?? Request.Headers.Referer!;
        
        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "id");
        if (userIdClaim is { Value: null } || !int.TryParse(userIdClaim?.Value, out var userId))
            return Unauthorized();
        var user = await _dbContext.Users
            .Include(u => u.Medias)
            .FirstOrDefaultAsync(u => u.Id == userId);
        if (user is null)
            return NotFound();
        
        ViewBag.ArchivedAds = await _dbContext.Ads.Where(a => a.SellerId == user.Id && !a.Status).ToListAsync();
        
        var photo = user.Medias?.FirstOrDefault();
        
        return View(await GetUserProfileAsync(user, photo?.Path ?? ""));
    }
    
    [HttpGet]
    [Route("Reviews")]
    public async Task<IActionResult> Reviews(string? returnUrl = null)
    {
        ViewBag.ReturnUrl = returnUrl ?? Request.Headers.Referer!;
        
        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "id");
        if (userIdClaim is { Value: null } || !int.TryParse(userIdClaim?.Value, out var userId))
            return Unauthorized();
        var user = await _dbContext.Users
            .Include(u => u.Medias)
            .FirstOrDefaultAsync(u => u.Id == userId);
        if (user is null)
            return NotFound();
        
        var ads = _dbContext.Ads
            .Include(a => a.Reviews)
            .Where(a => a.SellerId == user.Id);
                 
        ViewBag.Reviews = await ads.SelectMany(a => a.Reviews).ToListAsync();
        
        var photo = user.Medias?.FirstOrDefault();
        
        return View(await GetUserProfileAsync(user, photo?.Path ?? ""));
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