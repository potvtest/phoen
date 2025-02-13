using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using Pheonix.Core.v1.Services.Business;
using Pheonix.Models.Models;
using Pheonix.Models.VM;
using Pheonix.Web.Extensions;

namespace Pheonix.Web.Controllers.v1
{
    [RoutePrefix("v1/leave-management")]
    [Authorize]
    public class EmployeeLeaveController : ApiController
    {
        private readonly IEmployeeLeaveService _service;

        public EmployeeLeaveController(IEmployeeLeaveService service)
        {
            _service = service;
        }

        [HttpGet, Route("for-current-user/{leaveType:int}/{month:int}/{year:int}")]
        public async Task<LeaveViewModel<EmployeeLeaveViewModel>> GetLeaveDetails(int leaveType, int month, int year)
        {
            DateTime fromDate, toDate;
            GetStartDateEndDate(year, month, out fromDate, out toDate);
            return await _service.GetLeaveDetails<EmployeeLeaveViewModel>(GetUserId(), GetCountry(), leaveType, fromDate, toDate);
        }

        [HttpGet, Route("for-search-user/{leaveType:int}/{month:int}/{year:int}/{user:int}")]
        public async Task<LeaveViewModel<EmployeeLeaveViewModel>> GetSsearchLeaveDetails(int leaveType, int month, int year, int user)
        {
            DateTime fromDate, toDate;
            GetStartDateEndDate(year, month, out fromDate, out toDate);
            return await _service.GetLeaveDetails<EmployeeLeaveViewModel>(user, GetCountry(), leaveType, fromDate, toDate);
        }

        [HttpGet, Route("for-current-user")]
        public async Task<LeaveViewModel<EmployeeLeaveViewModel>> GetLeaveDetails()
        {
            DateTime fromDate, toDate;
            GetStartDateEndDate(DateTime.Now.Year, 0, out fromDate, out toDate);
            return await _service.GetLeaveDetails<EmployeeLeaveViewModel>(GetUserId(), GetCountry(), 0, fromDate, toDate);
        }

        [HttpGet, Route("Leaves-Based-On-Location/{userID:int?}")]
        public async Task<LocationSpecificLeavesViewModel> GetLeavesBasedOnLocation(int userID = 0)
            => await _service.GetLocationSpecificLeaves(userID == 0 ? GetUserId() : userID);

        [HttpGet, Route("current-year-holidaylist/{location:int}/{year:int}")]
        public async Task<IEnumerable<HolidayListViewModel>> GetHolidayList(int location, int year)
            => await _service.GetHolidayList(location, year);

        [HttpGet, Route("upcoming-holiday/{location:int}")]
        public async Task<HolidayListViewModel> GetUpcomingHoliday(int location)
            => await _service.GetUpcomingHoliday(location, DateTime.Now.Date);

        [HttpPost, Route("apply-or-update-leave")]
        public async Task<EnvelopeModel<EmployeeLeaveViewModel>> ApplyOrUpdateLeave(EmployeeLeaveViewModel model)
        {
            if (!((new int[] { 0, 1, 2, 3, 4, 6, 7, 8, 9, 11 }).Contains(model.LeaveType)))
                return new EnvelopeModel<EmployeeLeaveViewModel>(null, true, "You are entering the wrong LeaveType. You are allowed for Comp-Off, Leave, LWP, Maternity/Paternity");

            return await _service.ApplyOrUpdateLeave(GetUserId(), model, GetCountry());
        }

        [HttpPost, Route("approve-or-reject-leave")]
        public async Task<EnvelopeModel<EmployeeLeaveViewModel>> ApproveOrUejectLeave(EmployeeLeaveViewModel model)
        {
            if (!((new int[] { 0, 1, 2, 3, 4, 6, 7, 8, 9, 11 }).Contains(model.LeaveType)))
                return new EnvelopeModel<EmployeeLeaveViewModel>(null, true, "You are entering the wrong LeaveType. You are allowed for Comp-Off, Leave, LWP, Maternity/Paternity");

            return await _service.ApproveOrRejectLeave(GetUserId(), model, GetCountry());
        }

        [HttpGet, Route("IsApprover/{type}")]
        public async Task<Boolean> IsApprover(int type) => await _service.IsApprover(GetUserId(), type);

        [HttpPost, Route("apply-birthday-leave")]
        public async Task<EnvelopeModel<EmployeeLeaveViewModel>> ApplyBirthdayLeave(EmployeeLeaveViewModel model)
            => await _service.ApplyBirthdayLeave(GetUserId(), model, GetCountry());

        [HttpGet, Route("current-year-holidays/{year:int}/{id:int}")]
        public async Task<HolidaysListViewModel> GetHolidays(int year, int id)
           => await _service.GetHolidays(year, id);

        [HttpGet, Route("employmentstatus")]
        public async Task<int> GetEmploymentStatus() => await _service.GetEmploymentStatus(GetUserId());

        [HttpGet, Route("get-holiday-year")]
        public async Task<IEnumerable<int?>> GetHolidayYear() => await _service.GetHolidayYear();

        [HttpGet, Route("check-fhleave-availability/{personId:int}")]
        public async Task<List<FHLeaveViewModel>> CheckFHLeaveAvailability(int personId)
            => await _service.CheckFHLeaveAvailability(personId);

        [HttpGet, Route("check_sfhleave_availability/{personId:int}")]
        public async Task<bool> CheckSFHLeaveAvailability(int personId)
            => await _service.CheckSFHLeaveAvailability(personId);

        [HttpPost, Route("LeavesBulkUpload")]
        public async Task<string> UploadLeaves(LeavesToCreditViewModel objLeavesToCreditViewModel)
        {
            string result = await _service.ImportLeavesdata(objLeavesToCreditViewModel.fileName, objLeavesToCreditViewModel.LeaveType);
            if (result == "true")
                return result;
            throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.NotFound, result));
        }

        public void GetStartDateEndDate(int year, int month, out DateTime startDate, out DateTime endDate)
        {
            startDate = new DateTime(year, 1, 1).Date;
            endDate = new DateTime(year, 12, 31).Date;
        }

        private int GetUserId() => RequestContext.GetClaimInt(ClaimTypes.PrimarySid);
        private int GetCountry() => RequestContext.GetClaimInt(ClaimTypes.Country);

        [HttpPost, Route("add-fh-checklist-data")]
        public async Task<bool> AddFHCheckListData(FHCheckListViewModel model)
          => await _service.AddFHCheckListData(model);
    }
}