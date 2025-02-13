using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.VM.Classes
{
    public class ApprovalDetailViewModel
    {
        public int id { get; set; }
        public int approvalID { get; set; }
        public int approverID { get; set; }
        public int stage { get; set; }
        public int status { get; set; }
        public string statusComment { get; set; }
        public DateTime approvalDate { get; set; }
        public bool isDeleted { get; set; }
    }
}
