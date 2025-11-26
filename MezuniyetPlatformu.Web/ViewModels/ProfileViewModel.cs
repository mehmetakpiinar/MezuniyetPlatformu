using System.ComponentModel.DataAnnotations;

namespace MezuniyetPlatformu.Web.ViewModels
{
    public class ProfileViewModel
    {
        public int ProfileId { get; set; }
        public int UserId { get; set; }

        // Kullanıcının Adı/Soyadı bilgilerini göstermek için
        public UserViewModel User { get; set; }

        [Display(Name = "Profil Fotoğrafı URL")]
        public string? ProfilePhotoURL { get; set; }

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
    }
}