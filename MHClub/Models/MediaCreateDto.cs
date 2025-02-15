using System.ComponentModel.DataAnnotations;

namespace MHClub.Models;

public class MediaCreateDto
{
    public int? AdId { get; set; }
    
    public int? UserId { get; set; }

    [Required] public IFormFile Image { get; set; }
}