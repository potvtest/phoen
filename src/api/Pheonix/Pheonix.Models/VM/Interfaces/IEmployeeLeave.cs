using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.VM.Interfaces
{
    public interface IEmployeeLeave
    {
        int ID { get; set; }
        DateTime FromDate { get; set; }
        DateTime ToDate { get; set; }
        string Narration { get; set; }
        int LeaveType { get; set; }
        int Leaves { get; set; }
        int Absent { get; set; }
        int Status { get; set; }
    }
}
