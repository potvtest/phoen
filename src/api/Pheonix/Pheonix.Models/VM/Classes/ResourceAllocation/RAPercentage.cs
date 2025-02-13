using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.VM.Classes.ResourceAllocation
{
    public class RAPercentage
    {
        public int PersonID { get; set; }
        public int Percentage { get; set; }
        public int ProjectID { get; set; }
        public Nullable<int> SubProjectID { get; set; }
        public int BillableType { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public Nullable<System.DateTime> ActionDate { get; set; }
        public Nullable<System.DateTime> ReleaseDate { get; set; }
    }
}
