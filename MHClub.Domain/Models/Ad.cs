using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace MHClub.Domain.Models;

[Table("ad")]
public class Ad
{
    [Column("id")]
    public virtual int Id { get; set; }

    [Display(Name = "Название объявления")]
    [Column("ad_name")]
    public virtual string Name { get; set; } = string.Empty;

    [Display(Name = "Стоимость товара в рублях")]
    [Range(10, double.MaxValue, ErrorMessage = "Значение должно быть числом")]
    [Column("cost")]
    public virtual float Cost { get; set; }

    [Display(Name = "Страна производства")]
    [Column("manufacturecountry")]
    public virtual string? ManufactureCountry { get; set; }

    [Display(Name = "Количество")]
    [Column("quantity")]
    public virtual int? Quantity { get; set; }

    [Display(Name = "Описание")]
    [Column("description")]
    public virtual string? Description { get; set; } = "";

    [Display(Name = "Дата создания")]
    [Column("create_date")]
    public virtual DateTime CreationDate { get; set; }

    [Display(Name = "Категория")]
    [Column("idcategory")]
    public virtual int CategoryId { get; set; }
    public virtual Category? Category { get; set; }

    [Display(Name = "Состояние")]
    [Column("idcondition")]
    public virtual int ConditionId { get; set; }

    [ForeignKey("ConditionId")]
    public virtual Condition? Condition { get; set; }

    [Display(Name = "Статус")]
    [Column("idstatus")]
    public virtual int StatusId { get; set; }

    [ForeignKey("StatusId")]
    public virtual Status? Status { get; set; }

    [Display(Name = "Продавец")]
    [Column("id_user")]
    public virtual int SellerId { get; set; }

    [ForeignKey("SellerId")]
    public virtual User? Seller { get; set; }

    [Display(Name = "Дочернее объявление")]
    [Column("id_child_ad")]
    public virtual int? ChildAdId { get; set; }

    [ForeignKey("ChildAdId")]
    public virtual Ad? ChildAd { get; set; }

    [JsonIgnore] 
    public List<Media>? Medias { get; set; }

    [JsonIgnore] 
    public List<Review>? Reviews { get; set; }

    [JsonIgnore] 
    public List<Favourite>? Favourites { get; set; }

    public Ad() {}
    
    public Ad(Ad ad)
    {
        Id = ad.Id;
        Name = ad.Name;
        Cost = ad.Cost;
        ManufactureCountry = ad.ManufactureCountry;
        Quantity = ad.Quantity;
        Description = ad.Description;
        CreationDate = ad.CreationDate;
        CategoryId = ad.CategoryId;
        ConditionId = ad.ConditionId;
        StatusId = ad.StatusId;
        SellerId = ad.SellerId;
        ChildAdId = ad.ChildAdId;
    }
}