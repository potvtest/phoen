using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.VM.Classes.ResourceAllocation
{
    public class RARaisedRequest
    {
        public RARequest Request { get; set; }
        public List<RARequestDetail> RequestDetail { get; set; }
        public List<RAResource> Resource { get; set; }
    }
}
