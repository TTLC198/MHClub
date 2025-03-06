using System.ComponentModel;
using MHClub.Domain.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MHClub.Models.Ads;

public class AdsCreateViewModel : AdsIndexViewModel
{
    public List<SelectListItem> CountriesSelect { get; set; } = new();
    public List<SelectListItem> CategoriesSelect { get; set; } = new();
    
    [DisplayName("Примерные сроки доставки")]
    public int? PlannedDeliveryDays { get; set; }
    [DisplayName("Итого")]
    public int? TotalCost { get; set; }
    [DisplayName("Стоимость товара в рублях")]
    public int? ItemCost { get; set; }
    
    public AdsCreateViewModel() {}
    
    public AdsCreateViewModel(Ad ads) : base(ads) {}
    [DisplayName("Изображения")]
    public List<IFormFile> Images { get; set; } = [];
}