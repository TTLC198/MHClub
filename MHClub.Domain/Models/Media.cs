using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MHClub.Domain.Models;

[Table("media")]
public class Media
{
    [Column("id")]
    public int Id { get; set; }

    [Display(Name = "Путь к файлу")]
    [Column("path")]
    public string Path { get; set; }
    
    [Display(Name = "Тип контента")]
    [Column("content_type")]
    public string ContentType { get; set; }

    [Display(Name = "Объявление")]
    [Column("idad")]
    public int? AdId { get; set; }
    
    [ForeignKey("AdId")]
    public Ad? Ad { get; set; }
    
    [Display(Name = "Пользователь")]
    [Column("iduser")]
    public int? UserId { get; set; }
    
    [ForeignKey("UserId")]
    public User? User { get; set; }
}