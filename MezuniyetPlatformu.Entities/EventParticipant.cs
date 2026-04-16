using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MezuniyetPlatformu.Entities
{
    public class EventParticipant
    {
        [Key]
        public int Id { get; set; }
        public int EventId { get; set; }
        public int UserId { get; set; }
        public DateTime ParticipationDate { get; set; } = DateTime.Now;

        public Event Event { get; set; }
        public User User { get; set; }
    }
}
