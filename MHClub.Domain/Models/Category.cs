using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MHClub.Domain.Models;

[Table("category")]
public class Category
{
    public int Id { get; set; }

    [Display(Name = "Название категории")]
    public string? Name { get; set; }
}