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
    [Column("estimation")]
    public int Estimation { get; set; }

    [Display(Name = "Оставьте письменный отзыв")]
    [Required]
    [Column("description")]
    public string Description { get; set; } = string.Empty;

    [Display(Name = "Объявление")]
    [Column("idad")]
    public int AdId { get; set; }

    [ForeignKey("AdId")]
    public Ad? Ad { get; set; }

    [Display(Name = "Пользователь")]
    [Column("iduser")]
    public int UserId { get; set; }

    [ForeignKey("UserId")]
    public User? User { get; set; }
}