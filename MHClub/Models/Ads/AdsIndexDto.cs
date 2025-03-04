namespace MHClub.Models.Ads;

public class AdsIndexDto
{
    public AdsSearchViewModel AdsSearchViewModel { get; set; } = new();
    public List<AdsIndexViewModel> Ads { get; set; } = [];
}