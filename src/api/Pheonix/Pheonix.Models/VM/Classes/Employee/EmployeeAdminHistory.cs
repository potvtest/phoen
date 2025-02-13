using System;
using System.Collections.Generic;

namespace Pheonix.Models.VM.Classes.Employee
{
    public class EmployeeAdminHistory
    {
        public List<EmployeeAdminHistoryData> data { get; set; }

        public int CompOffs { get; set; }

        public int MaternityLeaveCount { get; set; }

        public int PaternityLeaveCount { get; set; }

        public int MtpLeaveCount { get; set; }

        public int ElectionHolidayLeaveCount { get; set; }
    }

    public class EmployeeAdminHistoryData
    {
        public int ID { get; set; }

        public Nullable<System.DateTime> FromDate { get; set; }

        public Nullable<System.DateTime> ToDate { get; set; }

        public int ActionTypeID { get; set; }

        public string ActionType { get; set; }

        public Nullable<int> Quantity { get; set; }

        public string Narration { get; set; }        

        public Nullable<System.DateTime> CreatedDate { get; set; }

        public Nullable<int> LeaveType { get; set; }

        public bool AppliedByHR { get; set; }
    }

    public class AdminSearchResult
    {
        public EmployeeBasicProfile BasicProfile { get; set; }

        public EmployeeAdminHistory EmployeeAdminHistory { get; set; }
    }
}
