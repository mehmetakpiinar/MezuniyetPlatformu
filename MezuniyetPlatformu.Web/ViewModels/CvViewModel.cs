using System;
using System.Collections.Generic;

namespace MezuniyetPlatformu.Web.ViewModels
{
    public class CvViewModel
    {
        // Kişisel Bilgiler
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }

        // Eğitim Bilgileri
        public string University { get; set; }
        public string Department { get; set; }
        public int GraduationYear { get; set; }

        // Hakkında & Yetenekler
        public string Bio { get; set; }

        // Yeni Eklediğimiz Alan: Yetenekler (String olarak tutacağız, view'da böleriz)
        public string Skills { get; set; }

        // Deneyimler
        public List<ExperienceDto> Experiences { get; set; } = new List<ExperienceDto>();
    }

    public class ExperienceDto
    {
        public string Title { get; set; }
        public string Company { get; set; }
        public string Description { get; set; }
        public DateTime Date { get; set; }
    }
}