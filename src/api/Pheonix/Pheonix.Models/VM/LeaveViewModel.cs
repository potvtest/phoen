using Pheonix.Models.VM.Classes.Employee;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.VM
{
    public class LeaveViewModel<T>
    {
        public IEnumerable<T> EmployeeLeaveViewModels { get; set; }
        public AvailableLeaves AvailableLeaves { get; set; }
        public IEnumerable<CompOffViewModel> CompOffsDetails { get; set; }

        public IEnumerable<EmployeeAdminHistoryData> AdminLeaveDetails { get; set; }
    }
}
