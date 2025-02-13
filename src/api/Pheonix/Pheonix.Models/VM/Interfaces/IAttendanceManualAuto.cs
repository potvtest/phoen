using System;


namespace Pheonix.Models.VM
{
    public interface IAttendanceManualAuto
    {
        long Time { get; set; }

        //long SignOutTime { get; set; }

        //DateTime LastModifiedDate { get; set; }

        string Narration { get; set; }

        bool IsManual { get; set; }

        bool IsSignIn { get; set; }
    }
}
