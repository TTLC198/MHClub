using Microsoft.AspNetCore.Mvc.Rendering;

namespace MHClub.Models.Ads;

public class AdsSearchViewModel
{
    public string? FilterText { get; set; }
    
    public int MinPrice { get; set; } = 384;
    public int MaxPrice { get; set; } = 60255;
    public bool HighRating { get; set; } = false;
    public string Condition { get; set; } = "Новое";
    public string Type { get; set; } = "Основное";

    public string SortBy { get; set; } = "";
}