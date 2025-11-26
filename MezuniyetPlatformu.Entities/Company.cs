using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MezuniyetPlatformu.Entities
{
    public class Company
    {
        public int CompanyId { get; set; }
        public string CompanyName { get; set; }
        public string? Website { get; set; }
        public string? LogoURL { get; set; }
        public string? Sector { get; set; }
        public string? About { get; set; }
    }
}
