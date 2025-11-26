namespace MezuniyetPlatformu.API.DTOs
{
    public class UpdateProfileDto
    {
        public string ProfilePhotoURL { get; set; }
        public string About { get; set; }
        public int GraduationYear { get; set; }
        public string StudyProgram { get; set; }
        public string LinkedInURL { get; set; }
        public string GitHubURL { get; set; }
        public string PhoneNumber { get; set; }
    }
}
