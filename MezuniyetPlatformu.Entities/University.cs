using System.ComponentModel.DataAnnotations;

namespace MezuniyetPlatformu.Entities
{
    public class University
    {
        [Key]
        public int UniversityId { get; set; }

        [Required]
        public string Name { get; set; } 

        public string? LogoUrl { get; set; } 
        public string? Website { get; set; }
    }
}