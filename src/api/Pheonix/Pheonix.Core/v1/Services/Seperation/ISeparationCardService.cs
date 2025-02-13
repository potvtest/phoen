using Pheonix.Models;
using Pheonix.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Pheonix.DBContext.Repository;
using Pheonix.Models.VM;

namespace Pheonix.Core.v1.Services.Seperation
{
    public interface ISeparationCardService
    {
        int UserId { get; set; }
        #region Resignation
        Task<ActionResult> Add(SeperationViewModel model);
        ActionResult Update(SeperationViewModel model);
        //ActionResult Delete(int id);
        int GetNoticePeriod(int id);
        IEnumerable<SeperationViewModel> GetEmpSeperationDetl(int empId);
        #endregion

        #region Manager Approval
        ActionResult Approve(SeperationViewModel model, Boolean IsHR);
        ActionResult Reject(SeperationViewModel model);
        IEnumerable<SeperationViewModel> GetSeperationList(Boolean isHR);
        #endregion

        #region Separation Job
        Task<bool> InitiateSeparationProcess(int SeparationID = 0, string discussionDt = "");
        #endregion

        #region Separation Process
        Task<SeperationProcessDetailsViewModel> GetSeperationProcessDetails(string roles, int personID, int isHistory, int? year,int? separationMode, bool isSelfView = false);
        ActionResult CompleteSeperationProcess(SeperationConfigProcessViewModel model, int userID);
        Task<SeperationProcessDetailsViewModel> GetSeperationProcessDetailsForDeptAdmin(string roles, int personID, bool isSelfView = false);
        #endregion

        #region Printing
        Task<HttpResponseMessage> GenerateDocument(int separationId, int letterType, int UserId, string fileType = "");
        #endregion

        #region Employee Termination
        //ActionResult TerminateEmployee(SeperationViewModel model, int UserId);
        List<SeperationTerminationViewModel> TerminateEmployee(SeperationViewModel model, int UserId);
        bool SendSCNotice(List<SeperationTerminationViewModel> model);
        ActionResult EmployeeInactive(EmployeeTerminationViewModel model);
        #endregion

        #region Withdrawal Process
        ActionResult WithdrawalApprove(SeperationViewModel model);
        ActionResult WithdrawalReject(SeperationViewModel model);
        #endregion

        Task<HttpResponseMessage> GenerateTerminationDocument(int separationId, int letterType, int UserId);
        Task<HttpResponseMessage> DownloadZip(int personId);

        //ActionResult ExitDateUpdate(ChangeReleaseDateViewModel model, int UserId, int isDateChange);
        SeperationTerminationViewModel ExitDateUpdate(ChangeReleaseDateViewModel model, int UserId, int isDateChange);

        Task<int> GetEmploymentStatus(int PersonId);

        Task<IEnumerable<SeperationViewModel>> GetSeperationListHistory(int userId, Boolean isHR);
        SeperationTerminationViewModel ShowCauseNotice2(EmailTemplateViewModel model, int type, int separationprocessid, int userId);

        //ActionResult ShowCauseNotice2(EmailTemplateViewModel model, int type, int separationprocessid, int userId);
        Task<object> SendSeparationReminderMail();

        #region Exit Process Form
        ActionResult AddExitForm(ExitProcessFormDetailViewModel model, int separationID);
        Task<ExitProcessFormDetailViewModel> GetExitFormData(int separationID);
        #endregion

        Task<string> GetPersonalEmailID(int PersonId);
        Task<IEnumerable<SeperationViewModel>> GetSeperationListForRMG(string roles, int personID);
        ContractConversionVM GetContractConversionData(int PersonId);
    }

    public interface IPrintSeparationReportInPDF
    {
        Task<HttpResponseMessage> GetSeparationPDFPrint(IContextRepository _repo, SeperationViewModel separation, string reportName, string fileType = "");
        Task<HttpResponseMessage> GetTerminationPDFPrint(IContextRepository _repo, int personID, string reportName);
        Task<HttpResponseMessage> GetSeparationDOCPrint(IContextRepository _repo, SeperationViewModel separation, string reportName, string fileType = "");
    }
}
