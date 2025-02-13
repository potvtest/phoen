using Pheonix.Models.VM.Interfaces.Employee;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.VM.Classes.Employee
{
    public class CorrectionEmployee: ICorrectionEmployee
    {
        public int reportingTo { get; set; }
        public int id { get; set; }
        public string firstName { get; set; }
        public string middleName { get; set; }
        public string lastName { get; set; }
    }
}
