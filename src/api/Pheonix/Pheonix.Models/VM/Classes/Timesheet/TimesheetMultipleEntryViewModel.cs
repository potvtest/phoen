using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.VM.Classes.Timesheet
{
    public class TimesheetMultipleEntryViewModel
    {
        public int ID { get; set; }

        public int PersonID { get; set; }

        public System.DateTime Date { get; set; }

        public int ProjectID { get; set; }

        public int? SubProjectID { get; set; }

        public int? TaskTypeId { get; set; }

        public int? SubTaskId { get; set; }

        public string TicketID { get; set; }

        public string Description { get; set; }

        public string TotalHours { get; set; }

        public string NonBillableDescription { get; set; }

        public string NonBillableTime { get; set; }

        public string UploadType { get; set; }

        public string JsonString { get; set; }

        public System.DateTime CreatedDate { get; set; }

        public Nullable<bool> IsEmailSent { get; set; }

        public bool IsDeleted { get; set; }

    }
}
