// MezuniyetPlatformu.Web/ViewModels/RegisterViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace MezuniyetPlatformu.Web.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Ad alanı zorunludur.")]
        [Display(Name = "Ad")] // Formda etiketin ne görüneceği
        public string FirstName { get; set; } // API'deki DTO ile aynı isimleri kullanmak işimizi kolaylaştırır

        [Required(ErrorMessage = "Soyad alanı zorunludur.")]
        [Display(Name = "Soyad")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "E-posta alanı zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi girin.")]
        [Display(Name = "E-posta Adresi")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Şifre alanı zorunludur.")]
        [DataType(DataType.Password)] // Formda bu alanı şifre (***) olarak gösterir
        [Display(Name = "Şifre")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Şifre tekrar alanı zorunludur.")]
        [DataType(DataType.Password)]
        [Display(Name = "Şifre Tekrar")]
        [Compare("Password", ErrorMessage = "Şifreler uyuşmuyor.")] // Password alanı ile karşılaştırır
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Kullanıcı tipi seçmelisiniz.")]
        [Display(Name = "Kayıt Tipi")]
        public int UserTypeId { get; set; } // 1: Ogrenci, 2: Mezun, 3: Isveren
        [Display(Name = "Üniversite")]
        public int? UniversityId { get; set; }
    }
}