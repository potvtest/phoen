using Pheonix.Core.v1.Services.Business;
using Pheonix.Models.VM;
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
    [RoutePrefix("v1/dashboard")]
    public class DashboardController : ApiController
    {
        private IEmployeeLeaveService _leaveService;
        private IEmployeeService _employeeService;

        public DashboardController(IEmployeeLeaveService service, IEmployeeService employeeService)
        {
            _leaveService = service;
            _employeeService = employeeService;
        }

        [HttpGet, Route("leaves/{leaveType:int}/{month:int}/{year:int}")]
        public async Task<LeaveViewModel<ApprovalLeaveViewModel>> GetLeaveDetails(int leaveType, int month, int year)
        {
            DateTime fromDate, toDate;

            GetStartDateEndDate(year, month, out fromDate, out toDate);
            var leaves = await _leaveService.GetLeaveDetails<ApprovalLeaveViewModel>(-1,
                RequestContext.GetClaimInt(ClaimTypes.Country),
                leaveType, fromDate, toDate);
            leaves.EmployeeLeaveViewModels = leaves.EmployeeLeaveViewModels.Where(t => t.Status == 1);
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

        //[HttpPost, Route("employee/approvals/{empID:int}/{stageID}/{moduleID:int}/{statusID:int}")]
        //public async Task<EmployeeApprovalViewModel> ApproveEmployeeDetails(string stageID, int moduleID, int empID, int statusID)
        //{
        //    return await _employeeService.ApproveEmployeeDetails(RequestContext.GetClaim(ClaimTypes.Email), stageID, moduleID, empID, statusID);
        //}

        public void GetStartDateEndDate(int year, int month, out DateTime startDate, out DateTime endDate)
        {
            startDate = new DateTime(year, 1, 1).Date;
            endDate = new DateTime(year, 12, 31).Date;
        }
    }
}