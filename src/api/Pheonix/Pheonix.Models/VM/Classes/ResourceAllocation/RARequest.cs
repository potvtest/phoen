using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.VM.Classes.ResourceAllocation
{
    public class RARequest
    {
        public bool IsRmg { get; set; }
        public int ID { get; set; }
        public int ProjectID { get; set; }
        public int SubProjectID { get; set; }
        public int RequestedBy { get; set; }
        public int RequestType { get; set; }
        public int Status { get; set; }
        public System.DateTime RequestDate { get; set; }
        public System.DateTime StatusDate { get; set; }
        public bool IsDeleted { get; set; }
    }
}
