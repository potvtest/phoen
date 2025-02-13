using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.VM
{
    public interface IEmployeeRole
    {
        int roleId { get; set; }
        string role { get; set; }
    }
}
