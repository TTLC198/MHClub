using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MHClub.Domain.Models;

[Table("complaint")]
public class Complaint
{
    [Column("id")]
    public int Id { get; set; }

    [Display(Name = "Описание жалобы")]
    [Column("description")]
    public string? Description { get; set; }

    [Display(Name = "Пользователь")]
    [Column("iduser")]
    public int UserId { get; set; }

    [ForeignKey("iduser")]
    public User User { get; set; } = new User();

    [Display(Name = "Объявление")]
    [Column("idad")]
    public int AdId { get; set; }

    [ForeignKey("AdId")]
    public Ad Ad { get; set; } = new Ad();
}