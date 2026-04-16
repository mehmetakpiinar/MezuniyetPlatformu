using System;
using System.ComponentModel.DataAnnotations;

namespace MezuniyetPlatformu.Entities
{
    public class Event
    {
        [Key]
        public int EventId { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        public DateTime EventDate { get; set; } // Tarih ve Saat

        [Required]
        public string Location { get; set; } 

        public bool IsActive { get; set; } = true;

        public int UniversityId { get; set; }

        public string? ImageUrl { get; set; }
    }
}