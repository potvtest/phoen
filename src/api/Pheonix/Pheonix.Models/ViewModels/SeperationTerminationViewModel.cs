using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.ViewModels
{
    public class SeperationTerminationViewModel
    {
        public string SeperationReason { get; set; }
        public string Template { get; set; }
        public int Id { get; set; }
        public int SeparationID { get; set; }
        public int ShowCauseNoticeType { get; set; }
        public DateTime EmailReceivedOn { get; set; }
        public DateTime ShowCauseNoticeSendOn { get; set; }
        public string Reason { get; set; }
        public string EmailFrom { get; set; }
        public string EmailTo { get; set; }
        public string EmailCC { get; set; }
        public string Subject { get; set; }        
        public string message { get; set; }        
        public bool isActionPerformed { get; set; }
    }
}
