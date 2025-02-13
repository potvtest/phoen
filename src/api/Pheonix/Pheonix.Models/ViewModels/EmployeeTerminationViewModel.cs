using Pheonix.Models.VM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.ViewModels
{
    public class EmployeeTerminationViewModel
    {
        public int PersonID { get; set; }
        public int ApprovalID { get; set; }
        public string TerminationReason { get; set; }
        public string TerminationRemark { get; set; }
        public DateTime ExitDate { get; set; }
        public Boolean IsTermination { get; set; }
        public string AttachedFile { get; set; }
        //public List<SeperationConfigProcessViewModel> SeperationConfigData { get; set; }
        //public EmployeeBasicProfile EmployeeProfile { get; set; }
        //public List<SeperationViewModel> Data { get; set; }

    }
}
