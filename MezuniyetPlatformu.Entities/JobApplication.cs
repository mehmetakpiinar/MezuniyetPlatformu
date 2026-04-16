using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MezuniyetPlatformu.Entities
{
    public class JobApplication
    {
        [Key]
        public int ApplicationId { get; set; }

        public int JobPostId { get; set; }

        [ForeignKey("JobPostId")]
        public JobPost JobPost { get; set; }

        public int CandidateUserId { get; set; }

        public DateTime ApplicationDate { get; set; }
        public string Status { get; set; }

        [ForeignKey("CandidateUserId")]
        public User CandidateUser { get; set; }
    }
}