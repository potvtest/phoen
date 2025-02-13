using Pheonix.DBContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.VM.Classes.ResourceAllocation
{
    public class ResourceAllocationModel
    {
        public int RequestID{get; set;}
        public int id { get; set; }
        public int ProjectID { get; set; }
        public string ProjectName { get; set; }
        public int RequestedBy { get; set; }
     
        public int RequestType { get; set; }
        public int Status { get; set; }     
      
        public System.DateTime RequestDate { get; set; }
        public System.DateTime StatusDate { get; set; }
      
        public int personid { get; set; }
        public int empid { get; set; }
        public string ResourceName { get; set; }
        public int percentage { get; set; }
        public System.DateTime fromdate { get; set; }
        public System.DateTime ToDate { get; set; }
        public int BillableType { get; set; }
        public int ProjectRole { get; set; }
        public int ReportingTo { get; set; }
        public string Description { get; set; }
        public string RMGCOmments { get; set; }
        public bool IsDeleted { get; set; }
      
    }
}
