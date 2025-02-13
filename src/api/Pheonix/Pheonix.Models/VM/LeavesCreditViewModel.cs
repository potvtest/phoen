using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.VM
{
    public class LeavesCreditViewModel
    {
        public int ID { get; set; }
        public int PersonID { get; set; }
        public int CreditBalance { get; set; }
        public int CreditedBy { get; set; }
        public string Narration { get; set; }
        public int Year { get; set; }
        public System.DateTime DateEffective { get; set; }
    }
}
