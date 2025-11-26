using System.ComponentModel.DataAnnotations;

namespace MezuniyetPlatformu.API.DTOs
{
    public class CreateJobPostingDto
    {
        [Required(ErrorMessage = "İlan başlığı zorunludur.")]
        public string Title { get; set; }

        [Required(ErrorMessage = "İlan açıklaması zorunludur.")]
        public string Description { get; set; }

        public string? Location { get; set; }

        [Required(ErrorMessage = "İlan tipi zorunludur.")]
        public string JobType { get; set; }

        public DateTime? Deadline { get; set; }
    }
}
