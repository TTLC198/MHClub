using System.Text.Json;
using MHClub.Domain;
using MHClub.Domain.Models;
using MHClub.Models.Ads;
using MHClub.Models.Complaints;
using MHClub.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MHClub.Controllers;

[Authorize]
[Controller]
[Route("[controller]")]
public class ComplaintsController : BaseController
{
    private readonly ILogger<ComplaintsController> _logger;
    private readonly ApplicationDbContext _dbContext;

    public ComplaintsController(ILogger<ComplaintsController> logger, ApplicationDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }
    
    [Authorize]
    [HttpGet]
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

            var complaints = _dbContext.Complaints
                .Include(c => c.User)
                .Include(c => c.Ad)
                .ToList();

            return View(complaints);
        }
        catch (Exception exception)
        {
            return RedirectToAction("Index", "Errors", new { error = exception.Message });
        }
    }
    
    [Authorize]
    [HttpPost]
    [Route("[action]")]
    public async Task<string> SolveComplaint(int complaintId, bool deleteAd)
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

            var complaint = await _dbContext.Complaints
                .FirstOrDefaultAsync(a => a.Id == complaintId);

            if (complaint is not null)
            {
                if (deleteAd)
                {
                   var ad = await _dbContext.Ads.FirstOrDefaultAsync(a => a.Id == complaint.AdId);
                   if (ad is not null)
                       ad.StatusId = 4;
                   foreach (var childrenAd in ad.ChildrenAds?.ToList() ?? [])
                   {
                       childrenAd.StatusId = 4;
                   }
                }
                _dbContext.Complaints.Remove(complaint);
                await _dbContext.SaveChangesAsync();
                return JsonSerializer.Serialize(true);
            }
        }
        catch
        {
            return JsonSerializer.Serialize(false);
        }
        return JsonSerializer.Serialize(false);
    }

    [Authorize]
    [HttpPost]
    public async Task<string> Create([FromBody]CreateComplaintDto complaintDto)
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
                .FirstOrDefaultAsync(a => a.Id == complaintDto.AdId);

            if (ad is not null)
            {
                if(ad.SellerId == userId)
                    return JsonSerializer.Serialize(false);
                
                var complaint = new Complaint()
                {
                    AdId = ad.Id,
                    Ad = ad,
                    UserId = userId,
                    User = user,
                    Description = complaintDto.Description.Trim(),
                };
                await _dbContext.Complaints.AddAsync(complaint);
                await _dbContext.SaveChangesAsync();

                return JsonSerializer.Serialize(true);
            }
        }
        catch
        {
            return JsonSerializer.Serialize(false);
        }
        return JsonSerializer.Serialize(false);
    }
}