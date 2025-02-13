using Pheonix.Core.Services;
using Pheonix.Core.v1.Services;
using Pheonix.Core.v1.Services.Seperation;
using Pheonix.Models;
using Pheonix.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;
using Pheonix.Web.Extensions;
using System.Threading.Tasks;
using System.Reflection;
using System.Net.Http;
using Pheonix.Models.VM;
using Pheonix.Models.Models;

namespace Pheonix.Web.Controllers.v1
{
    [RoutePrefix("v1/seperation")]
    //#if !DEBUG
    //    [Authorize]
    //#endif
    public class SeperationConfigController : ApiController, ISeperationConfigCRUDService<SeperationConfigViewModel>
    {
        private ISeperationConfigService _seperationConfigService;
        private ISeparationCardService _separationCardService;

        public SeperationConfigController(ISeperationConfigService seperationConfigService, ISeparationCardService separationCardService)//,
        {
            _seperationConfigService = seperationConfigService;
            _separationCardService = separationCardService;
        }

        /// <summary>
        /// provides current user id.
        /// </summary>
        /// <returns></returns>
        int GetUserId()
        {
            return RequestContext.GetClaimInt(ClaimTypes.PrimarySid);
        }

        #region  Seperation Config
        [Route("list"), HttpGet]
        public IEnumerable<SeperationConfigViewModel> GetList(string filters = null)
        {
            var employeeList = _seperationConfigService.GetList(filters);

            return employeeList;
        }

        [Route("add"), HttpPost]
        public ActionResult Add(SeperationConfigViewModel model)
        {
            if (model == null)
                throw new HttpException(404, "NotFound");

            _seperationConfigService.UserId = GetUserId();
            return _seperationConfigService.Add(model);
        }

        [Route("update"), HttpPost]
        public ActionResult Update(SeperationConfigViewModel model)
        {
            if (model == null)
                throw new HttpException(404, "NotFound");

            _seperationConfigService.UserId = GetUserId();
            return _seperationConfigService.Update(model);
        }

        [Route("delete/{id:int}"), HttpPost]
        public ActionResult Delete(int id)
        {
            if (id == 0)
                throw new HttpException(404, "NotFound");

            _seperationConfigService.UserId = GetUserId();
            return _seperationConfigService.Delete(id);
        }

        [Route("getRoles"), HttpGet]
        public Task<List<DropdownItems>> GetRoles()
        {
            var roleList = _seperationConfigService.GetRoleList();
            return roleList;
        }

        #endregion

        #region Sepration Apply
        [Route("add-separation/{id:int}"), HttpPost]
        public async Task<ActionResult> Add(SeperationViewModel model, int id = 0)
        {
            if (model == null)
                throw new HttpException(404, "NotFound");

            if (model.PersonID == 0)
                model.PersonID = id;
            _separationCardService.UserId = GetUserId();
            return await _separationCardService.Add(model);
        }

        [Route("update-separation"), HttpPost]
        public ActionResult Update(SeperationViewModel model)
        {
            if (model == null)
                throw new HttpException(404, "NotFound");

            _separationCardService.UserId = GetUserId();
            return _separationCardService.Update(model);
        }

        [Route("emp-noticeperiod/{id:int}"), HttpGet]
        public int GetNoticePeriod(int id)
        {
            return _separationCardService.GetNoticePeriod(id);
        }

        [Route("getSeparationDetail/{id:int}"), HttpGet]
        public IEnumerable<SeperationViewModel> GetCardDetail(int? id)
        {
            _separationCardService.UserId = id ?? GetUserId();
            return _separationCardService.GetEmpSeperationDetl(_separationCardService.UserId);
        }

        #endregion

        #region Manager Approval
        [Route("seperation-approve/{IsHR:bool}"), HttpPost]
        public ActionResult Approve(SeperationViewModel model, Boolean IsHR = false)
        {
            if (model == null)
            {
                throw new HttpException(404, "NotFound");
            }
            _separationCardService.UserId = GetUserId();
            return _separationCardService.Approve(model, IsHR);
        }

        //Resignation rejected by Reporting Manager
        [Route("separation-reject"), HttpPost]
        public ActionResult Reject(SeperationViewModel model)
        {
            if (model == null)
            {
                throw new HttpException(404, "NotFound");
            }
            _separationCardService.UserId = GetUserId();
            return _separationCardService.Reject(model);
        }

