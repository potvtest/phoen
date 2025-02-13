using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.VM.Classes.Candidate
{
    public class CandidateViewModel
    {
        public int ID { get; set; }
        public int OldPersonID { get; set; }
        public int LoggedInUserID { get; set; }
        public bool ISUS { get; set; }
        public bool ISContractConverion { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public Nullable<System.DateTime> DateOfBirth { get; set; }
        public Nullable<int> Gender { get; set; }
        public string Salutation { get; set; }
        public string EmployeeType { get; set; }
        public  Nullable<int> RrfRequestor { get; set; }
        public int OfficeLocation { get; set; }
        public string OrganizationEmail { get; set; }
        public Nullable<int> EmploymentStatus { get; set; }
        public Nullable<System.DateTime> JoiningDate { get; set; }
        public Nullable<int> DesignationID { get; set; }
        public Nullable<System.DateTime> ProbationReviewDate { get; set; }
        public Nullable<bool> RejoinedWithinYear { get; set; }
        public int CandidateStatus { get; set; }
        public Nullable<int> SourceType { get; set; }
        public string SourceName { get; set; }
        public Nullable<int> NoticePeriod { get; set; }
        public Nullable<int> ExperienceYears { get; set; }
        public Nullable<int> ExperienceMonths { get; set; }
        public Nullable<int> ReleventExperienceYears { get; set; }
        public Nullable<int> ReleventExperienceMonths { get; set; }
        public Nullable<decimal> CurrentCTC { get; set; }
        public string Reason { get; set; }
        public Nullable<bool> IsDeleted { get; set; }
        public string Image { get; set; }
        public string ResumePath { get; set; }
        public string PanNumber { get; set; }
        public string PfNumber { get; set; }
        public string RrfNumber { get; set; }
        public int RrfStatus { get; set; }
        public string OrgUnit { get; set; }
        public Nullable<int> DeliveryUnit { get; set; }
        public int CurrentDU { get; set; }
        public int DeliveryTeam { get; set; }
        public int ResourcePool { get; set; }
        public int ReportingTo { get; set; }
        public string ReportingManager { get; set; }
        public int ExitProcessManager { get; set; }
        //int? WL { get; set; }
        public Nullable<int> WorkLocation { get; set; }
        string WLText { get; set; }
        public List<CandidateAddressViewModel> CandidateAddressVM { get; set; }
        public List<CandidateCertificationViewModel> CandidateCertificationVM { get; set; }
        public List<CandidateEmploymentHistoryViewModel> CandidateEmploymentHistoryVM { get; set; }
        public List<CandidatePassportViewModel> CandidatePassportVM { get; set; }
        public CandidatePersonalViewModel CandidatePersonalVM { get; set; }
        public List<CandidateQualificationMappingViewModel> CandidateQualificationMappingVM { get; set; }
        public List<CandidateSkillMappingViewModel> CandidateSkillMappingVM { get; set; }
        public List<int> CandidateCCRecipients { get; set; }
    }
}
