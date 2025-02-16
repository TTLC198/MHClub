using System.ComponentModel.DataAnnotations;

namespace MHClub.Models.User;

public class UserEditDto : Domain.Models.User
{
    public IFormFile? Avatar { get; set; }
    public string ImageUrl { get; set; }
    
    [Display(Name = "Повторите пароль")]
    [Required(ErrorMessage = "Необходимо заполнить поле")]
    [StringLength(50, MinimumLength = 8, ErrorMessage = "Минимальная длина пароля - 8 символов")]
    public string RepeatPassword { get; set; } = string.Empty;
    
    [Display(Name = "Введите старый пароль")]
    [Required(ErrorMessage = "Необходимо заполнить поле")]
    [StringLength(50, MinimumLength = 8, ErrorMessage = "Минимальная длина пароля - 8 символов")]
    public string OldPassword { get; set; } = string.Empty;
    
    public bool IsPasswordEquals => 
        string.Equals(Password, RepeatPassword, StringComparison.InvariantCultureIgnoreCase);
    
    public UserEditDto() {}
    
    public UserEditDto(Domain.Models.User user, string imageUrl)
    {
        Email = user.Email;
        Phone = user.Phone;
        Name = user.Name;
        DateOfRegistration = user.DateOfRegistration;
        ImageUrl = imageUrl;
    }
}