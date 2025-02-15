using System.Net;
using System.Net.Mime;
using System.Web;
using MHClub.Domain;
using MHClub.Domain.Models;
using MHClub.Models;
using MHClub.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MHClub.Controllers;

[Controller]
[Route("Media")]
public class MediaController : Controller
{
    private readonly ILogger<MediaController> _logger;
    private readonly IWebHostEnvironment _environment;
    private readonly ApplicationDbContext _dbContext;

    public MediaController(ILogger<MediaController> logger, ApplicationDbContext dbContext, IWebHostEnvironment environment)
    {
        _logger = logger;
        _dbContext = dbContext;
        _environment = environment;
    }
    
    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        if (id <= 0)
            return RedirectToAction("Index", "Errors", new { error = "Id must be greater than 0" });
        
        _logger.LogDebug("Get image with id = {Id}", id);

        var image = await _dbContext.Media.FirstOrDefaultAsync(i => i.Id == id);
        
        if (image is null)
            return RedirectToAction("Index", "Errors", new { error = "Не найдено" });
        
        var imageData = await System.IO.File.ReadAllBytesAsync(image.Path);

        return File(imageData, image.ContentType!);
    }
    
    [AllowAnonymous]
    [HttpGet("{*path}")]
    public async Task<IActionResult> Get([FromRoute]string path)
    {
        if (path is null or {Length: 0})
            return RedirectToAction("Index", "Errors", new { error = "Пустой путь" });

        path = HttpUtility.UrlDecode(path);
        
        _logger.LogDebug("Get image with filename = {Filename}", path);

        var image = _dbContext.Media
            .Select(i => new
            {
                i.Path,
                i.ContentType
            })
            .ToList()
            .FirstOrDefault(i => string.Join(Path.DirectorySeparatorChar, i.Path.Split(Path.DirectorySeparatorChar).SkipWhile(s => s != "wwwroot").Skip(1)) == path);
        
        if (image is null)
            return RedirectToAction("Index", "Errors", new { error = "Не найдено изображение" });
        
        var imageData = await System.IO.File.ReadAllBytesAsync(image.Path);

        return File(imageData, image.ContentType);
    }

    [HttpPost]
    [AllowAnonymous]
    [Route("Upload")]
    [RequestSizeLimit(8 * 1024 * 1024)]
    public async Task<IActionResult> Upload([FromForm]MediaCreateDto mediaCreateDto)
    {
        if (mediaCreateDto is null or ({UserId: <= 0} and {AdId: <= 0}) or {Image: null})
            return RedirectToAction("Index", "Errors", new { error = "Id must be greater than 0" });
        
        var uniqueFileName = FileNameHelper.GetUniqueFileName(mediaCreateDto.Image.FileName);
        
        var filePath = Path.Combine(_environment.WebRootPath,
            "images",
            uniqueFileName);

        _logger.LogDebug("Upload image with path = {Name}", filePath);
        
        var media = new Media
        {
            AdId = mediaCreateDto.AdId == 0 ? null : mediaCreateDto.AdId,
            UserId = mediaCreateDto.UserId == 0 ? null : mediaCreateDto.UserId,
            ContentType = mediaCreateDto.Image.ContentType,
            Path = filePath
        };

        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(filePath) ?? throw new InvalidOperationException("Directory name is null"));
            
            _dbContext.Media.Add(media);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return StatusCode(StatusCodes.Status500InternalServerError,
                "Произошла ошибка");
        }
        finally
        {
            await using var stream = new FileStream(filePath, FileMode.CreateNew);
            await mediaCreateDto.Image.CopyToAsync(stream);
        }

        return await _dbContext.SaveChangesAsync() switch
        {
            0 =>
             RedirectToAction("Index", "Errors", new { error = "Id must be greater than 0" }),
            _ => Ok(ImageUriHelper.GetImagePathAsUri(media.Path))
        };
    }
}