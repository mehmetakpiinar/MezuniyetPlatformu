using System;

namespace MezuniyetPlatformu.Web.ViewModels
{
    public class JobApplicationViewModel
    {
        public int BasvuruID { get; set; } // API'deki isimle aynı olmalı (ApplicationId olabilir, kontrol et!)
        public int JobPostId { get; set; }
        public int CandidateUserId { get; set; }
        public DateTime ApplicationDate { get; set; }
        public string Status { get; set; }

        // Başvuran öğrencinin bilgilerini (Ad, Soyad, Email) tutmak için
        public UserViewModel CandidateUser { get; set; }
    }
}