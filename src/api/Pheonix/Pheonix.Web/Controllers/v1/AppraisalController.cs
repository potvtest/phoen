using log4net;
using Pheonix.Core.v1.Services.Business;
using Pheonix.Models.ViewModels;
using Pheonix.Models.VM;
using Pheonix.Models.VM.Classes.Appraisal;
using Pheonix.Models.VM.Classes.HelpDesk;
using Pheonix.Web.Authorization;
using Pheonix.Web.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;

namespace Pheonix.Web.Controllers.v1
{
    [RoutePrefix("v1/appraisal")]
    [Authorize]
    public class AppraisalController : ApiController
    {
        private IAppraisalService _appraisalService;
        private static readonly ILog Log4Net = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public AppraisalController(IAppraisalService appraisalService)
        {
            this._appraisalService = appraisalService;
        }

        [HttpGet, Route("get-questions")]
        public async Task<AppraiseQuestionsViewModel> GetAppraiseQuestionsBasedOnGrade()
        {
            return await _appraisalService.GetAppraiseQuestionsBasedOnGrade(Convert.ToInt32(RequestContext.GetClaim(ClaimTypes.PrimarySid)));
        }

        [HttpPost, Route("save-appraisee-form")]
        public async Task<int> SaveAppriseeForm(List<AppraiseeAnswerModel> appraiseFormModel)
        {
            return await _appraisalService.SaveAppraiseeForm(appraiseFormModel, Convert.ToInt32(RequestContext.GetClaim(ClaimTypes.PrimarySid)));
        }

        [HttpPost, Route("draft-appraisee-form")]
        public async Task<int> DraftAppriseeForm(List<AppraiseeAnswerModel> appraiseFormModel)
        {
            return await _appraisalService.DraftAppraiseeForm(appraiseFormModel, Convert.ToInt32(RequestContext.GetClaim(ClaimTypes.PrimarySid)));
        }

        [HttpPost, Route("save-appraiser-form/{averageRating}/{finalReviewerRating}/{systemRaiting}")]
        public async Task<int> SaveAppriseeForm(AppraiserFormModel appraiserFormModel, int averageRating, int finalReviewerRating, decimal systemRaiting)
        {
            return await _appraisalService.SaveAppraiserForm(appraiserFormModel, Convert.ToInt32(RequestContext.GetClaim(ClaimTypes.PrimarySid)), averageRating, finalReviewerRating, systemRaiting);
        }

        [HttpGet, Route("appraisal-assigned-to")]
        public async Task<AppraisalEmployeeViewModel> GetAppraisalAssignedTo()
        {
            return await _appraisalService.GetAppraisalAssignedTo(Convert.ToInt32(RequestContext.GetClaim(ClaimTypes.PrimarySid)));
        }

        [HttpGet, Route("get-appraiser-parameters/{appraiseeId}")]
        public async Task<AppraisalFormViewModel> GetAppraiserForm(int appraiseeId)
        {
            AppraisalFormViewModel objAppraisalFormViewModel = new AppraisalFormViewModel();
            var userID = Convert.ToInt32(RequestContext.GetClaim(ClaimTypes.PrimarySid));
            bool AllowAccess = _appraisalService.AllowUserToFeatchAppraisalData(userID, appraiseeId, DateTime.Now.Year);
            if (AllowAccess)
            {
                objAppraisalFormViewModel = await _appraisalService.GetAppraiserForm(appraiseeId);
            }
            else
            {
                objAppraisalFormViewModel = new AppraisalFormViewModel();
            }
            return objAppraisalFormViewModel;
        }

        [HttpGet, Route("get-reviewer-parameters/{appraiseeId}")]
        public async Task<AppraisalFormViewModel> GetReviewerForm(int appraiseeId)
        {
            AppraisalFormViewModel objAppraisalFormViewModel = new AppraisalFormViewModel();
            var userID = Convert.ToInt32(RequestContext.GetClaim(ClaimTypes.PrimarySid));
            bool AllowAccess = _appraisalService.AllowUserToFeatchAppraisalData(userID, appraiseeId, DateTime.Now.Year);
            if (AllowAccess)
            {
                objAppraisalFormViewModel = await _appraisalService.GetReviewerForm(appraiseeId);
            }
            else
            {
                objAppraisalFormViewModel = new AppraisalFormViewModel();
            }
            return objAppraisalFormViewModel;
            //return await _appraisalService.GetReviewerForm(appraiseeId);
        }

