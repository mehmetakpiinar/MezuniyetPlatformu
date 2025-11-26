using System;

namespace MezuniyetPlatformu.Web.ViewModels
{
    public class MessageViewModel
    {
        public long MessageId { get; set; }
        public int SenderUserId { get; set; }
        public int RecipientUserId { get; set; }
        public string Content { get; set; }
        public DateTime SentDate { get; set; }
        public bool IsRead { get; set; }

        // Bu mesajı kimin gönderdiğini arayüzde göstermek için
        // (Örn: "Ben" mi yoksa "Karşı Taraf" mı?)
        // Bunu Controller'da dolduracağız.
        public bool IsMyMessage { get; set; }
    }
}