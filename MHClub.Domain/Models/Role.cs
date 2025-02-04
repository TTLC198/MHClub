using System.ComponentModel.DataAnnotations.Schema;

namespace MHClub.Domain.Models;

using System.ComponentModel.DataAnnotations;

[Table("role")]
public class Role
{
    public int Id { get; set; }

    [Display(Name = "Название роли")]
    public string Name { get; set; } = string.Empty;
}