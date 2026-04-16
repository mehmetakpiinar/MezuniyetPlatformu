using System;

namespace MezuniyetPlatformu.Web.ViewModels
{
    public class StudentViewModel
    {
        public int UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public DateTime RegisterTime { get; set; }

        // Ekranda Ad Soyad birleşik göstermek istersen helper property:
        public string FullName => $"{FirstName} {LastName}";
        public List<ExperiencePostViewModel> Experiences { get; set; } = new List<ExperiencePostViewModel>();
    }
}