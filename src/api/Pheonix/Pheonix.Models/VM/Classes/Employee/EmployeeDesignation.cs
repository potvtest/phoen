using Pheonix.Models.VM.Interfaces.Employee;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.VM
{
    public class EmployeeDesignation : IEmployeeDesignation
    {
        public int ID
        {
            get;
            set;
        }

        public string Text
        {
            get;
            set;
        }
    }
}