        [Route("get-seperation-list/{isHR:bool}"), HttpGet]
        public IEnumerable<SeperationViewModel> GetSeperationList(string filters = null, Boolean isHR = false)
        {

            _separationCardService.UserId = GetUserId();
            return _separationCardService.GetSeperationList(isHR);
        }
        #endregion

        #region Seperation Job 8 Days before Seperation date

        /// <summary>
        /// Process Getting executed on this api call.
        /// Check upcoming Employees whose Seperation Date falls in next 8 Days.
        /// Get All Checklist from seperation config.
        /// Create Checklist records for employee to complete seperation process departmentwise.
        /// Add Records in Approval Grid of Dashboard for Department to complete checklist. 
        /// All Employee of department should see the checklist.
        /// Send Email notification to all department Employees.
        /// Once All checklist completed from each deparment, Email will be raised.
        /// </summary>
        /// <returns></returns>
        [Route("initiate-separation-process"), HttpGet]
        public async Task<object> InitiateSeparationProcess(int SeparationID = 0, string discussionDt = "")
        {
            return await Task.Run(() =>
                _separationCardService.InitiateSeparationProcess(SeparationID, discussionDt));
        }
        #endregion

        #region Seperation After job execution

        //[Route("get-SeparationDetailsForEmployee"), HttpGet]
        //public SeperationProcessDetailsViewModel GetSeparationDetailsForEmployee()
        //{
        //    //HR can see all Checklist Data. But can edit only HR deparment data.
        //    //Resigned Employee can see all the records, but cannot modify any of it.
        //    //Respective department person can see only his deparment specific data.

        //    var roles = RequestContext.GetClaim(ClaimTypes.Role);
        //    var personID = Convert.ToInt32(RequestContext.GetClaim(ClaimTypes.PrimarySid));
        //    return _seperationConfigService.GetSeperationProcessDetails(roles, personID, true);
        //}

        [Route("getSeperationProcessDetails/{isHistory:int}/{year:int?}/{separationMode:int?}"), HttpGet]
        public async Task<SeperationProcessDetailsViewModel> GetSeperationProcessDetails(int isHistory, int? year = null,int? separationMode = null)
        {
            //HR can see all Checklist Data in editable mode. 
            //Resigned Employee can see all the records, but cannot modify any of it.
            //Respective department person can see only his deparment specific data.

            var roles = RequestContext.GetClaim(ClaimTypes.Role);
            var personID = Convert.ToInt32(RequestContext.GetClaim(ClaimTypes.PrimarySid));
            return await _separationCardService.GetSeperationProcessDetails(roles, personID, isHistory, year, separationMode);
        }

        [Route("completeSeperationProcess"), HttpPost]
        public ActionResult CompleteSeperationProcess(SeperationConfigProcessViewModel model)
        {
            //_seperationConfigService.UserId = GetUserId();
            var userID = Convert.ToInt32(RequestContext.GetClaim(ClaimTypes.PrimarySid));
            return _separationCardService.CompleteSeperationProcess(model, userID);
        }

        [Route("PrintDoc/{separationId}/{letterType}/{userID}/{fileType}"), HttpGet]
        public async Task<HttpResponseMessage> PrintDoc(int separationId, int letterType, int userID, string fileType = "")
        {
            //var userID = Convert.ToInt32(RequestContext.GetClaim(ClaimTypes.PrimarySid));
            //fileType = "WORD";
            var result = await _separationCardService.GenerateDocument(letterType, separationId, userID, fileType);
            return result;
        }

        //[HttpGet, Route("getRoles")]
        //public async Task<IEnumerable<int>> GetConfigRoles()
        //{
        //    return await _separationCardService.GetConfigRoles();
        //}
        #endregion

        #region Employee Termination
        //[Route("EmployeeTermination"), HttpPost]
        //public ActionResult TerminateEmployee(SeperationViewModel model)
        //{
        //    var roles = RequestContext.GetClaim(ClaimTypes.Role);
        //    var personID = Convert.ToInt32(RequestContext.GetClaim(ClaimTypes.PrimarySid));
        //    return _separationCardService.TerminateEmployee(model, personID);
        //}

