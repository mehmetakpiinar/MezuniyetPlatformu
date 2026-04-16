using System;

namespace MezuniyetPlatformu.API.Dtos // Namespace projene göre değişebilir
{
    public class EventDto
    {
        public int EventId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime EventDate { get; set; }
        public string Location { get; set; }
        public int UniversityId { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsJoined { get; set; } // Kullanıcı katıldı mı?
    }
}