using Pheonix.Models;
using Pheonix.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Pheonix.Core.v1.Services;
using Pheonix.Core.v1.Services.Email;

namespace Pheonix.Core.Repository.SeparationCard
{
    public interface ISeparationCardRepository
    {
        #region Resignation
        SeperationViewModel Add(SeperationViewModel model, int UserId, Boolean isTerminate = false);
        ActionResult Update(SeperationViewModel model);
        // ActionResult Delete(int id);
        int GetNoticePeriod(int id);
        IEnumerable<SeperationViewModel> GetEmpSeperationDetl(int empId);
        #endregion

        #region Manager Approval
        ActionResult Approve(int id, DateTime approvalDate, int userID, int personID, Boolean IsHR, string ExitDateRemark);
        ActionResult Reject(int id, int userID, int personID);
        IEnumerable<SeperationViewModel> GetSeperationList(int userId, Boolean isHR);
        #endregion

        #region Separation Job
        Task<IEnumerable<SeperationViewModel>> GetSeperations();
        SeperationViewModel GetSeperationsById(int SeprationId);
        //IEnumerable<SeperationViewModel> GetSeperations();

        Task<ActionResult> AddSeperationCheckList(SeperationViewModel model, IBasicOperationsService basicOperation = null, IEmailService _EmailSendingService = null); //Need to test on 18/01/2018
        List<SeperationTerminationViewModel> AddCheckListForHRTermination(SeperationViewModel model, IBasicOperationsService basicOpsService = null, IEmailService _EmailSendingService = null);
        //Task<ActionResult> AddSeperationCheckList(SeperationViewModel model);
        bool SendSCNotice(List<SeperationTerminationViewModel> model);
        #endregion

        #region Separation Process
        Task<SeperationProcessDetailsViewModel> GetSeperationProcessDetails(string roles, int personID, int isHistory, int? year, int? separationMode, bool isSelfView = false);
        ActionResult CompleteSeperationProcess(SeperationConfigProcessViewModel model, int userId);
        Task<SeperationProcessDetailsViewModel> GetSeperationProcessDetailsForDeptAdmin(string roles, int personID, bool isSelfView = false);
        #endregion

        #region Printing
        Task<HttpResponseMessage> GenerateDocument(int separationId, int letterType, int UserId, string fileType = "");
        #endregion

        #region Employee Termination
        ActionResult EmployeeInactive(int personId, int userID, SeperationViewModel _SeperationViewModel = null);
        #endregion

        #region Withdrawal Process
        ActionResult WithdrawalApprove(SeperationViewModel model);
        //ActionResult WithdrawalReject(SeperationViewModel model);
        #endregion

        Task<HttpResponseMessage> GenerateTerminationDocument(int separationId, int letterType, int UserId);

        //ActionResult ExitDateUpdate(ChangeReleaseDateViewModel model, int UserId, int isDateChange);
        SeperationTerminationViewModel ExitDateUpdate(ChangeReleaseDateViewModel model, int UserId, int isDateChange);

        Task<IEnumerable<SeperationViewModel>> GetSeperationListHistory(int userId, Boolean isHR);

        SeperationTerminationViewModel ShowCauseNotice2(EmailTemplateViewModel model, int type, int separationprocessid, int userId);
        //ActionResult ShowCauseNotice2(EmailTemplateViewModel model, int type, int separationprocessid, int userId);

        #region Exit Process Form
        ActionResult AddExitForm(ExitProcessFormDetailViewModel model, int separationID, int userId);
        Task<ExitProcessFormDetailViewModel> GetExitFormData(int separationID);
        #endregion

        Task<IEnumerable<SeperationViewModel>> GetSeperationListForRMG(string roles, int personID);
    }
}
