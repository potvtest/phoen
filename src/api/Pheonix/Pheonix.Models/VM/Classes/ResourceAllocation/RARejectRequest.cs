using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.VM.Classes.ResourceAllocation
{
    public class RARejectRequest
    {
        public int RequestID { get; set; }
        public int RequestType { get; set; }
        public string Comment { get; set; }
        public int UserID { get; set; }
    }
}
