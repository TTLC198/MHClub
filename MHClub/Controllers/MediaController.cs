using System.Net;
using System.Net.Mime;
using System.Web;
using MHClub.Domain;
using MHClub.Domain.Models;
using MHClub.Models;
using MHClub.Services;
using MHClub.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MHClub.Controllers;

[Controller]
[Route("Media")]
public class MediaController : BaseController
{
    private readonly ILogger<MediaController> _logger;
    private readonly IWebHostEnvironment _environment;
    private readonly ApplicationDbContext _dbContext;
    private readonly MediaService _mediaService;

    public MediaController(ILogger<MediaController> logger, ApplicationDbContext dbContext, IWebHostEnvironment environment, MediaService mediaService)
    {
        _logger = logger;
        _dbContext = dbContext;
        _environment = environment;
        _mediaService = mediaService;
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

        var uploadResult = await _mediaService.UploadImage(mediaCreateDto);

        return uploadResult.Item1 ? Ok(uploadResult.Item2) : BadRequest(uploadResult.Item2);
    }
    
    [Authorize]
    [HttpGet]
    [Route("upload")]
    public ActionResult GetUploadForm()
    {
        return PartialView("_ImageUpload");
    }
}