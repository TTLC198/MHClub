using System.ComponentModel;
using MHClub.Domain.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MHClub.Models.Ads;

public class AdsCreateViewModel : AdsIndexViewModel
{
    public List<SelectListItem> CountriesSelect { get; set; } = new();
    
    [DisplayName("Примерные сроки доставки")]
    public int? PlannedDeliveryDays { get; set; }
    [DisplayName("Итоговая стоимость")]
    public int? TotalCost { get; set; }
    
    public AdsCreateViewModel() {}
    
    public AdsCreateViewModel(Ad ads) : base(ads) {}
    [DisplayName("Изображения")]
    public List<IFormFile> Images { get; set; } = [];
}