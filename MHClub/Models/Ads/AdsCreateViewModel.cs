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
    public override string Name { get; set; } = string.Empty;

    [Display(Name = "Стоимость товара в рублях")]
    [Required(ErrorMessage = "Значение не может быть пустым")]
    [Range(10, double.MaxValue, ErrorMessage = "Значение должно быть числом")]
    public override float Cost { get; set; }

    [Display(Name = "Страна производства")]
    public override string? ManufactureCountry { get; set; }

    [Display(Name = "Количество")]
    [Range(1, int.MaxValue, ErrorMessage = "Значение должно быть числом")]
    public override int? Quantity { get; set; }

    [Display(Name = "Описание")]
    public override string? Description { get; set; } = "";

    [Display(Name = "Страна промежуточного прибытия")]
    public override string? CountryOfIntermediateArrival { get; set; }

    [Display(Name = "Страна доставки")]
    [Required(ErrorMessage = "Значение не может быть пустым")]
    public override string CountryOfDelivery { get; set; } = string.Empty;
    
    [Display(Name = "Страна отправления")]
    [Column("startcountry")]
    public override string? StartCountry { get; set; }

    [Display(Name = "Высота")]
    [Required(ErrorMessage = "Значение не может быть пустым")]
    [Range(0, int.MaxValue, ErrorMessage = "Значение должно быть числом")]
    public override int Height { get; set; }

    [Display(Name = "Ширина")]
    [Required(ErrorMessage = "Значение не может быть пустым")]
    [Range(0, int.MaxValue, ErrorMessage = "Значение должно быть числом")]
    public override int Width { get; set; }

    [Display(Name = "Длина")]
    [Required(ErrorMessage = "Значение не может быть пустым")]
    [Range(0, int.MaxValue, ErrorMessage = "Значение должно быть числом")]
    public override int Length { get; set; }
    
    [Display(Name = "Объем (см\u00b3)")]
    [Range(0, int.MaxValue, ErrorMessage = "Значение должно быть числом")]
    [Column("volume")]
    public override int? Volume { get; set; }
    
    [Display(Name = "Приблизительный вес с упаковкой за единицу (г)")]
    [Required(ErrorMessage = "Значение не может быть пустым")]
    [Range(0, int.MaxValue, ErrorMessage = "Значение должно быть числом")]
    [Column("weight")]
    public override int? Weight { get; set; }

    [Display(Name = "Наценка продавца (в %)")]
    [Required(ErrorMessage = "Значение не может быть пустым")]
    [Range(0, int.MaxValue, ErrorMessage = "Значение должно быть числом")]
    public override int? SellerMargin { get; set; }

    [Display(Name = "Рассчет НЕ через наш сервис")]
    [Required(ErrorMessage = "Значение не может быть пустым")]
    public override bool OurService { get; set; } = true;

    [Display(Name = "Конечная стоимость доставки до покупателя")]
    [Range(10, int.MaxValue, ErrorMessage = "Значение должно быть числом")]
    public override int? CostOfDelivery { get; set; }

    [Display(Name = "Таможенный сбор в стране отправки (с учетом веса)")]
    [Range(0, int.MaxValue, ErrorMessage = "Значение должно быть числом")]
    public override int? CustomsDuty1 { get; set; }

    [Display(Name = "Таможенный сбор в стране приемки (с учетом веса)")]
    [Range(0, int.MaxValue, ErrorMessage = "Значение должно быть числом")]
    public override int? CustomsDuty2 { get; set; }
    
    [Display(Name = "Банковская комиссия (в %)")]
    [Range(0, int.MaxValue, ErrorMessage = "Значение должно быть числом")]
    public override int? BankCommission { get; set; }

    [Display(Name = "Категория")]
    [Required(ErrorMessage = "Значение не может быть пустым")]
    public override int? CategoryId { get; set; }

    [Display(Name = "Тариф")]
    [Required(ErrorMessage = "Значение не может быть пустым")]
    public override int? TariffId { get; set; }

    [Display(Name = "Состояние")]
    [Required(ErrorMessage = "Значение не может быть пустым")]
    public override int? ConditionId { get; set; }

    public List<SelectListItem> CountriesSelect { get; set; } = new();
    public List<SelectListItem> CategoriesSelect { get; set; } = new();
    
    [DisplayName("Примерные сроки доставки")]
    public int? PlannedDeliveryDays { get; set; }
    
    public AdsCreateViewModel() {}
    
    public AdsCreateViewModel(Ad ads) : base(ads) {}
    [DisplayName("Изображения")]
    public List<IFormFile> Images { get; set; } = [];
}