        [Route("GetContractConversionData/{personId}"), HttpGet]
        public ContractConversionVM GetContractConversionData(int personId)
        {
            return _separationCardService.GetContractConversionData(personId);
        }

        [Route("EmployeeTermination"), HttpPost]
        public List<SeperationTerminationViewModel> TerminateEmployee(SeperationViewModel model)
        {
            //var roles = RequestContext.GetClaim(ClaimTypes.Role);
            var personID = Convert.ToInt32(RequestContext.GetClaim(ClaimTypes.PrimarySid));
            return _separationCardService.TerminateEmployee(model, personID);
        }

        [Route("SendSCNotice"), HttpPost]
        public bool SendSCNotice(List<SeperationTerminationViewModel> model)
        {
            //var roles = RequestContext.GetClaim(ClaimTypes.Role);
            //var personID = Convert.ToInt32(RequestContext.GetClaim(ClaimTypes.PrimarySid));
            return _separationCardService.SendSCNotice(model);
        }
        //SA: TODO: Need to remove this method
        //[Route("EmployeeInactive"), HttpPost]
        //public ActionResult EmployeeInactive(EmployeeTerminationViewModel model)
        //{
        //    var roles = RequestContext.GetClaim(ClaimTypes.Role);
        //    //var personID = Convert.ToInt32(RequestContext.GetClaim(ClaimTypes.PrimarySid));
        //    //_seperationConfigService.UserId = GetUserId();
        //    _separationCardService.UserId = GetUserId();
        //    return _separationCardService.EmployeeInactive(model);
        //}
        #endregion

        #region SeparationReasonMaster
        [Route("addReason"), HttpPost]
        public ActionResult AddReason(SeparationReasonViewModel model)
        {
            if (model == null)
                throw new HttpException(404, "NotFound");

            _seperationConfigService.UserId = GetUserId();
            return _seperationConfigService.AddReason(model);
        }

        [Route("updateReason"), HttpPost]
        public ActionResult UpdateReason(SeparationReasonViewModel model)
        {
            if (model == null)
                throw new HttpException(404, "NotFound");

            _seperationConfigService.UserId = GetUserId();
            return _seperationConfigService.UpdateReason(model);
        }

        [Route("deleteReson/{id:int}"), HttpPost]
        public ActionResult DeleteReson(int id)
        {
            if (id == 0)
                throw new HttpException(404, "NotFound");

            _seperationConfigService.UserId = GetUserId();
            return _seperationConfigService.DeleteReson(id);
        }

        [Route("reasonList"), HttpGet]
        public IEnumerable<SeparationReasonViewModel> GetReasonList(string filters = null)
        {
            var employeeList = _seperationConfigService.GetReasonList(filters);

            return employeeList;
        }
        #endregion

        #region Withdrawal Process
        [Route("withdraw-approve"), HttpPost]
        public ActionResult WithdrawApproval(SeperationViewModel model)
        {
            if (model == null)
            {
                throw new HttpException(404, "NotFound");
            }
            _separationCardService.UserId = GetUserId();
            return _separationCardService.WithdrawalApprove(model);
        }

        //Resignation rejected by Reporting Manager
        [Route("withdraw-reject"), HttpPost]
        public ActionResult WithdrawRejection(SeperationViewModel model)
        {
            if (model == null)
            {
                throw new HttpException(404, "NotFound");
            }
            _separationCardService.UserId = GetUserId();
            return _separationCardService.WithdrawalReject(model);
        }
        #endregion        

        //[Route("sendremindermail"), HttpPost]
        //public async Task<object> SendProcessReminderMail()
        //{
        //    var result = await _separationCardService.SendProcessReminderMail();
        //    return new EnvelopeModel<object>(result, false, "");
        //}

        [Route("SeparationPrintDoc/{personId}/{letterType}/{userID}"), HttpGet]
        public async Task<HttpResponseMessage> PrintSeparationDoc(int personId, int letterType, int userID)
        {
            //var userID = Convert.ToInt32(RequestContext.GetClaim(ClaimTypes.PrimarySid));
            var result = await _separationCardService.GenerateTerminationDocument(letterType, personId, userID);
            return result;
        }


