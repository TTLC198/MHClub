using MHClub.Domain.Models;

namespace MHClub.Models.Ads;

public class AdsIndexViewModel : Ad
{
    public List<string> Images { get; set; } = [];

    public AdsIndexViewModel()
    {
    }

    public AdsIndexViewModel(Ad ad)
    {
        Id = ad.Id;
        Name = ad.Name;
        Cost = ad.Cost;
        Status = ad.Status;
        ManufactureCountry = ad.ManufactureCountry;
        Quantity = ad.Quantity;
        Description = ad.Description;
        CountryOfIntermediateArrival = ad.CountryOfIntermediateArrival;
        CountryOfDelivery = ad.CountryOfDelivery;
        Height = ad.Height;
        Width = ad.Width;
        Length = ad.Length;
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
    }
}