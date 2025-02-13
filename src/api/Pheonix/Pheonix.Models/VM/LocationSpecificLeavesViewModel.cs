using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pheonix.Models.VM.Interfaces;

namespace Pheonix.Models.VM
{
    public class LocationSpecificLeavesViewModel : ILocationSpecificLeaves
    {
        public int ID { get; set; }

        public string OfficeLocation { get; set; }

        public bool? IsLeaveApplicable { get; set; }

        public bool? IsCasualLeaveApplicable { get; set; }

        public bool? IsSickLeaveApplicable { get; set; }

        public bool? IsSpecialFloatingHolidayApplicable { get; set; }

        public bool? IsMaternityLeaveApplicable { get; set; }

        public bool? IsPaternityLeaveApplicable { get; set; }

    }
}
