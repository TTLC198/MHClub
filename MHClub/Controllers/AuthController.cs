using System.Diagnostics;
using System.Security.Claims;
using MHClub.Domain;
using MHClub.Domain.Models;
using Microsoft.AspNetCore.Mvc;
using MHClub.Models;
using MHClub.Models.User;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;

namespace MHClub.Controllers;

[Controller]
[Route("[controller]")]
public class AuthController : BaseController
{
    private readonly ILogger<AuthController> _logger;
    private readonly ApplicationDbContext _dbContext;
    private readonly PasswordHasher<User> _passwordHasher;

    public AuthController(ILogger<AuthController> logger, ApplicationDbContext dbContext, PasswordHasher<User> passwordHasher)
    {
        _logger = logger;
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
    }
    
    [HttpGet]
    [AllowAnonymous]
    [Route("Login")]
    public IActionResult Login()
    {
        return View();
    }
    
    [HttpPost]
    [AllowAnonymous]
    [Route("Login")]
    public async Task<IActionResult> Login(User inputUser, string returnUrl = "/")
    {
        ModelState.Remove(nameof(Domain.Models.User.Role));
        ModelState.Remove(nameof(Domain.Models.User.RoleId));
        ModelState.Remove(nameof(Domain.Models.User.Id));
        ModelState.Remove(nameof(Domain.Models.User.Name));
        ModelState.Remove(nameof(Domain.Models.User.Phone));
        ModelState.Remove(nameof(Domain.Models.User.DateOfRegistration));
        if (!ModelState.IsValid)
        {
            return View(inputUser);
        }
        
        var user = await _dbContext.Users
            .Include(user => user.Role)
            .FirstOrDefaultAsync(u => u.Email == inputUser.Email);

        if (user is null)
        {
            ModelState.AddModelError(string.Empty, "Пользователь с введенными данными не найден");
            return View(inputUser);
        }

        var result = _passwordHasher.VerifyHashedPassword(user, user.Password, inputUser.Password!);

        if (result == PasswordVerificationResult.Failed)
        {
            ModelState.AddModelError(string.Empty, "Пользователь с введенными данными не найден");
            return View(inputUser);
        }

        var authClaims = new List<Claim>
        {
            new("id", Strings.Trim($"{user.Id}")),
            new(ClaimTypes.Role, Strings.Trim(user.Role?.Name))
        };
        var claimsIdentity = new ClaimsIdentity(authClaims, "Cookies");
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity));
        return Redirect(returnUrl ?? "/");
    }
    
    [AllowAnonymous]
    [Route("Registration")]
    [HttpGet]
    public IActionResult Registration()
    {
        return View();
    }
    
    [AllowAnonymous]
    [Route("Registration")]
    [HttpPost]
    public async Task<IActionResult> Registration(UserRegisterDto inputUser, string returnUrl = "/")
    {
        try
        {
            if (!inputUser.IsPasswordEquals)
                ModelState.AddModelError(nameof(UserRegisterDto.RepeatPassword), "Пароли должны совпадать");

            if (!ModelState.IsValid)
                return View(inputUser);

            var existedUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == inputUser.Email);
            if (existedUser is not null)
            {
                ModelState.AddModelError(string.Empty, "Введенная почта уже используется");
                return View(inputUser);
            }

            var hasher = new PasswordHasher<User>();

            var user = new User()
            {
                Id = null,
                Name = inputUser.Name?.Trim(),
                Phone = inputUser.Phone?.Trim(),
                Email = inputUser.Email?.Trim(),
                Password = _passwordHasher.HashPassword(inputUser, inputUser.Password!).Trim(),
                DateOfRegistration = DateOnly.FromDateTime(DateTime.Now),
                RoleId = 2, //User
            };

            await _dbContext.Users.AddAsync(user);

            switch (await _dbContext.SaveChangesAsync())
            {
                case 0:
                    ModelState.AddModelError(string.Empty, "Произошла ошибка");
                    return View(inputUser);
                default:
                    return Redirect(returnUrl ?? "/");
            }
        }
        catch (Exception exception)
        {
            ModelState.AddModelError(string.Empty, "Произошла ошибка");
            return View(inputUser);
        }
    }
    
    [HttpGet]
    [Authorize]
    [Route("Logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Redirect("/");
    }
    
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}