using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.VM
{
    public class DeleletPersonArrearViewModel
    {
        public int id { get; set; }

        public Nullable<int> EmployeeId { get; set; }

        public string actionType { get; set; }

        public Nullable<System.DateTime> fromDate { get; set; }

        public Nullable<System.DateTime> toDate { get; set; }

        public string narration { get; set; }


    }
}
