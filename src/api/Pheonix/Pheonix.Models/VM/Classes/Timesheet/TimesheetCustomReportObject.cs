using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.VM.Classes.Timesheet
{
    public class TimesheetCustomReportObject
    {
        public int? type { get; set; }
        public int? duID { get; set; }
        public int? projectID { get; set; }
        public int? subProjectID { get; set; }
        public int? employeeID { get; set; }
        public int? billable { get; set; }
        public DateTime startDate { get; set; }
        public DateTime endDate { get; set; }
    }
}
