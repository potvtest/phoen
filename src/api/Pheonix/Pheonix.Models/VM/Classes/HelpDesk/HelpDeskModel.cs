using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.VM.Classes.HelpDesk
{
    public class HelpDeskModel
    {
        public int ID { get; set; }
        public string Number { get; set; }
        public int PersonID { get; set; }
        public int CategoryID { get; set; }
        public int SubCategoryID { get; set; }
        public int Type { get; set; }
        public int Severity { get; set; }
        public System.DateTime IssueDate { get; set; }
        public Nullable<int> AssignedTo { get; set; }
        public bool IsApprovalRequired { get; set; }
        public int Status { get; set; }
        public Nullable<int> duration { get; set; }
        public string comments { get; set; }
        public int reportingTo { get; set; }
        public string AttachedFiles { get; set; }
        public DateTime RequiredTill { get; set; }
        public Nullable<DateTime> PokedDate { get; set; }
        public bool IsOtherDepartmant { get; set; }
        public string SeatingLocation { get; set; }
        public string PhoneExtension { get; set; }
    }
}