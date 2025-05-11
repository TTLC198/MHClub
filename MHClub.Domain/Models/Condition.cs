using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MHClub.Domain.Models;

[Table("condition")]
public class Condition
{
    [Column("id")]
    public int Id { get; set; }

    [Display(Name = "Состояние")]
    [Column("name")]
    public string? Name { get; set; }
}