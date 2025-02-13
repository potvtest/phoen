using System;
using System.Collections.Generic;

namespace Pheonix.Models.VM
{
    public class EmployeeSISOViewModel : IEmployeeSISO
    {
        public long Id { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan SignInTime { get; set; }
        public TimeSpan SignOutTime { get; set; }
        public string DayNotation { get; set; }
        public string TotalHoursWorked { get; set; }
        public string Narration { get; set; }
        public bool IsManual { get; set; }
        public DateTime AttendanceDate { get; set; }       
        public EmployeeBasicProfile EmployeeProfile { get; set; }
        public int StatusID { get; set; }
        public string ApproverComments { get; set; }
        public int UserID { get; set; }
        public long ApproverID { get; set; }
        public string SignInSignOutDate { get; set; }
    }

    public class AttendanceViewModel
    {
        public IEnumerable<EmployeeSISOViewModel> EmployeeSISOViewModels { get; set; }
        public TodayStatus TodayStatus { get; set; }
        public IEnumerable<HolidayListViewModel> CurrentMonthHolidays { get; set; }
        public int HolidayCount { get; set; }
        public int LeaveCount { get; set; }
        public int PresentCount { get; set; }
        public int PreviousLeaveCount { get; set; }
        public int AbsentCount { get; set; }
        public List<DateTime> DateOfLeave { get; set; }
        public DateTime JoiningDate { get; set; }
    }

    public class TodayStatus
    {
        public bool IsSignIn { get; set; }
        public bool IsSignOut { get; set; }
        public bool IsOnleaveToday { get; set; }
    }
}