        [HttpGet, Route("get-onetoone-parameters/{appraiseeId}")]
        public async Task<AppraisalFormViewModel> GetOneToOneForm(int appraiseeId)
        {
            AppraisalFormViewModel objAppraisalFormViewModel = new AppraisalFormViewModel();
            var userID = Convert.ToInt32(RequestContext.GetClaim(ClaimTypes.PrimarySid));
            bool AllowAccess = _appraisalService.AllowUserToFeatchAppraisalData(userID, appraiseeId, DateTime.Now.Year);
            if (AllowAccess)
            {
                objAppraisalFormViewModel = await _appraisalService.GetOneToOneForm(appraiseeId);
            }
            else
            {
                objAppraisalFormViewModel = new AppraisalFormViewModel();
            }
            return objAppraisalFormViewModel;
            //return await _appraisalService.GetOneToOneForm(appraiseeId);
        }

        [HttpGet, Route("get-all-employess/{EmpListFor}"), Access(Api = "get-all-employess")]
        public async Task<IEnumerable<AppraisalEmployeeViewModel>> GetAllEmployess(string EmpListFor)
        {
            return await _appraisalService.GetAllEmployess(EmpListFor);
        }

        [HttpPost, Route("initiat-appraisal")]
        public async Task<bool> InitiatEmployesAppraisal(List<AppraisalEmployeeViewModel> EmpList)
        {
            return await _appraisalService.InitiatEmployesAppraisal(Convert.ToInt32(RequestContext.GetClaim(ClaimTypes.PrimarySid)), EmpList);
        }

        [HttpPost, Route("freezed-appraisal/{isFreezed}")]
        public async Task<bool> FreezedEmployesAppraisal(int isFreezed, List<AppraisalEmployeeViewModel> EmpList)
        {
            return await _appraisalService.FreezedEmployesAppraisal(Convert.ToInt32(RequestContext.GetClaim(ClaimTypes.PrimarySid)), EmpList, isFreezed);
        }

        [HttpGet, Route("get-all-managers")]
        public async Task<List<DropdownItems>> GetManagerDropdowns()
        {
            return await _appraisalService.GetManagerDropdowns();
        }

        [HttpGet, Route("appraisee-details/{userId}")]
        public async Task<AppraisalEmployeeViewModel> GetAppraisalAssignedTo(int userId)
        {
            return await _appraisalService.GetAppraisalAssignedTo(userId);
        }

        [HttpGet, Route("get-form-detail")]
        public async Task<AppraisalFormViewModel> GetAppraiseFormDetail()
        {
            return await _appraisalService.GetAppraiseFormDetail(Convert.ToInt32(RequestContext.GetClaim(ClaimTypes.PrimarySid)));
        }

        [HttpGet, Route("get-approval-history/{historyof}")]
        public async Task<IEnumerable<AppraisalListModel>> GetTicketsHistoryOfApproval(int historyof)
        {
            return await _appraisalService.GetTicketsHistoryOfApproval(Convert.ToInt32(RequestContext.GetClaim(ClaimTypes.PrimarySid)), historyof);
        }

        [HttpGet, Route("appraisal-report-details"), Access(Api = "appraisal-report-details")]
        public async Task<IEnumerable<AppraisalReportViewModel>> GetAppraisalReport(int? year = null)
        {
            return await _appraisalService.GetAppraisalReport(year);
        }
        [Route("get-appraisal-summary"), HttpGet, Access(Api = "get-appraisal-summary")]
        public async Task<AppraisalSummaryModel> GetAppraisalSummary(int? rating = null, int? deliveryUnit = null, int? deliveryTeam = null, int? year = null)
        {
            return await _appraisalService.GetAppraisalSummary(rating, deliveryUnit, deliveryTeam, year);
        }

