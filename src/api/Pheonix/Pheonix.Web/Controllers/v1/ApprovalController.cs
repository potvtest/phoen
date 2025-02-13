using Pheonix.Core.v1.Services.Business;
using Pheonix.Models.ViewModels;
using Pheonix.Models.VM;
using Pheonix.Models.VM.Classes.Appraisal;
using Pheonix.Models.VM.Classes.HelpDesk;
using Pheonix.Web.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;

namespace Pheonix.Web.Controllers.v1
{
    [RoutePrefix("v1/approval")]
    [Authorize]
    public class ApprovalController : ApiController
    {
        private IEmployeeLeaveService _leaveService;
        private IEmployeeService _employeeService;
        private IHelpDeskService _helpDeskService;
        private IAppraisalService _appraisalService;
        private Pheonix.Core.v1.Services.IApprovalService _approvalService;

        public ApprovalController(IEmployeeLeaveService service, IEmployeeService employeeService, Pheonix.Core.v1.Services.IApprovalService approvalService, IHelpDeskService helpDeskService, IAppraisalService appraisalService)
        {
            _leaveService = service;
            _employeeService = employeeService;
            _approvalService = approvalService;
            _helpDeskService = helpDeskService;
            _appraisalService = appraisalService;
        }

        [HttpGet, Route("leaves/{leaveType:int}/{month:int}/{year:int}")]
        public async Task<LeaveViewModel<ApprovalLeaveViewModel>> GetLeaveDetails(int leaveType, int month, int year)
        {
            var leaves = await _leaveService.GetLeaveDetailsForApproval<ApprovalLeaveViewModel>(RequestContext.GetClaimInt(ClaimTypes.PrimarySid));
            return leaves;
        }

        [HttpGet, Route("compoffs")]
        public async Task<LeaveViewModel<ApprovalCompOffViewModel>> GetCompOffDetails()
        {
            var leaves = await _leaveService.GetCompOffDetailsForApproval<ApprovalCompOffViewModel>(RequestContext.GetClaimInt(ClaimTypes.PrimarySid));
            return leaves;
        }

        [HttpPost, Route("leave/{applicantID:int}")]
        public async Task<EmployeeLeaveViewModel> ApproveLeave(int applicantID, EmployeeLeaveViewModel model)
        {
            int[] validApprovalCodes = new int[] { 2, 3 };
            if (validApprovalCodes.Contains(model.Status))//technically this validation should never required, but just to make sure no one tempers the request before flight
            {
                return await _leaveService.ApproveLeave(applicantID, RequestContext.GetClaimInt(ClaimTypes.PrimarySid), model);
            }
            throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.BadRequest));
        }

        [HttpPost, Route("compoff/{applicantID:int}")]
        public async Task<ApprovalCompOffViewModel> ApproveCompOff(int applicantID, ApprovalCompOffViewModel model)
        {
            int[] validApprovalCodes = new int[] { 1, 3 };
            if (validApprovalCodes.Contains(model.Status))//technically this validation should never required, but just to make sure no one tempers the request before flight
            {
                return await _leaveService.ApproveCompOff(applicantID, RequestContext.GetClaimInt(ClaimTypes.PrimarySid), model);
            }
            throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.BadRequest));
        }


        [HttpGet, Route("employee/approvals")]
        public async Task<IEnumerable<EmployeeApproval>> GetApprovalsList()
        {
            return await _employeeService.GetApprovalsList();
        }

        [HttpGet, Route("employee/approvals/request/{id:int}")]
        public async Task<EmployeeApproval> GetApprovalById(int id)
        {
            return await _employeeService.GetApprovalById(id);
        }

        [HttpGet, Route("employee/approvals/request/attachment/{approvalId:int}")]
        public async Task<IHttpActionResult> GetAttachmentRecordForProfile(int approvalId)
        {
            try
            {
                if (approvalId == 0)
                    return Content(HttpStatusCode.BadRequest, "ApprovalId should not be zero!");

                var empProfileAttachmentObject = await _employeeService.GetAttachmentRecordByApprovalId(approvalId);
                if(empProfileAttachmentObject != null)
                    return Content(HttpStatusCode.OK, empProfileAttachmentObject);
                return Content(HttpStatusCode.NotFound, "No record found!");
            }
            catch(Exception ex)
            {
                return Content(HttpStatusCode.InternalServerError, "Error while searching employee profile Attachment Details !!!");
            }
        }

        [HttpPost, Route("employee/approvals/{empID:int}/{stageID}/{moduleID:int}/{statusID:int}")]
        public async Task<EmployeeApprovalViewModel> ApproveEmployeeDetails(string stageID, int moduleID, int empID, int statusID, string comments)
        {
            return await _employeeService.ApproveEmployeeDetails(RequestContext.GetClaim(ClaimTypes.Email), stageID, moduleID, empID, statusID, comments);
        }
        [HttpPost, Route("employee/bulkapprovals/{statusID:int}")]
        public async Task<Boolean> ApproveBulkEmployeesDetails(int statusID, string comments, [FromBody]params int[] empIDs)        
            => await _employeeService.ApproveBulkEmployeeDetails(RequestContext.GetClaim(ClaimTypes.Email), statusID, comments, empIDs);   
        public void GetStartDateEndDate(int year, int month, out DateTime startDate, out DateTime endDate)
        {
            startDate = new DateTime(year, 1, 1).Date;
            endDate = new DateTime(year, 12, 31).Date;
        }

        [HttpGet, Route("by-me")]
        public List<ApprovalsViewModel> MyApprovals()
        {
            return _approvalService.ListAllApprovalsFor(RequestContext.GetClaimInt(ClaimTypes.PrimarySid));
        }

        [Route("leaves-history"), HttpGet]
        public async Task<LeaveViewModel<ApprovalLeaveViewModel>> GetleaveApprovalHistory()
        {
            var history = await _leaveService.GetleaveApprovalHistory(RequestContext.GetClaimInt(ClaimTypes.PrimarySid));
            return history;
        }

        [HttpGet, Route("help-desk")]
        public async Task<IEnumerable<HelpDeskListModel>> GetHelpDeskTicketDetails()
        {
            return await _helpDeskService.GetTicketsForApproval(Convert.ToInt32(RequestContext.GetClaim(ClaimTypes.PrimarySid)));
        }

        [HttpGet, Route("appraisal")]
        public async Task<IEnumerable<AppraisalListModel>> GetApprisalDetails()
        {
            return await _appraisalService.GetTicketsForApproval(Convert.ToInt32(RequestContext.GetClaim(ClaimTypes.PrimarySid)), 7);
        }

        [HttpGet, Route("onetoone")]
        public async Task<IEnumerable<AppraisalListModel>> GetApprisalOneToOneDetails()
        {
            return await _appraisalService.GetTicketsForApproval(Convert.ToInt32(RequestContext.GetClaim(ClaimTypes.PrimarySid)), 8);
        }
    }
}