using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.VM
{
    public class EmployeeAddressViewModel
    {
        public string Address { get; set; }
        public string Country { get; set; }
        public bool IsCurrent { get; set; }
        public string Phone { get; set; }
        public string PersonalEmail { get; set; }
        public string Mobile { get; set; }
    }
}
