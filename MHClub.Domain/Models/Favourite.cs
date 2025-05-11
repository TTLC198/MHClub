using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MHClub.Domain.Models;

[Table("favorite")]
public class Favourite
{
    [Column("id")]
    public int Id { get; set; }

    [Display(Name = "Объявление")]
    [Column("idad")]
    public int AdId { get; set; }
    
    [ForeignKey("AdId")]
    public Ad Ad { get; set; } = new Ad();

    [Display(Name = "Пользователь")]
    [Column("iduser")]
    public int UserId { get; set; }
    
    [ForeignKey("UserId")]
    public User User { get; set; } = new User();
}