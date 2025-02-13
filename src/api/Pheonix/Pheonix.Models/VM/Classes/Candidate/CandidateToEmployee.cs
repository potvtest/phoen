using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.VM.Classes.Candidate
{
    public class CandidateToEmployee
    {
        public int LoggedInUserID { get; set; }
        public string RrfNumber { get; set; }
        public int RrfRequestor { get; set; }
        public int EmployeeCode { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public Nullable<int> EmploymentStatus { get; set; }
        public Nullable<System.DateTime> JoiningDate { get; set; }
        public Nullable<int> DesignationID { get; set; }
        public Nullable<int> RRFRequestedDesignationID { get; set; }        
        public Nullable<int> DeliveryUnit { get; set; }
        public Nullable<System.DateTime> ProbationReviewDate { get; set; }
        public Nullable<bool> RejoinedWithinYear { get; set; }
        public int OfficeLocation { get; set; }
        public string OrganizationEmail { get; set; }
        public Nullable<int> WorkLocation { get; set; }
        public List<CandidateSkillMappingViewModel> CandidateSkillMappingVM { get; set; }
        public string EmployeeType { get; set; }
        public int ReportingTo { get; set; }
        public int ExitProcessManager { get; set; }
        public bool IsRRFDesignationChanged { get; set; }
        public List<int> CandidateCCRecipients { get; set; }
    }
}
