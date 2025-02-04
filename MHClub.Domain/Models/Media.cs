using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MHClub.Domain.Models;

[Table("media")]
public class Media
{
    public int Id { get; set; }

    [Display(Name = "Путь к файлу")]
    public string? Way { get; set; }

    [Display(Name = "Объявление")]
    public int? AdId { get; set; }

    public Ad? Ad { get; set; }
}