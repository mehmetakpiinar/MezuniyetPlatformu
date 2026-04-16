namespace MezuniyetPlatformu.Web.ViewModels
{
    public class ConversationViewModel
    {
        public int UserId { get; set; }
        public string FullName { get; set; }
        public string? ProfilePhotoUrl { get; set; }
        public string LastMessageContent { get; set; }
        public DateTime LastMessageDate { get; set; }
        public int UnreadCount { get; set; }
    }
}