using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MHClub.Domain.Models;

[Table("tariff")]
public class Tariff
{
    [Column("id")]
    public int Id { get; set; }

    [Display(Name = "Название тарифа")]
    [Required]
    [Column("name")]
    public string Name { get; set; } = string.Empty;
    
    [Display(Name = "Стоимость")]
    [Column("cost")]
    public int Cost { get; set; }
}