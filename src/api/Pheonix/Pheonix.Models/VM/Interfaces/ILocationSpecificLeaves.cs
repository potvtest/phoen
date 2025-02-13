using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.VM.Interfaces
{
    public class ILocationSpecificLeaves
    {
        int ID { get; set; }

        string OfficeLocation { get; set; }

        bool IsLeaveApplicable { get; set; }

        bool IsCasualLeaveApplicable { get; set; }

        bool IsSickLeaveApplicable { get; set; }

        bool IsSpecialFloatingHolidayApplicable { get; set; }

        bool IsMaternityLeaveApplicable { get; set; }

        bool IsPaternityLeaveApplicable { get; set; }

    }
}
