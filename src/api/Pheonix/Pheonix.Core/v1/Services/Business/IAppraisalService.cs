using Pheonix.DBContext;
using Pheonix.Models.ViewModels;
using Pheonix.Models.VM;
using Pheonix.Models.VM.Classes.Appraisal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Core.v1.Services.Business
{
    public interface IAppraisalService
    {
        Task<AppraiseQuestionsViewModel> GetAppraiseQuestionsBasedOnGrade(int userId);
        Task<List<AppraiseeParametersViewModel>> GetAppraiseeParametersBasedOnGrade(int userId);
        Task<AppraisalFormViewModel> GetAppraiseeForm(int userId);
        Task<AppraisalFormViewModel> GetAppraiserForm(int userId);
        Task<AppraisalFormViewModel> GetReviewerForm(int userId);
        Task<AppraisalFormViewModel> GetOneToOneForm(int userId);
        Task<int> SaveAppraiseeForm(List<AppraiseeAnswerModel> appraiseForm, int userId);
        Task<int> DraftAppraiseeForm(List<AppraiseeAnswerModel> appraiseForm, int userId);

        Task<int> SaveAppraiserForm(AppraiserFormModel appraiserForm, int userId, int averageRating, int finalReviewerRating, decimal systemRaiting);
        Task<IEnumerable<AppraisalListModel>> GetTicketsForApproval(int userID, int approvalFor);
        Task<AppraisalEmployeeViewModel> GetAppraisalAssignedTo(int userId);
        Task<IEnumerable<AppraisalEmployeeViewModel>> GetAllEmployess(string EmpListFor);
        Task<bool> InitiatEmployesAppraisal(int userId, List<AppraisalEmployeeViewModel> EmpList);
        Task<bool> FreezedEmployesAppraisal(int userId, List<AppraisalEmployeeViewModel> EmpList, int isFreezed);
        Task<List<DropdownItems>> GetManagerDropdowns();
        Task<AppraisalFormViewModel> GetAppraiseFormDetail(int userId);
        Task<IEnumerable<AppraisalListModel>> GetTicketsHistoryOfApproval(int userId, int historyof);
        Task<IEnumerable<AppraisalReportViewModel>> GetAppraisalReport(int? year);
        Task<AppraisalSummaryModel> GetAppraisalSummary(int? rating, int? delivertUnit, int? deliveryTeam,int? year);
        Task<IEnumerable<AppraisalReportViewModel>> GetNegotiationDetails();
        Task<bool> NormalizedEmployesAppraisal(int userId, List<AppraisalReportViewModel> EmpList);
        Task<IEnumerable<AppraiseeQuestion>> GetAllQuestions();
        Task<bool> AddAllQuestions(List<AppraiseeQuestion> questions);
        Task<bool> UpdateAllQuestions(int isDelete, AppraiseeQuestion questions);
        Task<IEnumerable<AppraiseeParametersViewModel>> GetAllParameters();
        Task<bool> AddAllParameters(AppraiseeParametersViewModel parameters);
        Task<bool> UpdateAllParameters(int isDelete, AppraiseeParametersViewModel parameters);
        Task<AppraisalFormViewModel> GetQuesitionsParameters(int level);
        Task<List<rpt_AppraisalCurrentStatus_Result>> GetAppraisalCurrentStatus(int year, int location);
        Task<AppraisalFormViewModel> GetAppraiseeFinalReport(int userId);
        Task<AppraisalFormViewModel> GetAppraiseeFinalReport(int userId, int year);
        Task<List<int>> GetAppraisalYears(int userId);
        Task<List<int>> GetAppraisalYears();
        Task<bool> UpdateEmployesAppraisal(int userId, List<AppraisalEmployeeViewModel> EmpList);
        HttpResponseMessage AppraisalReport(string location, int? status, int? grade, int? empID, int? year);
        Task<IEnumerable<AppraisalReportViewModel>> GetNegotiationHistoryDetails();
        Task<bool> UpdateNormalizedEmployesAppraisal(int userId, List<AppraisalReportViewModel> EmpList, int personID, int rating, bool isPromotionNorm, string promotionforByNorm);
        Task<AppraisalEmployeeViewModel> GetAppraisalAssignedTo(int userId, int year);
        Task<List<DropdownItems>> GetOrganizationCategory();
        HttpResponseMessage PendingAppraisalStatus();

        bool AllowUserToFeatchAppraisalData(int LogedInUserId, int AppraiseeId, int? year);
        Task<IEnumerable<GetAppraisalRatingByYears_Result>> AppraisalRatingLast5Years(int userId);
    }
}
