using MHClub.Domain.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace MHClub.Models.User;

public class UserProfileDto : Domain.Models.User
{
    
    public string ImageUrl { get; set; }
    
    public double? Rating { get; set; }
    
    public int? ReviewsCount { get; set; }
    
    public int? AdsCount { get; set; }

    public Review ReviewToCreate { get; set; } = new();
    
    public UserProfileDto() {}
    
    public UserProfileDto(Domain.Models.User user, double? rating, int? reviewsCount, int? adsCount, string imageUrl)
    {
        Email = user.Email;
        Name = user.Name;
        Phone = user.Phone;
        DateOfRegistration = user.DateOfRegistration;
        Password = user.Password;
        Rating = rating;
        ReviewsCount = reviewsCount;
        AdsCount = adsCount;
        ImageUrl = imageUrl;
    }
}