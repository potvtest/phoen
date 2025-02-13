using Pheonix.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.VM.Classes.ResourceAllocation
{
    public class RAResource
    {
        public int AllocationID { get; set; }
        public int RequestID { get; set; }
        public int ProjectID { get; set; }
        public int EmpID { get; set; }
        public int Id { get; set; }
        public string FullName { get; set; }
        public int Allocation { get; set; }
        public int Billability { get; set; }
        public string BillabilityName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int ProjectRole { get; set; }
        public string RoleName { get; set; }
        public int ProjectReporting { get; set; }
        public RAEmploymentDetails EmploymentDetails { get; set; }
        public string ProjectReportingName { get; set; }
        public Nullable<System.DateTime> ReleaseDate { get; set; }
        public List<RARatings> Ratings { get; set; }
        public List<int?> PMSProjectAction { get; set; }
        public Nullable<System.DateTime> ActionDate { get; set; }
        public int RequestedBy { get; set; }
        public int StatusBy { get; set; }
        public string Comments { get; set; }

        public bool IsBGCRequired { get; set; }
        public int? SubProjectID { get; set; }
        public string SubProjectName { get; set; }


    }
}
