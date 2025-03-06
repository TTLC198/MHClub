using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace MHClub.Domain.Models;

[Table("reviews")]
public class Review
{
    public int Id { get; set; }

    [Display(Name = "Оценка")]
    [Required]
    [Range(1, 5)]
    public int Estimation { get; set; }

    [Display(Name = "Оставьте письменный отзыв")]
    [Required]
    public string Description { get; set; } = string.Empty;

    [Display(Name = "Объявление")]
    [Column("AdId")]
    [ForeignKey("AdId")]
    [ValidateNever]
    public int AdId { get; set; }

    [ValidateNever]
    public Ad? Ad { get; set; }

    [Display(Name = "Пользователь")]
    [ValidateNever]
    public int UserId { get; set; }

    [ValidateNever]
    public User? User { get; set; }
}