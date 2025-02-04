using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MHClub.Domain.Models;

[Table("tariff")]
public class Tariff
{
    public int Id { get; set; }

    [Display(Name = "Название тарифа")]
    [Required]
    public string Name { get; set; } = string.Empty;
}