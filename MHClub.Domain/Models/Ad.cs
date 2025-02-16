using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MHClub.Domain.Models;

[Table("ad")]
public class Ad
{
    public int Id { get; set; }

    [Display(Name = "Название объявления")]
    [Required]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Стоимость")]
    [Required]
    [Range(0, double.MaxValue)]
    public float Cost { get; set; }

    [Display(Name = "Статус")]
    [Required]
    public bool Status { get; set; }

    [Display(Name = "Страна производства")]
    public string? ManufactureCountry { get; set; }

    [Display(Name = "Количество")]
    [Range(0, int.MaxValue)]
    public int? Quantity { get; set; }

    [Display(Name = "Описание")]
    public string? Description { get; set; }

    [Display(Name = "Страна промежуточного прибытия")]
    public string? CountryOfIntermediateArrival { get; set; }

    [Display(Name = "Страна доставки")]
    [Required]
    public string CountryOfDelivery { get; set; } = string.Empty;

    [Display(Name = "Высота")]
    [Required]
    [Range(0, int.MaxValue)]
    public int Height { get; set; }

    [Display(Name = "Ширина")]
    [Required]
    [Range(0, int.MaxValue)]
    public int Width { get; set; }

    [Display(Name = "Длина")]
    [Required]
    [Range(0, int.MaxValue)]
    public int Length { get; set; }

    [Display(Name = "Наценка продавца")]
    [Required]
    [Range(0, int.MaxValue)]
    public int SellerMargin { get; set; }

    [Display(Name = "Наш сервис")]
    [Required]
    public bool OurService { get; set; }

    [Display(Name = "Стоимость доставки")]
    [Range(0, int.MaxValue)]
    public int? CostOfDelivery { get; set; }

    [Display(Name = "Таможенная пошлина 1")]
    [Range(0, int.MaxValue)]
    public int? CustomsDuty1 { get; set; }

    [Display(Name = "Таможенная пошлина 2")]
    [Range(0, int.MaxValue)]
    public int? CustomsDuty2 { get; set; }

    [Display(Name = "Банковская комиссия")]
    [Range(0, int.MaxValue)]
    public int? BankCommission { get; set; }

    [Display(Name = "Категория")]
    public int CategoryId { get; set; }

    public Category Category { get; set; } = new Category();

    [Display(Name = "Тариф")]
    public int? TariffId { get; set; }

    public Tariff? Tariff { get; set; }

    [Display(Name = "Состояние")]
    public int ConditionId { get; set; }

    [ForeignKey("ConditionId")]
    public Condition Condition { get; set; } = new Condition();
    
    [Column("sellerid")]
    public int SellerId { get; set; }

    [ForeignKey("SellerId")] 
    public User Seller { get; set; } = new User();
    
    public List<Media> Medias { get; set; }
    
    public List<Review> Reviews { get; set; }
}