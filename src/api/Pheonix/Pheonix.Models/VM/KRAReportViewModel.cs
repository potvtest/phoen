using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.VM
{
    public class KRAReportViewModel
    {
        public Nullable<int> KRACategoryId { get; set; }
        public Nullable<int> PersonId { get; set; }
        public string EmployeeName { get; set; }
        public string KRA { get; set; }
        public string Description { get; set; }
        public Nullable<int> YearId { get; set; }
        public Nullable<bool> IsValid { get; set; }
        public Nullable<bool> Q1 { get; set; }
        public Nullable<bool> Q2 { get; set; }
        public Nullable<bool> Q3 { get; set; }
        public Nullable<bool> Q4 { get; set; }
        public Nullable<bool> IsCloned { get; set; }
        public Nullable<bool> IsKRADone { get; set; }
        public Nullable<System.DateTime> KRADoneOn { get; set; }
        public Nullable<int> KRAPercentageCompletion { get; set; }
        public Nullable<System.DateTime> KRAStartDate { get; set; }
        public Nullable<System.DateTime> KRAEndDate { get; set; }
        public Nullable<int> CreatedBY { get; set; }
        public Nullable<int> ModifiedBy { get; set; }
        public Nullable<int> ReviewerPersonId { get; set; }
        public string ReviewerName { get; set; }
        public Nullable<bool> IsCloneAvailable { get; set; }
    }
}
