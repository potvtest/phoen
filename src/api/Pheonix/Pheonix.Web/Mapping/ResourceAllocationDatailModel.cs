using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pheonix.Models.VM.Classes.ResourceAllocation
{
    public class ResourceAllocationDatailModel
    {
        public int ID { get; set; }
        public int PersonID { get; set; }
        public int EmpID { get; set; }
        public int Percentage { get; set; }
        public System.DateTime FromDate { get; set; }
        public System.DateTime ToDate { get; set; }
        public string Role { get; set; }
        public string ReportingTo { get; set; }
        public string Description { get; set; }
        public string RMGCOmments { get; set; }
    }
}
