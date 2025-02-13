using System;

namespace Pheonix.Models.VM
{
    public class SISOAttendanceRptModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int FilterType { get; set; }
        public int EmployeeId { get; set; }
        public int ManagerId { get; set; }
        public int DeliveryUnitId { get; set; }
        public int DeliveryTeamId { get; set; }
        public int WorkLocationId { get; set; }
        public int OfficeLocationId { get; set; }
    }
}
