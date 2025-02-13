using System;

namespace Pheonix.Models.VM.Interfaces
{
    public interface IPersonSISO
    {
        long SignInSignOutID { get; set; }
        DateTime? SignInTime { get; set; }
        DateTime? SignOutTime { get; set; }
        string SignInComment { get; set; }
        long UserID { get; set; }
        long ApproverID { get; set; }
        string DayNotation { get; set; }
    }
}