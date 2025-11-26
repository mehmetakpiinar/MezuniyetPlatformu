// MezuniyetPlatformu.Web/ViewModels/LoginViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace MezuniyetPlatformu.Web.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "E-posta alanı zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi girin.")]
        [Display(Name = "E-posta Adresi")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Şifre alanı zorunludur.")]
        [DataType(DataType.Password)]
        [Display(Name = "Şifre")]
        public string Sifre { get; set; } // API'deki LoginDto ile aynı ismi verdim

        [Display(Name = "Beni Hatırla")]
        public bool RememberMe { get; set; } = false; // "Beni Hatırla" kutucuğu (ileride kullanabiliriz)
    }
}