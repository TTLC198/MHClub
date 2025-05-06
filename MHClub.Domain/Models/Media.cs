using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MHClub.Domain.Models;

[Table("media")]
public class Media
{
    public int Id { get; set; }

    [Display(Name = "Путь к файлу")]
    [Column("path")]
    public string Path { get; set; }
    
    [Display(Name = "Тип контента")]
    [Column("contenttype")]
    public string ContentType { get; set; }

    [Display(Name = "Объявление")]
    [Column("idad")]
    [ForeignKey("idad")]
    public int AdId { get; set; }
    
    public Ad? Ad { get; set; }
    
    [Display(Name = "Пользователь")]
    [Column("iduser")]
    [ForeignKey("iduser")]
    public int UserId { get; set; }

    public User? User { get; set; }
}