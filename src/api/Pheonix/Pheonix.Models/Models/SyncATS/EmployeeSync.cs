using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models
{
    public class EmployeeSync
    {
        public int ApplicationId { get; set; }
        public DateTime ApplicationJoiningDate { get; set; }
        public int ApplicationCurrentStepId { get; set; }
        public int ApplicationRejectionFromStepId { get; set; }
        public DateTime applicationLastStatusChangeDate { get; set; }
        public int requisitionStatusId { get; set; }
        public int requisitionId { get; set; }
        public DateTime requisitionLastStatusChangeDate { get; set; }
        public string applicationOfferAcceptanceStatus { get; set; }
    }

}
