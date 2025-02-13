using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.VM.Classes.ResourceAllocation
{
    public class RAMyAllocation
    {
        public List<RACurrentAllocation> CurrentAllocation { get; set; }
        public IEnumerable<RAProjectedAllocation> ProjectedAllocation { get; set; }
        public List<RAHistoryAllocation> HistoryAllocation { get; set; }

    }
}
