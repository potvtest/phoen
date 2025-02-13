using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.VM
{
    public class EmployeeOrganizaionDetails : IEmployeeOrganizationDetails
    {
        public int ID { get; set; }
        public string OrgUnit { get; set; }
        public int DeliveryUnit { get; set; }
        public int CurrentDU { get; set; }
        public int DeliveryTeam { get; set; }
        public int ResourcePool { get; set; }
        public int ReportingTo { get; set; }
        public string ReportingManager { get; set; }
        public int ExitProcessManager { get; set; }
        public string TimeZone { get; set; }
        public int EmploymentStatus { get; set; }
        //public int? WL { get; set; }

        private string _WLText = null;
        public string WLText
        {
            get
            {
                return _WLText;
            }
            set
            {
                _WLText = value ?? null;
            }
        }
        public Nullable<int> WorkLocation { get; set; }
    }
}
