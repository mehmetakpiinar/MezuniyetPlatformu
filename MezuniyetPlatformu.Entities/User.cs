using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MezuniyetPlatformu.Entities
{
    public class User
    {
        [Key]
        public int UserId { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime RegisterTime { get; set; }

        public int UserTypeId { get; set; }

        [ForeignKey("UserTypeId")]
        public UserType TypeName { get; set; }

        public int? UniversityId { get; set; }

        [ForeignKey("UniversityId")]
        public University? University { get; set; }
        public AlumniProfile? AlumniProfile { get; set; }
        public EmployerProfile? EmployerProfile { get; set; }
    }
}