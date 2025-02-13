using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.VM.Classes.ResourceAllocation
{
    public class RAUpdateRequest 
    {
        public int Id { get; set; }
        public int RequestID { get; set; }
        public string FullName { get; set; }
        public int AllocationID { get; set; }
        public Nullable<int> Allocation { get; set; }
        public Nullable<int> Billability { get; set; }
        public string BillabilityName { get; set; }
        public Nullable<System.DateTime> StartDate { get; set; }
        public Nullable<System.DateTime> EndDate { get; set; }
        public Nullable<int> ProjectRole { get; set; }
        public string RoleName { get; set; }
        public Nullable<int> ProjectReporting { get; set; }
        public string ProjectReportingName { get; set; }
        public bool IsRmg { get; set; }
        public int EmpID { get; set; }
        public int CreatedBy { get; set; }
        public string CreatedByName { get; set; }
        public Nullable<int> ModifyBy { get; set; }
        public string ModifyByName { get; set; }
        public DateTime CreatedDate { get; set; }
        public Nullable<System.DateTime> ModifyDate { get; set; }
        public string Comments { get; set; }
        public RAEmploymentDetails EmploymentDetails { get; set; }
        public string RMGComments { get; set; }
        public int Status { get; set; }
        public bool IsDeleted { get; set; }
        public Nullable<System.DateTime> ActionDate { get; set; }

    }
}
