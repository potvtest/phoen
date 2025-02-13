using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.VM.Classes.ResourceAllocation
{
    public class RANewAllocations
    {
        public RA ResourceAllocation { get; set; }
        public bool ByResourceName { get; set; }
        public bool ByOthers { get; set; }
    }
}
