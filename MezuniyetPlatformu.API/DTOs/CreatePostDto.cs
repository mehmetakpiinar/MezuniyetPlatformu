using System.ComponentModel.DataAnnotations;

namespace MezuniyetPlatformu.API.DTOs
{
    public class CreatePostDto
    {
        [Required(ErrorMessage = "Başlık zorunludur.")]
        [StringLength(255)]
        public string Title { get; set; }

        [Required(ErrorMessage = "İçerik zorunludur.")]
        public string Content { get; set; }

    }
}