        [Route("DownloadZip/{personId}"), HttpGet]
        public async Task<HttpResponseMessage> DownloadZip(int personId)
        {
            //var userID = Convert.ToInt32(RequestContext.GetClaim(ClaimTypes.PrimarySid));
            var result = await _separationCardService.DownloadZip(personId);
            return result;
        }

        //[Route("ExitDateUpdate/{isDateChange}"), HttpPost]
        //public ActionResult ExitDateUpdate(ChangeReleaseDateViewModel model, int isDateChange)
        //{
        //    var userID = Convert.ToInt32(RequestContext.GetClaim(ClaimTypes.PrimarySid));
        //    var result = _separationCardService.ExitDateUpdate(model, userID, isDateChange);
        //    return result;
        //}
        [Route("ExitDateUpdate/{isDateChange}"), HttpPost]
        public SeperationTerminationViewModel ExitDateUpdate(ChangeReleaseDateViewModel model, int isDateChange)
        {
            var userID = Convert.ToInt32(RequestContext.GetClaim(ClaimTypes.PrimarySid));
            var result = _separationCardService.ExitDateUpdate(model, userID, isDateChange);
            return result;
        }

        [HttpGet, Route("employmentstatus/{personID}")]
        public async Task<int> GetEmploymentStatus(int personID)
        {
            return await _separationCardService.GetEmploymentStatus(personID);
        }

        [Route("approvalhistory/{isHR:bool}"), HttpGet]
        public async Task<IEnumerable<SeperationViewModel>> GetSeperationApprovalList(Boolean isHR = false)
        {
            var userID = Convert.ToInt32(RequestContext.GetClaim(ClaimTypes.PrimarySid));
            return await _separationCardService.GetSeperationListHistory(userID, isHR);
        }

        [Route("ShowCauseNotice2/{type}/{separationprocessid}"), HttpPost]
        public SeperationTerminationViewModel ShowCauseNotice2(EmailTemplateViewModel model, int type, int separationprocessid)
        {
            //var userID = Convert.ToInt32(RequestContext.GetClaim(ClaimTypes.PrimarySid));
            var userID = Convert.ToInt32(RequestContext.GetClaim(ClaimTypes.PrimarySid));
            return _separationCardService.ShowCauseNotice2(model, type, separationprocessid, userID);
        }

        //[Route("ShowCauseNotice2/{type}/{separationprocessid}"), HttpPost]
        //public ActionResult ShowCauseNotice2(EmailTemplateViewModel model, int type, int separationprocessid)
        //{
        //    //var userID = Convert.ToInt32(RequestContext.GetClaim(ClaimTypes.PrimarySid));
        //    var userID = Convert.ToInt32(RequestContext.GetClaim(ClaimTypes.PrimarySid));
        //    var result = _separationCardService.ShowCauseNotice2(model, type, separationprocessid, userID);
        //    return result;
        //}

        #region Exit Process Form
        [Route("addexitform/{separationID:int}"), HttpPost]
        public ActionResult AddExitForm(ExitProcessFormDetailViewModel model, int separationID)
        {
            if (model == null)
                throw new HttpException(404, "NotFound");

            _separationCardService.UserId = GetUserId();
            return _separationCardService.AddExitForm(model, separationID);
        }

        [Route("getExitFormData/{separationID:int}"), HttpGet]
        public async Task<ExitProcessFormDetailViewModel> GetExitFormData(int separationID)
        {
            _separationCardService.UserId = GetUserId();
            return await _separationCardService.GetExitFormData(separationID);
        }
        #endregion

        [HttpGet, Route("GetPersonalEmailID/{personID}")]
        public async Task<string> GetPersonalEmailID(int personID)
        {
            return await _separationCardService.GetPersonalEmailID(personID);
        }


        [Route("getSeperationListForRMG"), HttpGet]
        public async Task<IEnumerable<SeperationViewModel>> GetSeperationListForRMG()
        {
            //HR can see all Checklist Data in editable mode. 
            //Resigned Employee can see all the records, but cannot modify any of it.
            //Respective department person can see only his deparment specific data.

            var roles = RequestContext.GetClaim(ClaimTypes.Role);
            var personID = Convert.ToInt32(RequestContext.GetClaim(ClaimTypes.PrimarySid));
            return await _separationCardService.GetSeperationListForRMG(roles, personID);
        }

    }
}