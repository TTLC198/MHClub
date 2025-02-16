using MHClub.Domain;
using MHClub.Domain.Models;
using MHClub.Models;
using MHClub.Utils;
using Microsoft.EntityFrameworkCore;

namespace MHClub.Services;

public class MediaService
{
    private readonly ILogger<MediaService> _logger;
    private readonly ApplicationDbContext _dbContext;
    private readonly IWebHostEnvironment _environment;

    public MediaService(ILogger<MediaService> logger, ApplicationDbContext dbContext, IWebHostEnvironment environment)
    {
        _logger = logger;
        _dbContext = dbContext;
        _environment = environment;
    }

    public async Task<ValueTuple<bool, string>> UploadImage(MediaCreateDto mediaCreateDto)
    {
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
            return new ValueTuple<bool, string>(false, e.Message);
        }
        finally
        {
            await using var stream = new FileStream(filePath, FileMode.CreateNew);
            await mediaCreateDto.Image.CopyToAsync(stream);
        }

        return await _dbContext.SaveChangesAsync() switch
        {
            0 => new ValueTuple<bool, string>(false, "Данные не сохранены"),
            _ => new ValueTuple<bool, string>(true, ImageUriHelper.GetImagePathAsUri(media.Path))
        };
    }
    
    public async Task<ValueTuple<bool, string>> Delete(int id)
    {
        if (id <= 0)
            return new ValueTuple<bool, string>(false, "Id = 0");
        
        _logger.LogDebug("Get image with id = {Id}", id);
        
        var image = await _dbContext.Media.FirstOrDefaultAsync(i => i.Id == id);

        if (image is null)
            return new ValueTuple<bool, string>(false, "Не найдено");
        
        File.Delete(image.Path);
        
        _dbContext.Media.Remove(image);
        await _dbContext.SaveChangesAsync();

        return new ValueTuple<bool, string>(true, "");
    }
}