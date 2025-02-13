using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pheonix.Models.VM.Interfaces.Employee;

namespace Pheonix.Models.VM
{
    public class EmploymentStatusList : IEmploymentStatusList
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
