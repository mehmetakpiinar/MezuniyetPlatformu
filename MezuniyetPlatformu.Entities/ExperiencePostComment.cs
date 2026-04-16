using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MezuniyetPlatformu.Entities
{
    public class ExperiencePostComment
    {
        [Key]
        public int CommentId { get; set; }
        public int PostId { get; set; }
        public int UserId { get; set; }
        public string CommentText { get; set; }
        public DateTime CommentDate { get; set; }

        public User User { get; set; }
    }
}
