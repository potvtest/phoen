using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.Models.Admin
{
    public class AdminActionModel
    {
        public int ID { get; set; }

        public int EmployeeID { get; set; }

        public DateTime From { get; set; }

        public DateTime To { get; set; }

        public string Comments { get; set; }

        public int Quantity { get; set; }

        public int AdminID { get; set; }

        public string SubType { get; set; }

        public AdminTaskType ActionType { get; set; }

        public int Count { get; set; }

        public int SubTypeID { get; set; }

        public Nullable<int> LeaveType { get; set; }

        public int Validated { get; set; }

        public int LocationID { get; set; }
    }


    public enum AdminTaskType
    {        
        Leaves,
        CompOff,
        SignInSignOut,
        BulkSISO
    }

   

    public class AdminActionResult
    {
        public string message;
        public bool isActionPerformed = false;
    }
}
