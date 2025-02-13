using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.VM
{
    interface IEmployeeMyTeam
    {
        int ReportingTo { get; set; }

        IEmployeeBasicProfile EmployeeTeamProfile { get; set; }
    }
}
