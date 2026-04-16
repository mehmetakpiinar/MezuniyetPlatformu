using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http; // IFormFile için gerekli

namespace MezuniyetPlatformu.Web.ViewModels
{
    public class ProfileViewModel
    {
        public int ProfileId { get; set; }
        public int UserId { get; set; }

        public UserViewModel User { get; set; }

        [Display(Name = "Profil Fotoğrafı")]
        public string? ProfilePhotoURL { get; set; }

        [Display(Name = "Fotoğraf Yükle")]
        public IFormFile? ResimDosyasi { get; set; }

        [Display(Name = "Hakkımda")]
        public string? About { get; set; }

        [Display(Name = "Mezuniyet Yılı")]
        public int? GraduationYear { get; set; }

        [Display(Name = "Bölüm / Program")]
        public string? StudyProgram { get; set; }

        [Display(Name = "LinkedIn")]
        public string? LinkedInURL { get; set; }

        [Display(Name = "GitHub")]
        public string? GitHubURL { get; set; }

        [Display(Name = "Telefon Numarası")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "Üniversite Adı")]
        public string? UniversityName { get; set; }

        [Display(Name = "Yetenekler")]
        public string? Skills { get; set; }
    }
}