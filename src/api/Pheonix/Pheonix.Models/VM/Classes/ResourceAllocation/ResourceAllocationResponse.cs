using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.VM.Classes.ResourceAllocation
{
    public class ResourceAllocationResponse
    {
        public int PersonID { get; set; }
        public string PersonName { get; set; }
        public bool IsSuccess { get; set; }
        public string Response { get; set; }
        public int Percentage { get; set; }
        public int RequestType { get; set; }
    }
}
