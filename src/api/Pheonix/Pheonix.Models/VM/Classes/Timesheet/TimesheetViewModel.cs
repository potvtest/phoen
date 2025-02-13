using Pheonix.DBContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.VM.Classes.Timesheet
{
    public class TimesheetViewModel
    {
        public int ID { get; set; }

        public int PersonID { get; set; }

        public string Description { get; set; }

        public string NonBillableDescription { get; set; }

        public string TicketID { get; set; }

        public string TotalHours { get; set; }

        public string NonBillableTime { get; set; }

        public int ProjectID { get; set; }

        public string ProjectCode { get; set; }

        public int? SubProjectID { get; set; }

        public string SubProjectCode { get; set; }

        public Nullable<bool> IsEmailSent { get; set; }

        public System.DateTime Date { get; set; }

        public bool IsDeleted { get; set; }

        public System.DateTime CreatedDate { get; set; }

        public string ProjectName { get; set; }

        public string SubProjectName { get; set; }

        public int? TaskTypeId { get; set; }

        public int? SubTaskId { get; set; }

        public string SubTaskType { get; set; }

        public string TaskType { get; set; }

        public string startdate { get; set; }

        public string filename { get; set; }

        public string enddate { get; set; }

        public string JsonString { get; set; }

        public string EmployeeName { get; set; }

        public string BillableHours { get; set; }

        public string BillableDescription { get; set; }

        public string ProjectManager { get; set; }

        public string SubProjectManager { get; set; }

        public string DeliveryManager { get; set; }

        public string ReportingTo { get; set; }

        public string DeliveryUnitName { get; set; }

        public string RMGManager { get; set; }

        public string BillableHoursInNumber { get; set; }

        public string NonBillableHoursInNumber { get; set; }

        public Nullable<decimal> HourSum { get; set; }

    }
}
