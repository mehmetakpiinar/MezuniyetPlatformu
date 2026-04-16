using System;

namespace MezuniyetPlatformu.API.DTOs
{
    public class StudentDto
    {
        public int UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public DateTime RegisterTime { get; set; }
        // Bölüm bilgisi User tablosunda yoktu, o yüzden şimdilik eklemedim.
    }
}