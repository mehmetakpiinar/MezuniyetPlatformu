namespace MezuniyetPlatformu.API.DTOs
{
    public class UniversityDashboardDto
    {
        public string UniversityName { get; set; }
        public string UniversityLogo { get; set; }

        // KPI Verileri
        public int TotalStudents { get; set; }
        public int EmployedGraduates { get; set; }
        public int JobSeekers { get; set; }

        // Grafik Verileri
        public int SoftwareSectorCount { get; set; }
        public int FinanceSectorCount { get; set; }
        public int EngineeringSectorCount { get; set; }
    }
}
