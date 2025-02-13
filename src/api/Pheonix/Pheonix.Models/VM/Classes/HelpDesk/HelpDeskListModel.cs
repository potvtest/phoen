using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.VM.Classes.HelpDesk
{
    public class HelpDeskListModel
    {
        public int ID { get; set; }
        public string RequestedBy { get; set; }
        public int CategoryID { get; set; }
        public string Number { get; set; }
        public string Type { get; set; }
        public string Severity { get; set; }
        public System.DateTime IssueDate { get; set; }
        public int AssignedTo { get; set; }
        public string AssignedToName { get; set; }
        public int Status { get; set; }
        public int? Duration { get; set; }
        public string Description { get; set; }
        public string Comments { get; set; }
        public int CategoryId { get; set; }
        public bool IsPokeEnabled { get; set; }
        public EmployeeBasicProfile EmployeeProfile { get; set; }
        public DateTime? ResolvedDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
