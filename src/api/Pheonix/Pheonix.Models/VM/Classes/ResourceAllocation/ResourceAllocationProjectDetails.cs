using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.VM.Classes.ResourceAllocation
{
    public class ResourceAllocationProjectDetails
    {
        public int ProjectID { get; set; }
        public string ProjectCode { get; set; }
        public string ProjectName { get; set; }
        public int? Status { get; set; }
        public System.DateTime ActualStartDate { get; set; }
        public System.DateTime ActualEndDate { get; set; }
        public List<int> BGCParameters { get; set; }
        public List<RAResource> Resources { get; set; }
        public string SubProjectName { get; set; }
        public int? SubProjectID { get; set; }
        public List<RASubProject> SubProjects { get; set; }
    }
}
