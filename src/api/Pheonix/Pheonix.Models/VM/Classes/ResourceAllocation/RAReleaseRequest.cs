using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.VM.Classes.ResourceAllocation
{
    public class RAReleaseRequest
    {
        public int Id { get; set; }
        public int RequestID { get; set; }
        public int Allocation { get; set; }
        public int Billability { get; set; }
        public string BillabilityName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int ProjectRole { get; set; }
        public string RoleName { get; set; }
        public int ProjectReporting { get; set; }
        public string ProjectReportingName { get; set; }
        public bool IsRmg { get; set; }
        public int EmpID { get; set; }
        public int ReportingTO { get; set; }
        public string FullName { get; set; }
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
        public List<RARatings> Ratings { get; set; }
        public Nullable<System.DateTime> ActionDate { get; set; }
    }
}
