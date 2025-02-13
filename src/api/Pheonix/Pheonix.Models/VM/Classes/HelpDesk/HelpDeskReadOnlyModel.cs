using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.VM.Classes.HelpDesk
{
    public class HelpDeskReadOnlyModel
    {
        public int ID { get; set; }
        public string Number { get; set; }
        public System.DateTime IssueDate { get; set; }
        public string Category { get; set; }
        public string SubCategory { get; set; }
        public string Type { get; set; }
        public string Severity { get; set; }
        public int Status { get; set; }
        public int Duration { get; set; }
        public string Comments { get; set; }
        public string ReportingTo { get; set; }
        public string PhoneExtension { get; set; }
        public string SeatingLocation { get; set; }
        public DateTime RequiredTill { get; set; }
        public Nullable<DateTime> PokedDate { get; set; }
        public bool IsPokeEnabled { get; set; }
        public int AssignedTo { get; set; }
    }

    public enum HelpDeskType
    {
        Request,
        Issue
    }

    public enum HelpDeskSeverity
    {
        High,
        Medium,
        Low
    }

    public enum HelpDeskCategoriesList
    {
        AD,
        FI,
        HR,
        IT,
        AM,
        RMG,
        PMO,
        IS,
        VR
    }
}
