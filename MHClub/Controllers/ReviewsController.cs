using System.Text.Json;
using MHClub.Domain;
using MHClub.Domain.Models;
using MHClub.Models.Ads;
using MHClub.Models.User;
using MHClub.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MHClub.Controllers;

[Authorize]
[Controller]
[Route("[controller]")]
public class ReviewsController : BaseController
{
    private readonly ILogger<ReviewsController> _logger;
    private readonly ApplicationDbContext _dbContext;

    public ReviewsController(ILogger<ReviewsController> logger, ApplicationDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    [Authorize]
    [HttpPost]
    [Route("Create")]
    public async Task<IActionResult> Create(Review model)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest("Не валидные данные!");

            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "id");
            if (userIdClaim is { Value: null } || !int.TryParse(userIdClaim?.Value, out var ownUserId))
                return Unauthorized();
            
            var ownUser = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Id == ownUserId);
            
            if (ownUser is null)
            {
                return BadRequest("Пользователь не найден");
            }
            
            model.UserId = ownUserId;

            if (_dbContext.Reviews.Any(r => r.UserId == model.UserId && r.AdId == model.AdId))
            {
                return BadRequest("Нельзя создать два отзыва на одно объявление");
            }

            await _dbContext.Reviews.AddAsync(model);
            await _dbContext.SaveChangesAsync();

            return Ok();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return BadRequest();
        }
    }
}