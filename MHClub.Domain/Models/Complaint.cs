using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MHClub.Domain.Models;

[Table("complaint")]
public class Complaint
{
    public int Id { get; set; }

    [Display(Name = "Описание жалобы")]
    public string? Description { get; set; }

    [Display(Name = "Пользователь")]
    public int UserId { get; set; }

    public User User { get; set; } = new User();

    [Display(Name = "Объявление")]
    public int AdId { get; set; }

    public Ad Ad { get; set; } = new Ad();
}