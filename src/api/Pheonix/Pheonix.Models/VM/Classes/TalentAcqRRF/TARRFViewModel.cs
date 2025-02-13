using Pheonix.DBContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.VM.Classes.TalentAcqRRF
{
    public  class TARRFViewModel
    {
        public int Id { get; set; }
        public string RRFNo { get; set; }
        public bool IsDraft { get; set; }
        public DateTime RequestDate { get; set; }
        public int DeliveryUnit { get; set; }
        public string DeliveryUnitName { get; set; }
        public int Designation { get; set; }
        public string DesignationName { get; set; }
        public int Position { get; set; }
        public  int EmploymentType { get; set; }
        public string EmpTypeName { get; set; }
        public int MinYrs { get; set; }
        public int MaxYrs { get; set; }
        public int PrimaryApprover { get; set; }
        public string PrimaryApproverName { get; set; }
        public string PrimaryApproverComments { get; set; }
        public int JD { get; set; }
        public string JDLink { get;set; }
        public string JDTitle { get; set; }
        public string RequestorComments { get; set; }
        public int Requestor { get; set; }
        public int HRApprover { get; set; }
        public int SLA { get; set; }
        public string HRApproverComments { get; set; }
        public int CreatedBy { get; set; }
        public System.DateTime? CreatedDate { get; set; }
        public int ModifiedBy { get; set; }
        public System.DateTime? ModifiedDate { get; set; }
        public int RRFStatus { get; set; }
        public List<int> SkillIds { get; set; }
        public string SkillName { get; set; }
        public List<string> SkillsName { get; set; }
        public List<int> InterviewerIds { get; set; }
        public string InterviewerName { get; set; }
        public List<string> InterviewersName { get; set; }

        public DateTime? ExpectedClosureDate { get; set; }
        public IEnumerable<TARRFDetailViewModel> ReqDetails { get; set; }
        public EmployeeBasicProfile EmployeeProfile { get; set; }

    }

    
}
