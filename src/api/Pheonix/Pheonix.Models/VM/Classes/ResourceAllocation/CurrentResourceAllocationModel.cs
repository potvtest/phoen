using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.VM.Classes.ResourceAllocation
{
    public class CurrentResourceAllocationModel
    {
        public int ID { get; set; }
        public string FullName { get; set; }
        public Nullable<bool> IsDeleted { get; set; }
        public int RequestType { get; set; }
        public int RequestBy { get; set; }
        public string Comments { get; set; }
        public string RMGComments { get; set; }
        public int ProjectID { get; set; }
        public string ProjectName { get; set; }
        public int? SubProjectID { get; set; }
        public string SubProjectName { get; set; }
        public int PersonID { get; set; }
        public string ResourceName { get; set; }
        public int percentage { get; set; }
        public System.DateTime FromDate { get; set; }
        public System.DateTime ToDate { get; set; }
        public int BillbleType { get; set; }
        public string BillbleTypeName { get; set; }
        public int ProjectRole { get; set; }
        public string RoleName { get; set; }
        public int ReportingTo { get; set; }
        public string ReportingToName { get; set; }
        public RAEmploymentDetails EmploymentDetails { get; set; }
        public System.DateTime StatusDate { get; set; }
        public System.DateTime RequestDate { get; set; }
        public System.DateTime ActionDate { get; set; }
        public Nullable<System.DateTime> ReleaseDate { get; set; }
        public bool IsProjected { get; set; }
        public int AllocationID { get; set; }

    }
}
