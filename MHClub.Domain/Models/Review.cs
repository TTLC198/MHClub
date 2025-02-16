using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MHClub.Domain.Models;

[Table("reviews")]
public class Review
{
    public int Id { get; set; }

    [Display(Name = "Оценка")]
    [Required]
    [Range(1, 5)]
    public int Estimation { get; set; }

    [Display(Name = "Описание отзыва")]
    [Required]
    public string Description { get; set; } = string.Empty;

    [Display(Name = "Объявление")]
    [Column("AdId")]
    [ForeignKey("AdId")]
    public int AdId { get; set; }

    public Ad Ad { get; set; } = new Ad();

    [Display(Name = "Пользователь")]
    public int UserId { get; set; }

    public User User { get; set; } = new User();
}