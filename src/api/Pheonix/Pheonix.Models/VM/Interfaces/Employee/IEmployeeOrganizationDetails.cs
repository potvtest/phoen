using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.VM
{
    public interface IEmployeeOrganizationDetails
    {
        int ID { get; set; }
        string OrgUnit { get; set; }
        int DeliveryUnit { get; set; }
        int CurrentDU { get; set; }
        int DeliveryTeam { get; set; }
        int ResourcePool { get; set; }
        int ReportingTo { get; set; }
        string ReportingManager { get; set; }
        int ExitProcessManager { get; set; }

        //int? WL { get; set; }
        Nullable<int> WorkLocation { get; set; }
        string WLText { get; set; }
    }
}
