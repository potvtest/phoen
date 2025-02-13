using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.ViewModels
{
    public class VPBUApproverHandlerModel
    {
        public int userId { get; set; }
        public bool isBUApprover { get; set; }
        public bool isBUFilterOn { get; set; }
        public List<long> assignedBUList { get; set; }
        public List<long> copyOfAssignedBUList { get; set; }
        public bool isSelfIdea { get; set; }
        public bool isSearchByEmp { get; set; }
        public string queryEmpNameOrIds { get; set; }
        public List<long> statusList { get; set; }
    }
}
