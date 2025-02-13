using Pheonix.DBContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.VM.Classes.ResourceAllocation
{
    public class ResourceAllocationDetailModel
    {
        public int RequestID { get; set; }
        public int ID { get; set; }
        public int PersonID { get; set; }
        public int EmpID { get; set; }
        public int Percentage { get; set; }
        public System.DateTime FromDate { get; set; }
        public System.DateTime ToDate { get; set; }
        public string Role { get; set; }
        public string ReportingTo { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public string RMGCOmments { get; set; }
        public System.DateTime StatusDate { get; set; }
       // public string BillableType { get; set; }
    }
}
