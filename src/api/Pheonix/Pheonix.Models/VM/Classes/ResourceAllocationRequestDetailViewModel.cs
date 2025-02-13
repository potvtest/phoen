using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.VM.Classes
{
    public class ResourceAllocationRequestDetailViewModel
    {
        public int RequestID { get; set; }
        public int PersonID { get; set; }
        public string ResourceName { get; set; }
        public int Percentage { get; set; }
        public int BillbleType { get; set; }
        public System.DateTime FromDate { get; set; }
        public System.DateTime ToDate { get; set; }
        public int Role { get; set; }
        public string RoleName { get; set; }
        public int ReportingTo { get; set; }
        public string ReportingToName { get; set; }
        public string Description { get; set; }
        public int Status { get; set; }
        public string RMGCOmments { get; set; }
        public System.DateTime StatusDate { get; set; }
        public bool IsDeleted { get; set; }
        public int ModifyBy { get; set; }
    }
}
