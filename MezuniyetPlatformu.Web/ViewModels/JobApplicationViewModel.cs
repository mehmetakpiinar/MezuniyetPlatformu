using System;

namespace MezuniyetPlatformu.Web.ViewModels
{
    public class JobApplicationViewModel
    {
        // İsim düzeltmesi: API'den "applicationId" olarak geliyor
        public int ApplicationId { get; set; }

        public int JobPostId { get; set; }
        public int CandidateUserId { get; set; }
        public DateTime ApplicationDate { get; set; }
        public string Status { get; set; } // Örn: "Bekliyor", "Görüntülendi"

        // --- YENİ EKLENEN ALANLAR ---
        // Listede göstermek için ilan detayları
        public string? JobPostTitle { get; set; }
        public string? CompanyName { get; set; }
        public string? CompanyLogoURL { get; set; }

        // İşveren panelinde adayı göstermek için (Eskisi durabilir)
        public UserViewModel CandidateUser { get; set; }
    }
}