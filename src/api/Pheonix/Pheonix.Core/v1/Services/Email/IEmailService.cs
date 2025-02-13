using Pheonix.DBContext;
using Pheonix.Models;
using Pheonix.Models.ViewModels;
using Pheonix.Models.VM;
using Pheonix.Models.VM.Classes.Candidate;
using Pheonix.Models.VM.Classes.ResourceAllocation;
using Pheonix.Models.VM.Classes.TalentAcqRRF;
using System.Collections.Generic;

namespace Pheonix.Core.v1.Services.Email
{
    public interface IEmailService
    {
        void SendExpenseApprovalEmail(Expense expense, Person person, ApprovalStage stage, string comments);

        void SendUserProfileApproval(int empID, string empName, string cardName, string from, string to, string imageName);

        void SendUserProfileStatus(int empID, string empName, string cardName, string from, string to, int statusID, string imageName);

        void SendAttendanceApproval(int empID, string empName, string senderEmail, string approverName, string approvalEmail, string startDate, string imageName);

        void SendLeaveApproval(EmailLeaveDetails objEmailLeaveDetails);

        void SendLeaveApprovalStatus(int empID, string empName, string senderEmail, string approverName, string approvalEmail, string startDate, string endDate, int noOfDays, string status, string imageName, int? leaveType); //For: #149624435 - Change done to display leave From date, To date & No. of days

        void SendHelpdeskApproval(string issueId, PersonHelpDesk newHelpDeskTicket, string empName, string senderEmail, string approverName, string approvalEmail, int status, string managerName, string managerEmail, string managerComment);

        void SendAppraisalInitiat(string senderEmail, string sendToEmail);

        void SendTravelApproval(Traveller travel, Person loggedInUser, ApprovalStage stage, string comments);

        void SendResignationEmail(SeperationViewModel model, string subject, string body, string emailFrom, string emailTo, bool isHR, bool isMgr, int CurrStatus, string allCC, string LogInUser);//string grpHeadEmail
        SeperationTerminationViewModel GetResignationEmail(SeperationViewModel model, string subject, string body, string emailFrom, string emailTo, bool isHR, bool isMgr, int CurrStatus, string allCC, string LogInUser);//string grpHeadEmail
        bool SendSCNotice(SeperationTerminationViewModel separation);
        void SendConfirmationEmail(PersonConfirmation confirmation, int editStyle, bool isHR);

        void SendResignationProcessEmails(SeperationViewModel model, string subject, string body, string emailToCC);

        void InitiateReminderEmails(PhoenixEntities context, IList<PersonConfirmation> confirmations);
        void InitiateSeparationReminderEmails(PhoenixEntities context, IList<SeperationProcess> separationprocess);

        void SendExpenseApprovalEmail(Expense_New expense, Person person, ApprovalStage stage, string comments);


        bool SendExpenseReminder(ExpenseMail expReminder);

        bool SendCustomerMails(CustomerMailAction action, Customer customer, int userID);

        bool SendProjectMails(ProjectMailAction action, ProjectList project, int userID);
        bool SendInvoiceMails(InvoiceMailAction action, PMSInvoice model, bool isApprover, int userId);
        bool SendOrgDetailsUpdateStatus(int personId, int id, EmployeeOrganizaionDetails model);

        #region Resource allocation methods
        bool SendResourceAllocationRaisedEmail(RARaisedRequest model, int userId);
        bool SendResourceAllocationUpdatedEmail(RAGetRaisedRequest model, int userId);
        bool SendResourceAllocationActionEmail(RAGetRaisedRequest model, int userId, string comments);

        //bool ResourceAllocationAllocationUpdateEmail(RAGetRaisedRequest model, int userId, string comments);
        bool EmployeeUpdateEmail(RAResource model);
        bool EmployeeReleaseEmail(RAResource model);
        //bool SendResourceAllocationReleaseEmail(RAGetRaisedRequest model, int userId, string comments);
        bool SendResourceAllocationActionStatusEmail(int userId, int RequestedBy, int requestId, int requestType, int requestStatus, string comments);
        #endregion

        #region RRF
        bool SendNewEmployeeEmail(CandidateToEmployee candidateToEmployee, TARRF modelTARRF = null, TARRFDetail modelTARRFDetail = null);
        bool SendNewRRFRequestEmail(TARRFViewModel model);
        bool SendApproveRRFEmail(TARRFViewModel model);
        bool SendRejectRRFEmail(TARRFViewModel model);
        bool SendApprovedRRFHREmail(TARRFViewModel model);
        bool SendCanceledRRFHREmail(TARRFDetailViewModel model, TARRF modeltarrf);
        bool RrfSwapNotification(int sender, int admin, string rrfList);
        #endregion
        bool SendContractCompletionEmail(CandidateViewModel model, int nextPersonID);
        bool SendBGVerificationToHR(RAResource model, List<int> RequiredBGCList);
        //bool SendBGVerificationToRMG(int StatusBy, int ProjectID, int PersonID);

        void SendValuePortalCommentUpdate(int senderEmpId, int receiverEmpId, VCIdeaMasterViewModel ideaViewModel, string reviewerComments);
        //void SendValuePortalCommentUpdate(string RequiredEffort, string SenderName, string SenderEmail, string SenderEmpId, string ReceiverName, string ReceiverEmail, string ReceiverEmpId, string Comment, string IdeaID, string IdeaHeadline, string IdeaDescription, string IdeaBenefits);
        void SendValuePortalIdeaSubmitted(VPSubmittedIdeaViewModel vpSubmittedIdeaViewModel, VCIdeaMasterViewModel ideaVM);
        //void SendValuePortalIdeaUpdate(string SenderName, string SenderEmail, int SenderEmpId, string ReceiverName, string ReceiverEmail, int ReceiverEmpId, string[] DirtyValuesList);
        void SendValuePortalIdeaUpdate(VCIdeaMasterViewModel ideaVM, int SenderEmpId, int ReceiverEmpId, bool isGlobalApprover, bool isBUApprover, string[] DirtyValuesList);
        List<int> getListOfGloblaAndBUApprovers(bool isBUApprover, int bussinessUnitId);
        string getEmpEmailIDsFromEmpIDList(List<int> employeeIDList);
        void SendVCFPIIdeaUpdate(VCIdeaMasterViewModel ideaVM, int SenderEmpId, int ReceiverEmpId, bool isGlobalApprover, bool isBUApprover, string[] DirtyValuesList);
        void sendEmailOnVCFPhaseISubmit(VPSubmittedIdeaViewModel vpSubmittedIdeaViewModel, VCIdeaMasterViewModel ideaVM);
        void SendLeaveCancellationStatus(int empID, string empName, string senderEmail,string startDate, string endDate, int noOfDays, string Narration, string status, string imageName, int? LeaveType);
    }
}
