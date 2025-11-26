// MezuniyetPlatformu.Entities/Message.cs

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MezuniyetPlatformu.Entities
{
    [Table("Messages")] // Veritabanındaki tablonun adıyla eşleştiğinden emin ol
    public class Message
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long MessageId { get; set; } // Çok fazla mesaj olabileceği için 'long' kullanmak iyi bir pratiktir

        public int SenderUserId { get; set; } // Gönderen Kullanıcı ID'si (FK)
        public int RecipientUserId { get; set; } // Alıcı Kullanıcı ID'si (FK)

        [Required]
        public string Content { get; set; } // Mesaj içeriği

        public DateTime SentDate { get; set; } // Gönderim tarihi

        public bool IsRead { get; set; } // Okundu mu?
        public DateTime? ReadDate { get; set; } // Okunma tarihi (null olabilir)


        // Navigation Properties (Gönderen ve Alan Kullanıcıların bilgilerine erişmek için)
        [ForeignKey("SenderUserId")]
        public User SenderUser { get; set; }

        [ForeignKey("RecipientUserId")]
        public User RecipientUser { get; set; }
    }
}