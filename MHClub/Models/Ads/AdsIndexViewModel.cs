using MHClub.Domain.Models;
using MHClub.Models.User;

namespace MHClub.Models.Ads;

public class AdsIndexViewModel : Ad
{
    public List<string>? Images { get; set; } = [];
    
    public bool? IsFavourite { get; set; }
    
    public bool? IsOwn { get; set; }
    public bool? IsArchived { get; set; }
    
    public UserProfileDto? UserProfileDto { get; set; }

    public AdsIndexViewModel()
    {
    }

    public AdsIndexViewModel(Ad ad) : base(ad) {}
}