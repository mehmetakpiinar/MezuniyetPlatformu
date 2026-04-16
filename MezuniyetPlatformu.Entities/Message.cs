using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MezuniyetPlatformu.Entities
{
    [Table("Messages")]
    public class Message
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long MessageId { get; set; }

        public int SenderUserId { get; set; }
        public int RecipientUserId { get; set; }

        [Required]
        public string Content { get; set; }

        public DateTime SentDate { get; set; }

        public bool IsRead { get; set; }
        public DateTime? ReadDate { get; set; }

        [ForeignKey("SenderUserId")]
        public User SenderUser { get; set; }

        [ForeignKey("RecipientUserId")]
        public User RecipientUser { get; set; }
    }
}