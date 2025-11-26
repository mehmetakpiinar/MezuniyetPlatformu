// MezuniyetPlatformu.Web/ViewModels/ExperiencePostViewModel.cs
using System;

namespace MezuniyetPlatformu.Web.ViewModels
{
    // API'den dönen JSON ile isimlerin eşleşmesi önemli
    // (Senin API'ndeki ExperiencePost entity'ne göre uyarlanmıştır)
    public class ExperiencePostViewModel
    {
        public int PostId { get; set; }
        public int AuthorUserId { get; set; }

        // API'miz 'Include(p => p.AuthorUser)' yaptığı için
        // Yazarın bilgilerini de alacak bir iç nesneye ihtiyacımız var.
        public UserViewModel AuthorUser { get; set; }

        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }

    // AuthorUser içindeki bilgileri yakalamak için basit bir User ViewModel
    // (Eğer bu 'UserViewModel'ı başka yerlerde de kullanacaksak,
    // ayrı bir 'UserViewModel.cs' dosyasına taşımak daha iyi bir pratiktir)
    public class UserViewModel
    {
        public int UserId { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int UserTypeId { get; set; }
    }
}