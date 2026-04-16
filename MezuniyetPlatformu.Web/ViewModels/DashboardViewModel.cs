namespace MezuniyetPlatformu.Web.ViewModels
{
    public class DashboardViewModel
    {
        public string Role { get; set; }

        // İşveren Verileri
        public int TotalPosts { get; set; }
        public int ActivePosts { get; set; }
        public int TotalApplications { get; set; }

        // Öğrenci Verileri
        public int MyApplications { get; set; }
        public int TotalOpportunities { get; set; }
        public int UnreadMessages { get; set; }
        public int ProfileCompletionRate { get; set; }
        public List<int> ChartData { get; set; }
    }
}