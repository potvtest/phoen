using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.VM
{
    public class RRFDetailsViewModels
    {
        public int DetailID { get; set; }
        public int RRFId { get; set; }
        public int DeliveryTeam { get; set; }
        public bool DeliveryUnit { get; set; }
        public int Project { get; set; }
        public string BusinessJustification { get; set; }
        public int EnrollmentType { get; set; }
        public int ReportingTo { get; set; }
        public string BillabilityStatus { get; set; }
        public bool ApprovalStatus { get; set; }
        public int ApprovedBy { get; set; }
        public int Experience { get; set; }
        public int EmploymentType { get; set; }
    }
}