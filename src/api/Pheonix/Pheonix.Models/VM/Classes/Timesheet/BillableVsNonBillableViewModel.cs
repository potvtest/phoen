using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.VM.Classes.Timesheet
{
    public class BillableVsNonBillableViewModel
    {
        public Nullable<int> CustomerID { get; set; }
        public string CustomerName { get; set; }
        public Nullable<int> ProjectID { get; set; }
        public string ProjectName { get; set; }
        public Nullable<decimal> TotalHoursAvailable { get; set; }
        public Nullable<decimal> TotalEnteredBillableHours { get; set; }
        public Nullable<decimal> TotalEnteredNonBillableHours { get; set; }
        public Nullable<decimal> TotalMissingHours { get; set; }
    }
}
