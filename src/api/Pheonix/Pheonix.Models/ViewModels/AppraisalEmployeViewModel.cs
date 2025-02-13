using Pheonix.DBContext;
using Pheonix.Models.VM.Classes.Appraisal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.ViewModels
{
    public class AppraisalEmployeeViewModel
    {
        public int ID { get; set; }
        public int EmpID { get; set; }
        public string EmpName { get; set; }
        public int ReviewerId { get; set; }
        public int AppraiserId { get; set; }
        public string ReviewerName { get; set; }
        public string AppraiserName { get; set; }
        public string Location { get; set; }
        public int LocationId { get; set; }
        public Nullable<int> Grade { get; set; }
        public string FreezedComment { get; set; }
        public string Designation { get; set; }
        public Nullable<int> Status { get; set; }
        public string ReviewerImage { get; set; }
        public string AppraiserImage { get; set; }
    }
}
