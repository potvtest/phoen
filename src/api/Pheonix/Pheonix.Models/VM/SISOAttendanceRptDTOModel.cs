using System;

namespace Pheonix.Models.VM
{
    public class SISOAttendanceRptDTOModel
    {
        public long EmployeeCode { get; set; }
        public string EmployeeName { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public TimeSpan SignInTime { get; set; }
        public TimeSpan SignOutTime { get; set; }
        public string TotalHoursWorked { get; set; }
        public string Status { get; set; }
        public int ManagerCode { get; set; }
        public string ManagerName { get; set; }
        public string DeliveryTeamName { get; set; }
        public string DeliveryUnitName { get; set; }
        public string OrgUnit { get; set; }
        public string WorkLocation { get; set; }
        public string OfficeLocation { get; set; }
    }
}