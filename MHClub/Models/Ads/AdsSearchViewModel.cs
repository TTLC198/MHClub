using Microsoft.AspNetCore.Mvc.Rendering;

namespace MHClub.Models.Ads;

public class AdsSearchViewModel
{
    public string? FilterText { get; set; }
    public int? SelectedCategoryId { get; set; }
    public List<SelectListItem> Categories { get; set; } = new List<SelectListItem>();
}