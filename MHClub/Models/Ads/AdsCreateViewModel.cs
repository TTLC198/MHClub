using MHClub.Domain.Models;

namespace MHClub.Models.Ads;

public class AdsCreateViewModel : AdsIndexViewModel
{
    public AdsCreateViewModel() {}
    
    public AdsCreateViewModel(Ad ads) : base(ads) {}
    public List<IFormFile> ImageFiles { get; set; } = [];
}