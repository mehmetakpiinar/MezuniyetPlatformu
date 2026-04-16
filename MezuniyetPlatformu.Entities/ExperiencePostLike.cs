using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MezuniyetPlatformu.Entities
{
    public class ExperiencePostLike
    {
        [Key]
        public int LikeId { get; set; }
        public int PostId { get; set; }
        public int UserId { get; set; }
        public DateTime LikedDate { get; set; }
    }
}
