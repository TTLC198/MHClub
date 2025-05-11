using MHClub.Domain.Models.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MHClub.Models.Ads;

public class AdsSearchViewModel
{
    public double? MinPrice { get; set; }
    public double? MaxPrice { get; set; }
    public bool HighRating { get; set; }
    public ItemCondition? Condition { get; set; }
    public ItemType? Type { get; set; }
    public string SortBy { get; set; }
}