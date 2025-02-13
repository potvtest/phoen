using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
 
namespace Pheonix.Models.VM.Interfaces.Employee
{
    interface ICorrectionEmployee
    {
        int reportingTo { get; set; }
        int id { get; set; }
        string firstName { get; set; }
        string middleName { get; set; }
        string lastName { get; set; }
    }
}
