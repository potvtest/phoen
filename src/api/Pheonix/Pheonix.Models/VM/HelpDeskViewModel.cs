using Pheonix.DBContext;
using Pheonix.Models.VM.Classes.HelpDesk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.VM
{
    public class HelpDeskViewModel
    {
        public HelpDeskReadOnlyModel HelpDesk { get; set; }
        public List<HelpDeskCommentModel> HelpDeskComments { get; set; }
        public EmployeeBasicProfile EmployeeProfile { get; set; }
        public List<DropdownItems> Assigness { get;set; }
        public List<DropdownItems> OtherDepartmentAdmin { get; set; }
        public double DurationRemaining { get; set; }
    }
}
