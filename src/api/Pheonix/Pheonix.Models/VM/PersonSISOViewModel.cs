using System;
using Pheonix.Models.VM.Interfaces;

namespace Pheonix.Models.VM
{
    public class PersonSISOViewModel : IPersonSISO
    {
        public long SignInSignOutID { get; set; }
        public DateTime? SignInTime { get; set; }
        public DateTime? SignOutTime { get; set; }
        public string SignInComment { get; set; }
        public long UserID { get; set; }
        public long ApproverID { get; set; }
        public string DayNotation { get; set; }
        public EmployeeBasicProfile EmployeeProfile { get; set; }
    }
}