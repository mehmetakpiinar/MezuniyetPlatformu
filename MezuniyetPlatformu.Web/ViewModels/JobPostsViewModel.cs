using System;

namespace MezuniyetPlatformu.Web.ViewModels
{
    public class JobPostsViewModel
    {
        public int JobPostId { get; set; }
        public int EmployerProfileId { get; set; }
        public int CompanyId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string? Location { get; set; }
        public string JobType { get; set; }
        public DateTime PublishedDate { get; set; }
        public DateTime? Deadline { get; set; }
        public bool IsActive { get; set; }

        // ---- YENİ EKLENEN KISIM ----
        // API'den gelen "employerProfile": { "userId": 11 ... } verisini yakalamak için
        public EmployerProfileViewModel EmployerProfile { get; set; }
    }

    // İç içe gelen veriyi karşılayacak küçük sınıf
    public class EmployerProfileViewModel
    {
        public int UserId { get; set; } // Bize lazım olan asıl bilgi bu!
    }
}