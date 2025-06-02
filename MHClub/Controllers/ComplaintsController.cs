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
    private readonly MediaService _mediaService;

    public ComplaintsController(ILogger<ComplaintsController> logger, ApplicationDbContext dbContext,
        MediaService mediaService)
    {
        _logger = logger;
        _dbContext = dbContext;
        _mediaService = mediaService;
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