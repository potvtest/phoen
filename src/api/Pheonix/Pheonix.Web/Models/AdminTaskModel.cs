using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Pheonix.Web.Models
{
    public class AdminTaskModel
    {
        public int EmployeeID { get; set; }

        public DateTime FromDate { get; set; }

        public DateTime ToDate { get; set; }

        public string Comments { get; set; }

        public string SubType { get; set; }

        public int Quantity { get; set; }

        public int Validated { get; set; }

        public int locationID { get; set; }
    }
}