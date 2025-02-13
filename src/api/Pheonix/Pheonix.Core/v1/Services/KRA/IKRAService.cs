using System.Threading.Tasks;
using System.Collections.Generic;
using Pheonix.DBContext;
using Pheonix.Models.ViewModels;
using Pheonix.Models.VM;
using System;

namespace Pheonix.Core.v1.Services.KRA
{
    public interface IKRAService
    {
        Task<IDictionary<string, IEnumerable<DropdownItems>>> GetCategoryDropdown();
        Task<IDictionary<string, IEnumerable<DropdownItems>>> GetGradeList();
        Task<List<KRACycleConfiguration>> GetCycleConfiguration(int personId, int[] yearIds);
        Task<IEnumerable<GetKRADetails_Result>> GetDetails(int id, int personId);
        Task<IEnumerable<object>> GetActiveByPersonId(int personId, int[] yearIds);
        Task<bool> AddUpdateDetails(int userId, bool isCloned, PersonKRADetailViewModel model);
        Task<bool> CloneHistoryDetails(int userId, PersonKRADetailViewModel viewModel);
        Task<IEnumerable<ValidateDetailsToCloneKRA_Result>> ValidateCloneFromHistory(int PersonId);
        Task<IEnumerable<KRAProgressViewModel>> GetProgressList(int kraGoalId);
        Task<IEnumerable<KRAFileAttachment>> GetAttachment(int kraGoalId);
        Task<bool> SaveOrUpdateProgress(KRAProgressViewModel viewModel, int userId);
        Task<bool> UpdateProgressBar(int kraGoalId, int modifiedBy);
        Task<bool> DeleteProgressEntry(KRAProgressViewModel model);
        Task<IEnumerable<KRAAllEmployeesViewModel>> GetInitiationEmployeesList(string initListType, int year);
        Task<bool> InitiatePerson(IEnumerable<PersonKRAViewModel> viewModel);
        Task<KRAInitiationDetail> GetInitiationStatus(int personId);
        Task<IEnumerable<GetKRAAllocatedEmployeesByReviewerId_Result>> GetMyAllocatedEmployees(int reviewerId,int yearId);
        Task<bool> MarkAsDone(KRAMarkDone kraMarkDone);
        Task<bool> MarkAsInvalid(InvalidKRADetails viewModel);
        Task<bool> ChangeReviewerById(int kraInitiationId, int newReviewerId);
        Task<IEnumerable<GetKRAAllocatedEmployeesByReviewerId_Result>> GetMyAllocatedEmployeesForReports(int reviewerId, int yearId);
        Task<IEnumerable<KRAReportViewModel>> SearchAllKRADetail(string personId, string kraCategoryId, string yearId, string weightageId, string quarters, string isValid, string isInValid, string isKRADone);
        Task<bool> DownloadReport(string personId, string kraCategoryId, string yearId, string weightageId, string quarters, string isValid, string isKRADone);
        Task<IEnumerable<object>> SearchAllKRAHistoryDetails(string personId, string yearId, string isKRADone);
        Task<IEnumerable<GetKRAHistoryDetails_Result>> GetHistoryDetailsForClone(int id, int personId);
        Task<UpdateKRAHistory_Result> UpdateKRAHistoryDetails(int userId, PersonKRAUpdateHistoryViewModel model);
        Task<bool> SaveKRAAttachment(KRAFileAttachment fileAttachment, int loggedInUserId);
        Task<bool> DeleteKRAAttachment(int Id, int KRAId, int UserId, int KRAGoalId);
        Task<IEnumerable<GetKRAFeedbackForm_Result>> GetReviewerFeedbackDetails(int kraGoalId, int personId);
        Task<bool> AddUpdateReviewerFeedbackDetails(KRAFeedbackViewModel viewModel);
        Task<IEnumerable<KRALogViewModel>> GetLogs(int personId, string yearList);
        Task<bool> DeleteFeedback(KRAFeedbackDeleteViewModel model);
    }
}