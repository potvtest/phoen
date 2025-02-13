using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.ViewModels
{
   public class VPCommentsViewModel
    {

        public int id { get; set; }
        public long VPIdeaDetailID { get; set; }
        public int ReviewerId { get; set; }
        public string ReviewerComments { get; set; }
        public Pheonix.Models.VM.EmployeeProfileViewModel SubmittedByDetails { get; set; }

        public DateTime CreatedDate { get; set; }
    }
}
