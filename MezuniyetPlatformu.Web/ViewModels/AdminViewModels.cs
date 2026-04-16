using System;

namespace MezuniyetPlatformu.Web.ViewModels
{
    // İstatistikleri taşıyan model
    public class AdminStatsViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalJobPosts { get; set; }
        public int ActiveJobPosts { get; set; }
        public int TotalExperiencePosts { get; set; }
    }

    // Kullanıcı listesini taşıyan model
    public class AdminUserViewModel
    {
        public int UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public DateTime RegisterTime { get; set; }
    }
    public class UniversityViewModel
    {
        public int UniversityId { get; set; }
        public string Name { get; set; }
        public string LogoUrl { get; set; }
        public int StudentCount { get; set; }
    }
    public class AdminJobPostViewModel
    {
        public int JobPostId { get; set; }
        public string Title { get; set; }
        public string CompanyName { get; set; }
        public string EmployerName { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsActive { get; set; }
    }

    public class AdminExperiencePostViewModel
    {
        public int PostId { get; set; }
        public string Title { get; set; }
        public string AuthorName { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}