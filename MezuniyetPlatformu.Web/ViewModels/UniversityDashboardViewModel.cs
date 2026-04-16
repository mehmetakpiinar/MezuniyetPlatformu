namespace MezuniyetPlatformu.Web.ViewModels
{
    public class UniversityDashboardViewModel
    {
        public string UniversityName { get; set; }
        public string UniversityLogo { get; set; }

        // KPI Kartları için Veriler
        public int TotalStudents { get; set; }      // Toplam Kayıtlı Öğrenci
        public int EmployedGraduates { get; set; }  // İşe Girmiş Mezunlar
        public int JobSeekers { get; set; }         // Aktif İş Arayanlar

        // Grafik için Veri (Sektör Dağılımı - Basit Örnek)
        public int SoftwareSectorCount { get; set; }
        public int FinanceSectorCount { get; set; }
        public int EngineeringSectorCount { get; set; }
    }
}