namespace MezuniyetPlatformu.API.DTOs
{
    public class ConversationDto
    {
        public int UserId { get; set; } // Karşıdaki Kişinin ID'si
        public string FullName { get; set; } // Adı Soyadı
        public string ProfilePhotoUrl { get; set; } // Profil Resmi
        public string LastMessageContent { get; set; } // Son Mesaj İçeriği
        public DateTime LastMessageDate { get; set; } // Son Mesaj Tarihi
        public int UnreadCount { get; set; } // Okunmamış Mesaj Sayısı
    }
}
