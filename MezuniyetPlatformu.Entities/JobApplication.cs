using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MezuniyetPlatformu.Entities
{
    public class JobApplication
    {
        [Key]
        public int ApplicationId { get; set; }

        public int JobPostId { get; set; } 

        public int CandidateUserId { get; set; } 

        public DateTime ApplicationDate { get; set; }
        public string Status { get; set; }
        public User CandidateUser { get; set; }
    }
}
