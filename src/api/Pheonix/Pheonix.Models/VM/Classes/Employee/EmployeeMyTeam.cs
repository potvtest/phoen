using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.VM
{
    public class EmployeeMyTeam : IEmployeeMyTeam
    {
        public int ReportingTo { get; set; }
        public Dictionary<string, dynamic> EmployementStatus { get; set; }

        public IEmployeeBasicProfile EmployeeTeamProfile { get; set; }
    }
}
