using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using Pheonix.Core.v1.Services.Business;
using Pheonix.Models.VM;
using Pheonix.Web.Extensions;

namespace Pheonix.Web.Controllers.v1
{
    [RoutePrefix("v1/attendance")]
    public class SISOController : ApiController, IAttendanceContract
    {
        private readonly ISISOService _sisoService;

        public SISOController(ISISOService sisoService)
        {
            _sisoService = sisoService;
        }

        [HttpGet, Route("for-current-user/{month}")]
        public async Task<AttendanceViewModel> ForLoggedInUser(string month)
        {
            DateTime StartDate, EndDate = DateTime.Now;
            GetStartDateEndDate(Convert.ToInt32(month), DateTime.Now.Year, out StartDate, out EndDate);
            return await ForUserWithID(GetUserId(), StartDate, EndDate);
        }

        [HttpGet, Route("for-current-user/{month}/{year}/{today}")]
        public async Task<AttendanceViewModel> ForLoggedInUser(string month, string year, string today)
        {
            DateTime startDate;
            DateTime Today = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(Double.Parse(today));
            GetStartDateEndDate(Convert.ToInt32(month), Convert.ToInt32(year), out startDate, out Today);
            return await ForUserWithID(GetUserId(), startDate, Today);
        }

        [HttpGet, Route("for-search-user/{month}/{year}/{today}/{user}")]
        public async Task<AttendanceViewModel> ForSearchUser(string month, string year, string today, int user)
        {
            DateTime startDate;
            DateTime Today = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(Double.Parse(today));
            GetStartDateEndDate(Convert.ToInt32(month), Convert.ToInt32(year), out startDate, out Today);
            return await ForUserWithID(user, startDate, Today);
        }

        [HttpGet, Route("for-current-user/todays-login/{timezone}")]
        public async Task<AttendanceViewModel> ForLoggedInUser1(string timezone)
            => await _sisoService.GetTodaysAttendance(GetUserId(), timezone);

        [HttpGet, Route("for-user-{ID}/{month}/{year}")]
        public async Task<AttendanceViewModel> ForUserWithID(int ID, string month, string year)
        {
            DateTime startDate, endDate = DateTime.Now;
            GetStartDateEndDate(Convert.ToInt32(month), Convert.ToInt32(year), out startDate, out endDate);
            return await ForUserWithID(GetUserId(), startDate, endDate);
        }

        public async Task<AttendanceViewModel> ForUserWithID(int user, DateTime start, DateTime end)
            => await _sisoService.GetAttendanceDetails(user, start, end);

        public void GetStartDateEndDate(int month, int year, out DateTime startDate, out DateTime endDate)
        {
            startDate = new DateTime(year, month, 1).Date;
            endDate = startDate.Date.AddMonths(1).AddDays(-1).Date;
        }

        [HttpPost, Route("add")]
        public async Task<int> SISOAutoManual(SISOManualAutoViewModel model)
            => await _sisoService.Add(GetUserId(), model);

        [HttpPost, Route("addbulkentries")]
        public async Task<bool> AddBulkEntries(SISOManualAutoViewModel model)
            => await _sisoService.AddBulkEntries(GetUserId(), model);

        [Route("getpendinglistdatewise"), HttpGet]
        public async Task<IEnumerable<EmployeeSISOViewModel>> GetPendingListDateWise()
            => await _sisoService.GetPendingListDateWise(GetUserId());

        [Route("getpendinglistuserwise"), HttpGet]
        public async Task<IEnumerable<EmployeeSISOViewModel>> GetPendingListUserWise()
             => await _sisoService.GetPendingListUserWise(GetUserId());

        [HttpPost, Route("approve")]
        public async Task<PersonARStatus> ApproveSISO(SISOApprovalViewModel model)
            => await _sisoService.ApproveSISO(GetUserId(), model);

        [HttpPost, Route("reject")]
        public async Task<PersonARStatus> RejectSISO(SISORejectViewModel model)
            => await _sisoService.RejectSISO(GetUserId(), model);

        [HttpPost, Route("getsisoattendancerpt")]
        public async Task<IEnumerable<SISOAttendanceRptDTOModel>> GetSISOAttendanceRpt(SISOAttendanceRptModel model)
            => await _sisoService.GetSISOAttendanceRpt(model);

        [HttpPost, Route("downloadsisoattendancerpt")]
        public HttpResponseMessage DownloadSISOAttendanceRpt(SISOAttendanceRptModel model) 
            => _sisoService.DownloadSISOAttendanceRpt(model);

        [HttpGet, Route("downloadreport")]
        public HttpResponseMessage DownloadReport() 
            => _sisoService.DownloadReport(GetUserId());

        private int GetUserId() => RequestContext.GetClaimInt(ClaimTypes.PrimarySid);
    }
}