using System.ComponentModel.DataAnnotations;

namespace WebsocketChat.Library.Models
{
    public class ChangePasswordModel
    {
        [Required(ErrorMessage = "Обязательно для заполнения")]
        [DataType(DataType.Password)]
        [Display(Name = "Старый пароль password")]
        public string OldPassword { get; set; }

        [Required(ErrorMessage = "Обязательно для заполнения")]
        [DataType(DataType.Password)]
        [Display(Name = "Новый пароль")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Подтверждение пароля должно совпадать с паролем")]
        [Display(Name = "Подтверждение пароля")]
        public string ConfirmPassword { get; set; }
    }
}
