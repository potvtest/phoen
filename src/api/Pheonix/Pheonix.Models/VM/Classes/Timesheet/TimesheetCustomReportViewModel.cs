using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.VM.Classes.Timesheet
{
    public class TimesheetCustomReportViewModel
    {
        public Nullable<int> DuId { get; set; }
        public string DuName { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public string ProjectCode { get; set; }
        public int ProjectId { get; set; }
        public Nullable<int> SubProjectId { get; set; }
        public string ProjectName { get; set; }
        public string SubProjectName { get; set; }
        public Nullable<bool> Billable { get; set; }
        public Nullable<int> TaskTypeId { get; set; }
        public string TaskType { get; set; }
        public string TicketId { get; set; }
        public System.DateTime Date { get; set; }
        public string BillableHours { get; set; }
        public string BillableDescription { get; set; }
        public string NonBillableHours { get; set; }
        public string NonBillableDescription { get; set; }
        public Nullable<decimal> EmployeeBillableHours { get; set; }
        public Nullable<decimal> EmployeeNonBillableHours { get; set; }
        public Nullable<decimal> TotalBillableHours { get; set; }
        public Nullable<decimal> TotalNonBillableHours { get; set; }
        public Nullable<int> ProjectManager { get; set; }
        public string ProjectManagerName { get; set; }
        public Nullable<int> SubProjectManager { get; set; }
        public string SubProjectManagerName { get; set; }
        public Nullable<int> DeliveryManager { get; set; }
        public int OrgReportingManager { get; set; }
        public Nullable<int> RmgManager { get; set; }
        public string DeliveryManagerName { get; set; }
        public string OrgReportingManagerName { get; set; }
        public string RmgManagerName { get; set; }
    }
}
