using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.VM.Classes.ResourceAllocation
{
    public class RAHistoryAllocation
    {
        public int ID { get; set; }
        public int ResourceAllocationID { get; set; }
        public int PersonID { get; set; }
        public string FullName { get; set; }
        public int ProjectID { get; set; }
        public string ProjectName { get; set; }
        public int? SubProjectID { get; set; }
        public string SubProjectName { get; set; }
        public int Percentage { get; set; }
        public System.DateTime FromDate { get; set; }
        public System.DateTime ToDate { get; set; }
        public int ProjectRole { get; set; }
        public string RoleName { get; set; }
        public Nullable<bool> IsDeleted { get; set; }
        public int ReportingTo { get; set; }
        public string ReportingToName { get; set; }
        public int BillbleType { get; set; }
        public string BillbleTypeName { get; set; }
        public RAEmploymentDetails EmploymentDetails { get; set; }
        public Nullable<DateTime> ReleaseDate { get; set; }
        public Nullable<int> ResourcePoolID { get; set; }
        public int ModifyBy { get; set; }
        public List<RARatings> Ratings { get; set; }
        public int RequestType { get; set; }
        
    }
}
