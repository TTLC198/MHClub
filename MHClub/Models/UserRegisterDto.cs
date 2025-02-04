using System.ComponentModel.DataAnnotations;
using MHClub.Domain.Models;

namespace MHClub.Models;

public class UserRegisterDto : User
{
    [Display(Name = "Повторите пароль")]
    [Required(ErrorMessage = "Необходимо заполнить поле")]
    [StringLength(50, MinimumLength = 8, ErrorMessage = "Минимальная длина пароля - 8 символов")]
    public string RepeatPassword { get; set; } = string.Empty;
    
    public bool IsPasswordEquals => 
        string.Equals(Password, RepeatPassword, StringComparison.InvariantCultureIgnoreCase);
}