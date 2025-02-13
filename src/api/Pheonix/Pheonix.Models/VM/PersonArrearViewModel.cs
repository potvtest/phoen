using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.VM
{
    public class PersonArrearViewModel
    {
     
        public int ArrearID { get; set; }

        public Nullable<int> EmployeeId { get; set; }

        public Nullable<int> LocationID { get; set; }

        public Nullable<System.DateTime> fromDate { get; set; }

        public string comments { get; set; }

        public Nullable<int> CreatedBy { get; set; }

        public Nullable<System.DateTime> CreatedDate { get; set; }

               
    }
}
