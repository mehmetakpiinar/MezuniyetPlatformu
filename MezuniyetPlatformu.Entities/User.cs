using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
    }
}