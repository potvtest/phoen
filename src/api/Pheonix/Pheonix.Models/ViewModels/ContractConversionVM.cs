using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.ViewModels
{
    public class ContractConversionVM
    {
        public int NewPersonID { get; set; }
        public int OldPersonID { get; set; }
        public bool ISUS { get; set; }
        public bool ISContractConverion { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string EmployeeType { get; set; }
        public Nullable<int> OfficeLocation { get; set; }
        public string OrganizationEmail { get; set; }
        public Nullable<int> EmploymentStatus { get; set; }
        public Nullable<System.DateTime> JoiningDate { get; set; }
        public Nullable<int> DesignationID { get; set; }
        public Nullable<System.DateTime> ProbationReviewDate { get; set; }
        public Nullable<bool> RejoinedWithinYear { get; set; }
        public Nullable<System.DateTime> OldPersonExitDate { get; set; }
        public Nullable<int> DeliveryUnit { get; set; }
        public int ReportingTo { get; set; }
        public string ReportingManager { get; set; }
        public Nullable<int> ExitProcessManager { get; set; }
        public Nullable<int> WorkLocation { get; set; }
        string WLText { get; set; }
        List<int> CandidateCCRecipients { get; set; }
    }
}
