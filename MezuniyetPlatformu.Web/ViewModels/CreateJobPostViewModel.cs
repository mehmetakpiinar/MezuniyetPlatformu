using System;
using System.ComponentModel.DataAnnotations;

namespace MezuniyetPlatformu.Web.ViewModels
{
    public class CreateJobPostViewModel
    {
        [Required(ErrorMessage = "İlan başlığı zorunludur.")]
        [Display(Name = "İlan Başlığı")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Açıklama zorunludur.")]
        [Display(Name = "İlan Açıklaması")]
        public string Description { get; set; }

        [Display(Name = "Konum")]
        public string? Location { get; set; }

        [Required(ErrorMessage = "Çalışma şekli zorunludur.")]
        [Display(Name = "Çalışma Şekli (Tam Zamanlı, Staj vb.)")]
        public string JobType { get; set; }

        [Display(Name = "Son Başvuru Tarihi")]
        [DataType(DataType.Date)]
        public DateTime? Deadline { get; set; }
    }
}