using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MHClub.Domain.Models;

[Table("favorite")]
public class Favourite
{
    public int Id { get; set; }

    [Display(Name = "Объявление")]
    [Column("idad")]
    [ForeignKey("idad")]
    public int AdId { get; set; }

    public Ad Ad { get; set; } = new Ad();

    [Display(Name = "Пользователь")]
    [Column("iduser")]
    [ForeignKey("iduser")]
    public int UserId { get; set; }

    public User User { get; set; } = new User();
}