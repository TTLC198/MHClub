using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MHClub.Domain.Models;
using MHClub.Models.Medias;
using MHClub.Utils;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MHClub.Models.Ads;

public class AdsCreateViewModel : AdsIndexViewModel
{
    [Display(Name = "Название объявления")]
    [Required(ErrorMessage = "Значение не может быть пустым")]
    public override string Name { get; set; } = string.Empty;

    [Display(Name = "Стоимость товара в рублях")]
    [Required(ErrorMessage = "Значение не может быть пустым")]
    [Range(0, double.MaxValue, ErrorMessage = "Значение должно быть числом")]
    public override float Cost { get; set; }

    [Display(Name = "Страна производства")]
    [Required(ErrorMessage = "Значение не может быть пустым")]
    public override string ManufactureCountry { get; set; }

    [Display(Name = "Количество")]
    [Required(ErrorMessage = "Значение не может быть пустым")]
    [Range(1, int.MaxValue, ErrorMessage = "Значение должно быть числом больше одного")]
    public override int? Quantity { get; set; }

    [Display(Name = "Описание")]
    [Required(ErrorMessage = "Значение не может быть пустым")]
    public override string Description { get; set; }

    [Display(Name = "Категория")]
    [Required(ErrorMessage = "Значение не может быть пустым")]
    public override int CategoryId { get; set; }

    [Display(Name = "Состояние")]
    [Required(ErrorMessage = "Значение не может быть пустым")]
    [Range(0, 3, ErrorMessage = "Выберите состояние товара")]
    public override int ConditionId { get; set; }
    
    [Display(Name = "Использовать как основной товар")]
    public bool IsMainAd { get; set; }

    public List<SelectListItem> CountriesSelect { get; set; } = new();
    
    public List<SelectListItem> CategoriesSelect { get; set; } = new();
    
    public List<MediaDto> ExistingMedia { get; set; } = new();
    
    public AdsCreateViewModel() {}

    public AdsCreateViewModel(Ad ads) : base(ads)
    {
        // Заполняем список существующих медиа (если передаёте их из контроллера)
        if (ads.Medias != null)
        {
            ExistingMedia = ads.Medias
                .Select(m => new MediaDto 
                {
                    Id = m.Id,
                    Url = m.Path
                    // либо Url.Action("Get", "Media", new { path = m.Path })
                })
                .ToList();
        }
    }
    [DisplayName("Изображения")]
    public List<IFormFile> Images { get; set; } = [];
}