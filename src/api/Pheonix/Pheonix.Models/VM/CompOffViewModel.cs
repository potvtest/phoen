using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.VM
{
    public class CompOffViewModel
    {
        public int ID { get; set; }
        public Nullable<int> PersonID { get; set; }
        public Nullable<System.DateTime> ForDate { get; set; }
        public Nullable<System.DateTime> ExpiresOn { get; set; }
        public Nullable<int> Year { get; set; }
        public Nullable<bool> IsApplied { get; set; }
        public int Status { get; set; }
        public Nullable<int> LeaveRequestID { get; set; }
        public string Narration { get; set; }
        public Nullable<int> ByUser { get; set; }
        public Nullable<System.DateTime> OnDate { get; set; }
    }
}
