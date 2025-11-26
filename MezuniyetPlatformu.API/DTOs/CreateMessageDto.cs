using System.ComponentModel.DataAnnotations;

namespace MezuniyetPlatformu.API.DTOs
{
    public class CreateMessageDto
    {
        [Required]
        public int RecipientUserId { get; set; }

        [Required]
        [StringLength(1000, ErrorMessage = "Mesaj 1000 karakterden uzun olamaz.")]
        public string Content { get; set; } 
    }
}