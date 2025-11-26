using System;
using System.Collections.Generic;

namespace MezuniyetPlatformu.Entities
{
    public class UserType
    {
        public int UserTypeId { get; set; }
        public string TypeName { get; set; }
        public ICollection<User> Users { get; set; }
    }
}
