using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.VM
{
    public class PersonLeaveViewModel
    {
        public int? LeaveType { get; set; }
        public System.DateTime FromDate { get; set; }
        public int? NotConsumed { get; set; }
        public int? Status { get; set; }
    }
}
