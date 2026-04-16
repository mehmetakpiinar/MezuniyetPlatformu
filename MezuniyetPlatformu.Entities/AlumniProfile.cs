using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MezuniyetPlatformu.Entities
{
    [Table("AlumniProfile")]
    public class AlumniProfile
    {
        [Key]
        public int ProfileId { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public string? ProfilePhotoURL { get; set; }
        public string? About { get; set; }
        public int? GraduationYear { get; set; }
        public string? StudyProgram { get; set; }
        public string? LinkedInURL { get; set; }
        public string? GitHubURL { get; set; }
        public string? PhoneNumber { get; set; }
        public string? UniversityName { get; set; }
        public string? Skills { get; set; }
    }
}
