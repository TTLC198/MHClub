using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MHClub.Domain.Models;

[Table("category")]
public class Category
{
    public int Id { get; set; }
    
    [Column("parentcategoryid")]
    public int? ParentCategoryId { get; set; }
    
    [ForeignKey("ParentCategoryId")]
    public Category? ParentCategory { get; set; }

    public List<Category> Children { get; set; } = [];

    [Display(Name = "Название категории")]
    public string? Name { get; set; }
}