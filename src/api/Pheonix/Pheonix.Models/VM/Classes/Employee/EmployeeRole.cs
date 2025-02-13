using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.VM
{
    public class EmployeeRole : IEmployeeRole
    {
        public int roleId
        {
            get;
            set;
        }

        public string role
        {
            get;
            set;
        }
    }
}
