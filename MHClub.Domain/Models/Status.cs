using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MHClub.Domain.Models;

[Table("status")]
public class Status
{
    [Column("id")]
    public int Id { get; set; }

    [Display(Name = "Название статуса")]
    [Column("status_name")]
    public string Name { get; set; } = string.Empty;
} 