        [Route("appraisal-negotiation-details"), HttpGet, Access(Api = "appraisal-negotiation-details")]
        public async Task<IEnumerable<AppraisalReportViewModel>> GetNegotiationDetails()
        {
            return await _appraisalService.GetNegotiationDetails();
        }

        [HttpPost, Route("normalized-employes-appraisal")]
        public async Task<bool> NormalizedEmployesAppraisal(List<AppraisalReportViewModel> EmpList)
        {
            return await _appraisalService.NormalizedEmployesAppraisal(Convert.ToInt32(RequestContext.GetClaim(ClaimTypes.PrimarySid)), EmpList);
        }

        [Route("get-appraisal-questions"), HttpGet, Access(Api = "get-appraisal-questions")]
        public async Task<IEnumerable<AppraiseeQuestion>> GetAllQuestions()
        {
            return await _appraisalService.GetAllQuestions();
        }

        [Route("add-appraisal-questions"), HttpPost]
        public async Task<bool> AddAllQuestions(List<AppraiseeQuestion> questions)
        {
            return await _appraisalService.AddAllQuestions(questions);
        }

        [Route("update-appraisal-questions/{isDelete}"), HttpPost]
        public async Task<bool> UpdateAllQuestions(int isDelete, AppraiseeQuestion questions)
        {
            return await _appraisalService.UpdateAllQuestions(isDelete, questions);
        }

        [Route("get-appraisal-parameters"), HttpGet, Access(Api = "get-appraisal-parameters")]
        public async Task<IEnumerable<AppraiseeParametersViewModel>> GetAllParameters()
        {
            return await _appraisalService.GetAllParameters();
        }

        [Route("add-appraisal-parameters"), HttpPost]
        public async Task<bool> AddAllParameters(AppraiseeParametersViewModel parameters)
        {
            return await _appraisalService.AddAllParameters(parameters);
        }

        [Route("update-appraisal-parameters/{isDelete}"), HttpPost]
        public async Task<bool> UpdateAllParameters(int isDelete, AppraiseeParametersViewModel parameters)
        {
            return await _appraisalService.UpdateAllParameters(isDelete, parameters);
        }

        [Route("get-appraisal-preview/{level}"), HttpGet, Access(Api = "get-appraisal-preview")]
        public async Task<AppraisalFormViewModel> GetQuesitionsParameters(int level)
        {
            return await _appraisalService.GetQuesitionsParameters(level);
        }

        [Route("get-appraisal-status-report/{year}/{location}"), HttpGet, Access(Api = "get-appraisal-status-report")]
        public async Task<IHttpActionResult> GetAppraisalCurrentStatus(int year, int location)
        {
            return Ok(await _appraisalService.GetAppraisalCurrentStatus(year, location));
        }

        //[HttpGet, Route("get-appraisee-final-report")]
        //public async Task<AppraisalFormViewModel> GetAppraiseeFinalReport()
        //{
        //    return await _appraisalService.GetAppraiseeFinalReport(Convert.ToInt32(RequestContext.GetClaim(ClaimTypes.PrimarySid)));
        //}

        /// <summary>
        /// To fetch year wise appraisal data for the respective user: Rahul R
        /// </summary>
        /// <param name="year"></param>
        /// <returns></returns>
        [HttpGet, Route("get-appraisee-final-report-yearwise/{year}/{userid}")]
        public async Task<AppraisalFormViewModel> GetAppraiseeFinalReport(int year, int userid)
        {
            AppraisalFormViewModel objAppraisalFormViewModel = new AppraisalFormViewModel();
            var userID = Convert.ToInt32(RequestContext.GetClaim(ClaimTypes.PrimarySid));
            bool AllowAccess = _appraisalService.AllowUserToFeatchAppraisalData(userID, userid, year);
            if (AllowAccess)
            {
                objAppraisalFormViewModel = await _appraisalService.GetAppraiseeFinalReport(userid, year);
            }
            else
            {
                objAppraisalFormViewModel = new AppraisalFormViewModel();
            }
            return objAppraisalFormViewModel;
            //return await _appraisalService.GetAppraiseeFinalReport(userid, year);
        }

