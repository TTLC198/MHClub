using MHClub.Domain;
using MHClub.Domain.Models;
using MHClub.Models.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MHClub.Controllers;

[Authorize]
[Controller]
[Route("[controller]")]
public class ManagementController : BaseController
{
  private readonly ILogger<ManagementController> _logger;
  private readonly ApplicationDbContext _dbContext;
  private readonly PasswordHasher<User> _passwordHasher;

  public ManagementController(ILogger<ManagementController> logger, ApplicationDbContext dbContext,
    PasswordHasher<User> passwordHasher)
  {
    _logger = logger;
    _dbContext = dbContext;
    _passwordHasher = passwordHasher;
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

      var admins = await _dbContext.Users
        .Where(u => u.RoleId == 1)
        .ToListAsync();

      return View(admins);
    }
    catch (Exception exception)
    {
      return RedirectToAction("Index", "Errors", new { error = exception.Message });
    }
  }

  [HttpGet("Create")]
  public IActionResult Create()
  {
    ViewData["Title"] = "Регистрация нового админа";
    return View(); // отобразит Views/Management/Create.cshtml
  }

  // POST: /Management/Create
  [HttpPost("Create")]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> Create(UserRegisterDto inputUser)
  {
    if (!inputUser.IsPasswordEquals)
      ModelState.AddModelError(
        nameof(UserRegisterDto.RepeatPassword),
        "Пароли должны совпадать");

    if (!ModelState.IsValid)
      return View(inputUser);

    var existed = await _dbContext.Users
      .AnyAsync(u => u.Email == inputUser.Email);
    if (existed)
    {
      ModelState.AddModelError(
        string.Empty,
        "Введенная почта уже используется");
      return View(inputUser);
    }

    var admin = new User
    {
      Name = inputUser.Name?.Trim(),
      Phone = inputUser.Phone?.Trim(),
      Email = inputUser.Email?.Trim(),
      Password = _passwordHasher
        .HashPassword(inputUser, inputUser.Password!)!
        .Trim(),
      DateOfRegistration = DateOnly.FromDateTime(DateTime.Now),
      RoleId = 1 // админ
    };

    await _dbContext.Users.AddAsync(admin);
    await _dbContext.SaveChangesAsync();

    return RedirectToAction(nameof(Index));
  }

  [HttpPost("Delete")]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> Delete(int id)
  {
    try
    {
      var admin = await _dbContext.Users.FindAsync(id);
      if (admin == null) return NotFound();

      _dbContext.Users.Remove(admin);
      await _dbContext.SaveChangesAsync();
      return RedirectToAction(nameof(Index));
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Ошибка при удалении админа");
      return RedirectToAction("Index", "Errors", new { error = ex.Message });
    }
  }
}