using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MHClub.Domain.Models;

[Table("favourites")]
public class Favourite
{
    public int Id { get; set; }

    [Display(Name = "Объявление")]
    public int AdId { get; set; }

    public Ad Ad { get; set; } = new Ad();

    [Display(Name = "Пользователь")]
    public int UserId { get; set; }

    public User User { get; set; } = new User();
}