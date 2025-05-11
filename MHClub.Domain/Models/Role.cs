using System.ComponentModel.DataAnnotations.Schema;

namespace MHClub.Domain.Models;

using System.ComponentModel.DataAnnotations;

[Table("role")]
public class Role
{
    [Column("id")]
    public int Id { get; set; }

    [Display(Name = "Название роли")]
    [Column("name")]
    public string Name { get; set; } = string.Empty;
}