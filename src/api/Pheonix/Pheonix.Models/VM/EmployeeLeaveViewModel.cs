using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pheonix.Models.VM.Interfaces;

namespace Pheonix.Models.VM
{
    public class EmployeeLeaveViewModel : IEmployeeLeave
    {
        public int ID { get; set; }

        public DateTime FromDate { get; set; }

        public DateTime ToDate { get; set; }

        public string Narration { get; set; }

        public int LeaveType { get; set; }

        public int Leaves { get; set; }

        public int Absent { get; set; }

        public int Status { get; set; }

        public string StatusComment { get; set; }

        public int UserId { get; set; }

    }


    public class EmailLeaveDetails : EmployeeLeaveViewModel
    {
        public int personID { get; set; }

        public string  personName { get; set; }

        public string personOrganizationEmail { get; set; }

        public string approverOrganizationEmail { get; set; }

        public string approverName { get; set; }

        public int leaveCount { get; set; }

        public string personImage { get; set; }

        public string FromDate1 { get; set; }

        public string ToDate1 { get; set; }
    }

    public class ApprovalLeaveViewModel : EmployeeLeaveViewModel
    {
        public string UserName { get; set; }
        public string CurrentDesignation { get; set; }
        public string ImagePath { get; set; }
        public string Email { get; set; }
        public string Mobile { get; set; }
        public string OLText { get; set; }
        public string ResidenceNumber { get; set; }
        public string SeatingLocation { get; set; }
        public string Extension { get; set; }
    }

    public class ApprovalCompOffViewModel : ApprovalLeaveViewModel
    {
        public DateTime ForDate { get; set; }
        public DateTime ExpiresOn { get; set; }
        public bool IsApplied { get; set; }
        public bool HasManagerApproved { get; set; }
    }

    public class AvailableLeaves : IAvailableLeaves
    {
        public int? TotalLeaves { get; set; }
        public int? LeavesTaken { get; set; }
        public int? CompOff { get; set; }
        public int? LeavesAvailable { get; set; }
        public int? LeavesApplied { get; set; }
        public int? CompOffAvailable { get; set; }
        public int? LWP { get; set; }
        public int? CompOffConsumed { get; set; }
        public int? CLCredited { get; set; }
        public int? CLDebited { get; set; }
        public int? CLUtilized { get; set; }
        public int? CLApplied { get; set; }
        public int? SLCredited { get; set; }
        public int? SLDebited { get; set; }
        public int? SLUtilized { get; set; }
        public int? SLApplied { get; set; }


    }
}
