using System;

namespace Pheonix.Models.VM
{
    interface IEmployeeSISO 
    {
        long Id { get; set; }
        DateTime Date { get; set; }
        TimeSpan SignInTime { get; set; }
        TimeSpan SignOutTime { get; set; }
        string TotalHoursWorked { get; set; }
        string Narration { get; set; }
        bool IsManual { get; set; }
        DateTime AttendanceDate { get; set; }
        int StatusID { get; set; }
        string ApproverComments { get; set; }
    }
}