        /// <summary>
        /// To fetch the yeas of appraisal faced by the respective user: Rahul R
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("get-appraisee-years/{userid}")]
        public async Task<IHttpActionResult> GetAppraisalYears(int userid)
        {
            return Ok(await _appraisalService.GetAppraisalYears(userid));
        }

        /// <summary>
        /// To fetch the yeas of appraisal carried out in the system: Rahul R
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("get-appraisal-years")]
        public async Task<IHttpActionResult> AppraisalYears()
        {
            return Ok(await _appraisalService.GetAppraisalYears());
        }

        [HttpPost, Route("update-appraisal")]
        public async Task<bool> UpdateEmployesAppraisal(List<AppraisalEmployeeViewModel> EmpList)
        {
            return await _appraisalService.UpdateEmployesAppraisal(Convert.ToInt32(RequestContext.GetClaim(ClaimTypes.PrimarySid)), EmpList);
        }

        [Route("appraisal-negotiation-history"), HttpGet, Access(Api = "appraisal-negotiation-details")]
        public async Task<IEnumerable<AppraisalReportViewModel>> GetNegotiationHistoryDetails()
        {
            return await _appraisalService.GetNegotiationHistoryDetails();
        }

        [HttpPost, Route("update-normalized-employee/{personID}/{rating}/{isPromotionNorm}/{promotionforByNorm}")]
        public async Task<bool> UpdateNormalizedEmployesAppraisal(List<AppraisalReportViewModel> EmpList, int personID, int rating, bool? isPromotionNorm, string promotionforByNorm = null)
        {
            return await _appraisalService.UpdateNormalizedEmployesAppraisal(Convert.ToInt32(RequestContext.GetClaim(ClaimTypes.PrimarySid)), EmpList, personID, rating, isPromotionNorm ?? false, promotionforByNorm);
        }

        /// <summary>
        /// fetch the details yearwise as the year changes.: Rahul R
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="year"></param>
        /// <returns></returns>
        [HttpGet, Route("appraisee-details-yearwise/{userId}/{year}")]
        public async Task<AppraisalEmployeeViewModel> GetAppraisalAssignedTo(int userId, int year)
        {
            return await _appraisalService.GetAppraisalAssignedTo(userId, year);
        }

        /// <summary>
        ///For Downloading appraisal summary report. : Rahul R
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("download-appraisal-report"), Access(Api = "appraisal-negotiation-details")]
        public HttpResponseMessage DownloadAppraisalReport(string location, int? status = null, int? grade = null, int? empID = 0, int? year=0)
        {
            return _appraisalService.AppraisalReport(location, status, grade, empID, year);
        }


        /// <summary>
        /// For Downloading Pending Appraisal Status Summary. : Rahul R
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("pending-appraisal-status"), Access(Api = "appraisal-negotiation-details")]
        public HttpResponseMessage DownloadPendingAppraisalSummary()
        {
            return _appraisalService.PendingAppraisalStatus();
        }

        [HttpGet, Route("get-organization-category")]
        public async Task<List<DropdownItems>> GetOrganizationCategory()
        {
            return await _appraisalService.GetOrganizationCategory();
        }
        
       
        /// To get Rating and Promo for last 5 years
        
        [HttpGet, Route("get-rating-5years/{UserId}")]
        public async Task<IHttpActionResult> AppraisalRatingLast5Years(int userId)
        {
            try
            {
                var result = await _appraisalService.AppraisalRatingLast5Years(userId);
                if (result != null)
                    return Content(HttpStatusCode.OK, result);

                return Content(HttpStatusCode.NotFound, "No record found!");
            }
            catch (Exception ex)
            {
                Log4Net.Error("Error while fetching Appraisal Rating for last 5 years: Inner Exception: " + ex.InnerException + "StackTrace :" + ex.StackTrace);
                return Content(HttpStatusCode.InternalServerError, "Error while fetching Appraisal Rating for last 5 years");
            }
        }
    }
}