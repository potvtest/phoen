using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.VM
{
    public class FHLeaveViewModel
    {
        public string HolydayName { get; set; }
        public System.DateTime? HolidayDate { get; set; }
        public bool HolydayAvailability { get; set; }
    }
}
