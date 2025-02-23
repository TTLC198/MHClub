using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MHClub.Domain.Models;

[Table("favourites")]
public class Favourite
{
    public int Id { get; set; }

    [Display(Name = "Объявление")]
    [Column("AdId")]
    [ForeignKey("AdId")]
    public int AdId { get; set; }

    public Ad Ad { get; set; } = new Ad();

    [Display(Name = "Пользователь")]
    [Column("UserId")]
    [ForeignKey("UserId")]
    public int UserId { get; set; }

    public User User { get; set; } = new User();
}