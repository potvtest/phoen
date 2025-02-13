using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.VM.Classes.ResourceAllocation
{
    public class RAUpdateAllocations
    {
        public bool IsRmg { get; set; }
        public RARequest RARequest { get; set; }
        public RARequestDetail RequestDetail { get; set; }
        public RAResource Resource { get; set; }
    }
}
