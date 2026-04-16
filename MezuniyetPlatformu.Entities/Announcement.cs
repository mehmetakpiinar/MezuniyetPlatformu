using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MezuniyetPlatformu.Entities
{
    public class Announcement
    {
        [Key]
        public int AnnouncementId { get; set; }

        public string Title { get; set; }
        public string Message { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public int UniversityId { get; set; }

        [ForeignKey("UniversityId")]
        public University University { get; set; }
    }
}