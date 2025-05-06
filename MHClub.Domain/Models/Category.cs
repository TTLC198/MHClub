using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MHClub.Domain.Models;

[Table("category")]
public class Category
{
    public int Id { get; set; }
    
    [Display(Name = "Название категории")]
    [Column("category_name")]
    public string? Name { get; set; }
    
    [Column("id_child_caterory")]
    public int? ParentCategoryId { get; set; }
    
    [ForeignKey("ParentCategoryId")]
    public Category? ParentCategory { get; set; }

    public List<Category> Children { get; set; } = [];
}