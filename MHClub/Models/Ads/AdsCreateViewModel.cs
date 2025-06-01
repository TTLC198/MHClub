using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MHClub.Domain.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MHClub.Models.Ads;

public class AdsCreateViewModel : AdsIndexViewModel
{
    [Display(Name = "Название объявления")]
    [Required(ErrorMessage = "Значение не может быть пустым")]
    public  string Name { get; set; } = string.Empty;

    [Display(Name = "Стоимость товара в рублях")]
    [Required(ErrorMessage = "Значение не может быть пустым")]
    [Range(0, double.MaxValue, ErrorMessage = "Значение должно быть числом")]
    public float Cost { get; set; }

    [Display(Name = "Страна производства")]
    public  string? ManufactureCountry { get; set; }

    [Display(Name = "Количество")]
    [Range(1, int.MaxValue, ErrorMessage = "Значение должно быть числом больше одного")]
    public  int? Quantity { get; set; }

    [Display(Name = "Описание")] 
    public string? Description { get; set; } = "";

    [Display(Name = "Категория")]
    [Required(ErrorMessage = "Значение не может быть пустым")]
    public  int? CategoryId { get; set; }

    [Display(Name = "Состояние")]
    [Required(ErrorMessage = "Значение не может быть пустым")]
    public  int? ConditionId { get; set; }

    public List<SelectListItem> CountriesSelect { get; set; } = new();
    
    public AdsCreateViewModel() {}
    
    public AdsCreateViewModel(Ad ads) : base(ads) {}
    [DisplayName("Изображения")]
    public List<IFormFile> Images { get; set; } = [];
}