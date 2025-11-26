using System.ComponentModel.DataAnnotations;

namespace MezuniyetPlatformu.Web.ViewModels
{
    public class CreatePostViewModel
    {
        [Required(ErrorMessage = "Başlık zorunludur.")]
        [Display(Name = "Başlık")]
        [StringLength(100, ErrorMessage = "Başlık en fazla 100 karakter olabilir.")]
        public string Title { get; set; }

        [Required(ErrorMessage = "İçerik zorunludur.")]
        [Display(Name = "Deneyim İçeriği")]
        public string Content { get; set; }
    }
}