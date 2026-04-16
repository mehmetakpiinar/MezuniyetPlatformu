using System;
using System.ComponentModel.DataAnnotations;

namespace MezuniyetPlatformu.Web.ViewModels
{
    public class EventViewModel
    {
        public int EventId { get; set; }

        [Display(Name = "Etkinlik Başlığı")]
        [Required(ErrorMessage = "Başlık zorunludur.")]
        public string Title { get; set; }

        [Display(Name = "Açıklama")]
        [Required(ErrorMessage = "Açıklama zorunludur.")]
        public string Description { get; set; }

        [Display(Name = "Tarih ve Saat")]
        [Required(ErrorMessage = "Tarih zorunludur.")]
        public DateTime EventDate { get; set; }

        [Display(Name = "Konum / Yer")]
        [Required(ErrorMessage = "Konum zorunludur.")]
        public string Location { get; set; }

        public int UniversityId { get; set; }

        // Görünümde gün/ay ayrımı yapmak istersen ekstra özellikler ekleyebilirsin
        public string Day => EventDate.ToString("dd");
        public string Month => EventDate.ToString("MMM");

        public string? ImageUrl { get; set; } // Veritabanından gelen yol
        public IFormFile? ImageUpload { get; set; } // Formdan gelen dosya

        public bool IsJoined { get; set; }
    }
}