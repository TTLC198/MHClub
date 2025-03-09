using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace MHClub.Domain.Models;

[Table("users")]
public class User
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int? Id { get; set; }

    [Display(Name = "Имя пользователя")]
    [Required(ErrorMessage = "Необходимо заполнить поле")]
    [RegularExpression("^[a-zA-Zа-яА-Я]+$", ErrorMessage = "Имя должно содержать только буквы (русские или английские)")]
    public string? Name { get; set; }

    [Display(Name = "Email")]
    [Required(ErrorMessage = "Необходимо заполнить поле")]
    [RegularExpression("^[a-zA-Z0-9._-]+@[a-zA-Z0-9.-]+\\.[a-zA-Z]{2,6}$", ErrorMessage = "Пожалуйста, введите действительный адрес электронной почты")]
    public string? Email { get; set; }

    [Display(Name = "Телефон")]
    [Required(ErrorMessage = "Необходимо заполнить поле")]
    [RegularExpression("^[+]?(\\d{1,4})?(\\d{10})$", ErrorMessage = "Пожалуйста, введите действительный номер телефона")]
    public string? Phone { get; set; }

    [Display(Name = "Пароль")]
    [Required(ErrorMessage = "Необходимо заполнить поле")]
    [RegularExpression(@"^(?=.*[a-zа-яё])(?=.*[A-ZА-ЯЁ])(?=.*\d).{8,64}$",
        ErrorMessage =
            "Пароль должен содержать от 8 до 64 символов, включая как минимум одну заглавную букву, одну строчную букву и одну цифру")]
    [StringLength(50, MinimumLength = 8, ErrorMessage = "минимальная длина пароля - 8 символов")]
    public string? Password { get; set; }

    [Display(Name = "Дата регистрации")]
    public DateOnly DateOfRegistration { get; set; }

    [Display(Name = "Роль")]
    public int RoleId { get; set; }

    [JsonIgnore]
    public Role? Role { get; set; }
    
    [JsonIgnore]
    public List<Media>? Medias { get; set; }
    
    [JsonIgnore]
    public List<Favourite>? Favourites { get; set; }
}