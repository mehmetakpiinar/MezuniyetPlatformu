using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MezuniyetPlatformu.Entities
{
    [Table("JobPosts")]
    public class JobPost
    {
        [Key]
        public int JobPostId { get; set; }
        public int EmployerProfileId { get; set; }

        public int CompanyId { get; set; }

        public string Title { get; set; }
        public string Description { get; set; }
        public string? Location { get; set; }
        public string JobType { get; set; }
        public DateTime PublishedDate { get; set; }
        public DateTime? Deadline { get; set; }
        public bool IsActive { get; set; }

        [ForeignKey("EmployerProfileId")]
        public EmployerProfile EmployerProfile { get; set; }

        [ForeignKey("CompanyId")]
        public Company Company { get; set; }


        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}