using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.VM.Classes.ResourceAllocation
{
    public class RAGetRaisedRequest
    {
        public bool IsRmg { get; set; }
        public int ID { get; set; }
        public int ProjectID { get; set; }
        public int SubProjectID { get; set; }
        public string ProjectName { get; set; }
        public int RequestedBy { get; set; }
        public string RequestedByName { get; set; }
        public int RequestType { get; set; }
        public int Status { get; set; }
        public int StatusBy { get; set; }
        public string StatusByName { get; set; }
        public System.DateTime RequestDate { get; set; }
        public System.DateTime StatusDate { get; set; }
        public bool IsDeleted { get; set; }
        public List<RANewRequest> RANewRequest { get; set; }
        public List<RAUpdateRequest> RAUpdateRequest { get; set; }
        public List<RAExtentionRequest> RAExtentionRequest { get; set; }
        public List<RAReleaseRequest> RAReleaseRequest { get; set; }
    }
}
