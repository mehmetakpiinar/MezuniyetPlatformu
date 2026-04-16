namespace MezuniyetPlatformu.Web.ViewModels
{
    // Bu sınıfı artık hem Profil hem de Deneyim sayfaları ortak kullanacak
    public class UserViewModel
    {
        public int UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
    }
}