using System.ComponentModel.DataAnnotations;

namespace MezuniyetPlatformu.API.DTOs
{
    public class LoginDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Sifre { get; set; }
    }
}