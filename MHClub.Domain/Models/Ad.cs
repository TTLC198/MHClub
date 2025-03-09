using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace MHClub.Domain.Models;

[Table("ad")]
public class Ad
{
    public virtual int Id { get; set; }

    [Display(Name = "Название объявления")]
    public virtual string Name { get; set; } = string.Empty;

    [Display(Name = "Стоимость товара в рублях")]
    [Range(10, double.MaxValue, ErrorMessage = "Значение должно быть числом")]
    public virtual float Cost { get; set; }

    [Display(Name = "Статус")]
    public virtual bool Status { get; set; }

    [Display(Name = "Страна производства")]
    public virtual string? ManufactureCountry { get; set; }

    [Display(Name = "Количество")]
    public virtual int? Quantity { get; set; }

    [Display(Name = "Описание")]
    public virtual string? Description { get; set; } = "";

    [Display(Name = "Страна промежуточного прибытия")]
    public virtual string? CountryOfIntermediateArrival { get; set; }

    [Display(Name = "Страна доставки")]
    public virtual string CountryOfDelivery { get; set; } = string.Empty;
    
    [Display(Name = "Страна отправления")]
    [Column("startcountry")]
    public virtual string? StartCountry { get; set; }

    [Display(Name = "Высота")]
    public virtual int Height { get; set; }

    [Display(Name = "Ширина")]
    public virtual int Width { get; set; }

    [Display(Name = "Длина")]
    public virtual int Length { get; set; }
    
    [Display(Name = "Объем (см\u00b3)")]
    [Column("volume")]
    public virtual int? Volume { get; set; }
    
    [Display(Name = "Приблизительный вес с упаковкой за единицу (г)")]
    [Column("weight")]
    public virtual int? Weight { get; set; }

    [Display(Name = "Наценка продавца (в %)")]
    public virtual int? SellerMargin { get; set; }

    [Display(Name = "Рассчет НЕ через наш сервис")]
    public virtual bool OurService { get; set; } = true;

    [Display(Name = "Конечная стоимость доставки до покупателя")]
    public virtual int? CostOfDelivery { get; set; }

    [Display(Name = "Таможенный сбор в стране отправки (с учетом веса)")]
    public virtual int? CustomsDuty1 { get; set; }

    [Display(Name = "Таможенный сбор в стране приемки (с учетом веса)")]
    public virtual int? CustomsDuty2 { get; set; }
    
    [Display(Name = "Банковская комиссия (в %)")]
    public virtual int? BankCommission { get; set; }

    [Display(Name = "Категория")]
    public virtual int? CategoryId { get; set; }
    public virtual Category? Category { get; set; }

    [Display(Name = "Тариф")]
    public virtual int? TariffId { get; set; }

    public virtual Tariff? Tariff { get; set; }

    [Display(Name = "Состояние")]
    public virtual int? ConditionId { get; set; }

    [ForeignKey("ConditionId")]
    public virtual Condition? Condition { get; set; }
    
    [Column("sellerid")]
    public virtual int SellerId { get; set; }

    [ForeignKey("SellerId")]
    public virtual User? Seller { get; set; }
    
    [DisplayName("Итого")]
    [Column("totalcost")]
    public double? TotalCost { get; set; }
    
    [Column("creationdate")]
    public virtual DateTime CreationDate { get; set; }

    [JsonIgnore] public List<Media>? Medias { get; set; }

    [JsonIgnore] public List<Review>? Reviews { get; set; }

    [JsonIgnore] public List<Favourite>? Favourites { get; set; }

    public Ad() {}
    
    public Ad(Ad ad)
    {
        Id = ad.Id;
        TotalCost = ad.TotalCost;
        Name = ad.Name;
        Cost = ad.Cost;
        Status = ad.Status;
        ManufactureCountry = ad.ManufactureCountry;
        Quantity = ad.Quantity;
        Description = ad.Description;
        StartCountry = ad.StartCountry;
        CountryOfIntermediateArrival = ad.CountryOfIntermediateArrival;
        CountryOfDelivery = ad.CountryOfDelivery;
        Height = ad.Height;
        Width = ad.Width;
        Length = ad.Length;
        Weight = ad.Weight;
        Volume = ad.Volume;
        SellerMargin = ad.SellerMargin;
        OurService = ad.OurService;
        CostOfDelivery = ad.CostOfDelivery;
        CustomsDuty1 = ad.CustomsDuty1;
        CustomsDuty2 = ad.CustomsDuty2;
        BankCommission = ad.BankCommission;
        CategoryId = ad.CategoryId;
        TariffId = ad.TariffId;
        Tariff = ad.Tariff;
        ConditionId = ad.ConditionId;
        SellerId = ad.SellerId;
        CreationDate = ad.CreationDate;
    }
}