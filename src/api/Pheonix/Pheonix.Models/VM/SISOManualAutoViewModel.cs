using System;

namespace Pheonix.Models.VM
{
    public class SISOManualAutoViewModel : IAttendanceManualAuto
    {
        public long Time { get; set; }
        public string Narration { get; set; }
        public bool IsManual { get; set; }
        public bool IsSignIn { get; set; }
        public string TimeZoneName { get; set; }
        public Nullable<System.DateTime> SignInTime { get; set; }
        public Nullable<System.DateTime> SignOutTime { get; set; }
    }
}
