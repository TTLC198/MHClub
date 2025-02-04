using MHClub.Domain.Models;

namespace MHClub.Models;

public class UserProfileDto : User
{
    public double Rating { get; set; }
    
    public int ReviewsCount { get; set; }
    
    public int AdsCount { get; set; }
    
    public UserProfileDto() {}
    
    public UserProfileDto(User user, double rating, int reviewsCount, int adsCount)
    {
        Email = user.Email;
        Name = user.Name;
        DateOfRegistration = user.DateOfRegistration;
        Photo = user.Photo;
        Password = user.Password;
        Rating = rating;
        ReviewsCount = reviewsCount;
        AdsCount = adsCount;
    }
}