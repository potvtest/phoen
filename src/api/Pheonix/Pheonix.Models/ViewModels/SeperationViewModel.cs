using Pheonix.Models.VM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.ViewModels
{
    public class SeperationViewModel : IViewModel
    {
        public int ID { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public int UpdatedBy { get; set; }
        public DateTime UpdatedOn { get; set; }
        public DateTime ResignDate { get; set; }
        public DateTime ExpectedDate { get; set; }
        public DateTime ActualDate { get; set; }
        public string SeperationReason { get; set; }
        public string Comments { get; set; }
        public int ApprovalID { get; set; }
        public DateTime ApprovalDate { get; set; }
        public int StatusID { get; set; }
        public int PersonID { get; set; }
        public EmployeeBasicProfile EmployeeProfile { get; set; }
        public int NoticePeriod { get; set; }
        public string RejectRemark { get; set; }
        public virtual Boolean isApprovedByHR { get; set; }
        public string WithdrawRemark { get; set; }
        public int IsTermination { get; set; }
        public string TerminationRemark { get; set; }
        public int TerminationReason { get; set; }
        public string AttachedFile { get; set; }
        public DateTime ExitDate { get; set; }
        public virtual Boolean isWithdraw { get; set; }
        public int? TotalLeaves { get; set; }
        public int? LeavesTaken { get; set; }
        public int? CompOff { get; set; }
        public int? LeavesAvailable { get; set; }
        public int? LeavesApplied { get; set; }
        public int? CompOffAvailable { get; set; }
        public int? LWP { get; set; }
        public int? CompOffConsumed { get; set; }
        public string ExitDateRemark { get; set; }
        public string EmailID { get; set; }
        public DateTime LWPDate { get; set; }
        public string ResignationWOSettlement { get; set; }
        public int OldEmploymentStatus { get; set; }
    }

    public class SeperationProcessViewModel
    {
        public int ID { get; set; }
        public int RoleID { get; set; }
        public int SeperationID { get; set; }
        public int StatusID { get; set; }
        public string Comments { get; set; }
        public string ChecklistProcessedData { get; set; }
        public int ChecklistAuthorizePersonId { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public int UpdatedBy { get; set; }
        public DateTime UpdatedOn { get; set; }
        public DateTime ApprovalDate { get; set; }
        public string ReportingManager { get; set; }
        public int ReportingManagerID { get; set; }
        //TODO : Ganesh : why this property required.
        public string SeperationReason { get; set; }
        public int TotalStages { get; set; }
        public int OnStageStatus { get; set; }
        public int OnStage { get; set; }
        //TODO : Ganesh : why this property required. Duplicating with above comment field
        public string EmployeeComments { get; set; }
        public EmployeeBasicProfile Employee { get; set; }
        public List<SeperationConfigProcessViewModel> SeperationConfigData { get; set; }
        public int TerminationReason { get; set; }
        public int IsTermination { get; set; }
        public string TerminationRemark { get; set; }
        public int AssignedTo { get; set; }
        public string AttachedFile { get; set; }
        public DateTime ResignDate { get; set; }
        public Boolean IsExitFormFill { get; set; }
        public string ExitDateRemark { get; set; }
    }

    public class SeperationProcessDetailsViewModel
    {
        public List<SeperationProcessViewModel> SeperationProcess { get; set; }
    }

    public class ChangeReleaseDateViewModel
    {
        public int ID { get; set; }
        public int SeparationID { get; set; }
        public int Type { get; set; }
        public string ExitDateRemark { get; set; }
        public DateTime ExitDate { get; set; }
        public DateTime LWPDate { get; set; }
    }

    public class EmailTemplateViewModel
    {
        public int ID { get; set; }
        public int SeparationID { get; set; }
        public int ShowCauseNoticeType { get; set; }
        public string Reason { get; set; }
        public DateTime EmailReceivedOn { get; set; }
        public DateTime ShowCauseNoticeSendOn { get; set; }
    }
}
