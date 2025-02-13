using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.VM.Classes.Candidate
{
    public class CandidateEmploymentHistoryViewModel
    {
        public int ID { get; set; }
        public string OrganisationName { get; set; }
        public string Location { get; set; }
        public Nullable<System.DateTime> JoiningDate { get; set; }
        public Nullable<System.DateTime> WorkedTill { get; set; }
        public string EmploymentType { get; set; }
        public string LastDesignation { get; set; }
        public string RoleDescription { get; set; }
        public int CandidateID { get; set; }
        public Nullable<bool> IsDeleted { get; set; }
    }
}
