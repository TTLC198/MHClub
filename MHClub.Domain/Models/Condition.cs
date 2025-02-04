using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MHClub.Domain.Models;

[Table("condition")]
public class Condition
{
    public int Id { get; set; }

    [Display(Name = "Состояние")]
    public string? Name { get; set; }
}