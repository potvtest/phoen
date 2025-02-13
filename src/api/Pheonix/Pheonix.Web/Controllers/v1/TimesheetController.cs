using Pheonix.Core.v1.Services.Business;
using Pheonix.Models.VM.Classes.Timesheet;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Security.Claims;
using Pheonix.Web.Extensions;
using System.Web.Http;
using Pheonix.Models;
using Pheonix.Models.VM;
using Pheonix.DBContext;
using log4net;
using System.Reflection;

namespace Pheonix.Web.Controllers.v1
{
    [RoutePrefix("v1/timesheet"), Authorize]
    public class TimesheetController : ApiController
    {
        private ITimesheetService service;
        static string fileUrl = ConfigurationManager.AppSettings["UploadedFileUrl"].ToString();
        private static readonly ILog Log4Net = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public TimesheetController(ITimesheetService service)
        {
            this.service = service;
        }

        [Route("save-update"), HttpPost]
        public async Task<bool> SaveOrUpdate(TimesheetViewModel model)
        {
            return await service.SaveOrUpdate(model, RequestContext.GetClaimInt(ClaimTypes.PrimarySid));
        }

        [Route("save-update-form"), HttpPost]
        public async Task<bool> SaveOrUpdateTimesheetForm(TimesheetViewModel model)
        {
            return await service.SaveOrUpdateTimesheetForm(model, RequestContext.GetClaimInt(ClaimTypes.PrimarySid));
        }

        [Route("save-multiple-entry-form"), HttpPost]
        public async Task<IHttpActionResult> SaveMultipleEntryTimesheetForm(List<TimesheetMultipleEntryViewModel> modelList)
        {
            try
            {
                bool result = await service.SaveMultipleEntryTimesheetForm(modelList, RequestContext.GetClaimInt(ClaimTypes.PrimarySid));
                if(result == true)
                    return Content(HttpStatusCode.OK, "Timesheet entries updated successfully");
                else
                    return Content(HttpStatusCode.OK, "Timesheet entries for a day either exceeds 24 hours or is 0.Kindly check existing and new timesheet entries.");
            }
            catch (Exception ex)
            {
                Log4Net.Error("Error while inserting multiple entry Timesheet: Inner Exception: " + ex.InnerException + "StackTrace :" + ex.StackTrace);
                return Content(HttpStatusCode.InternalServerError, "Error while inserting multiple Timesheet entries: Inner Exception: " + ex.InnerException);
            }
        }

        [Route("get-all-list"), HttpGet]
        public async Task<IEnumerable<GetTimesheetList_Result>> GetTimesheetList()
            => await service.GetTimesheetList(RequestContext.GetClaimInt(ClaimTypes.PrimarySid));


        [Route("get-search-list"), HttpPost]
        public async Task<IEnumerable<TimesheetViewModel>> SearchTimesheetList(TimesheetViewModel model)
        {
            return await service.SearchTimesheetList(model, RequestContext.GetClaimInt(ClaimTypes.PrimarySid));
        }

        [Route("get-timesheet/{timesId:int?}"), HttpGet]
        public async Task<TimesheetViewModel> GetTimesheet(int timesId = 0)
        {
            return await service.GetTimesheet(timesId);
        }

        [Route("get-nonbillorbill-client-report"), HttpGet]
        public async Task<IHttpActionResult> GetBillableVsNonbillableClientReport(int? deliveryUnitId, DateTime startDate, DateTime endDate)
        {
            try
            {
                var sDate = new DateTime(startDate.Year, startDate.Month, startDate.Day);
                var eDate = new DateTime(endDate.Year, endDate.Month, endDate.Day);
                var dateToday = new DateTime(DateTime.Today.Year,DateTime.Today.Month,DateTime.Today.Day);
                if((eDate - dateToday).Days == 0)
                {
                    eDate = eDate.AddDays(-1);
                }
                var diffDays = (eDate - sDate).Days;
                var result = new List<BillableVsNonBillableViewModel>();
                if(diffDays > 90 || eDate < sDate)
                {
                    result = null;
                }
                else
                {
                    result = (List<BillableVsNonBillableViewModel>)await service.GetBillableVsNonBillableClientRep(deliveryUnitId, sDate, eDate);
                }
            
                if (result != null)
                    return Content(HttpStatusCode.OK, result);   

                return Content(HttpStatusCode.NotFound, "No record found!");
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.InternalServerError, "Error in fetching Billable vs NonBillable Client Project report:" + ex.InnerException);
            }
        }

        [Route("get-timesheet-custom-report"), HttpGet]
        public async Task<IHttpActionResult> GetTimesheetCustomReport(int type, int? duID, int? projectID, int? subProjectID, int? employeeID, int? billable, DateTime startDate, DateTime endDate)
        {
            try
            {
                TimesheetCustomReportObject customReportObject = new TimesheetCustomReportObject();
                customReportObject.type = type;
                customReportObject.duID = duID;
                customReportObject.projectID = projectID;
                customReportObject.subProjectID = subProjectID;
                customReportObject.employeeID = employeeID;
                customReportObject.billable = billable;
                customReportObject.startDate = new DateTime(startDate.Year, startDate.Month, startDate.Day);
                customReportObject.endDate = new DateTime(endDate.Year, endDate.Month, endDate.Day);

                if (customReportObject.type == 1)
                {
                    var result = await service.GetTimesheetCustomReportDefault(customReportObject);
                    if (result != null)
                        return Content(HttpStatusCode.OK, result);
                }
                else if (customReportObject.type == 2)
                {
                    var result = await service.GetTimesheetCustomReportIndividualOverall(customReportObject);
                    if (result != null)
                        return Content(HttpStatusCode.OK, result);
                }
                else if (customReportObject.type == 3)
                {
                    var result = await service.GetTimesheetCustomReportIndividualWithProject(customReportObject);
                    if (result != null)
                        return Content(HttpStatusCode.OK, result);
                }
                else if (customReportObject.type == 4)
                {
                    var result = await service.GetTimesheetCustomReportDULevel(customReportObject);
                    if (result != null)
                        return Content(HttpStatusCode.OK, result);
                }
                else if (customReportObject.type == 5)
                {
                    var result = await service.GetTimesheetCustomReportProjectLevel(customReportObject);
                    if (result != null)
                        return Content(HttpStatusCode.OK, result);
                }

                return Content(HttpStatusCode.NotFound, "No record found!");
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.InternalServerError, "Error in fetching Custom timesheet report:" + ex.InnerException);
            }
        }

        [Route("get-emp-list"), HttpGet]
        public async Task<IEnumerable<EmployeeProfileViewModel>> GetEmplist()
        {
            return await service.GetEmpList();
        }

        [HttpPost, Route("download")]
        public HttpResponseMessage DownloadReport([FromBody] List<object> reportQueryParams)
        {
            return service.GetDownload(reportQueryParams);
        }

        [HttpPost, Route("template")]
        public HttpResponseMessage DownloadTemplate([FromBody] List<object> reportQueryParams)
        {
            return service.GetTemplate(reportQueryParams, (RequestContext.GetClaimInt(ClaimTypes.PrimarySid)));
        }

        [Route("dropdowns"), HttpGet]
        public async Task<IHttpActionResult> GetDropDowns()
        {
            return Ok(await service.GetDropdowns(RequestContext.GetClaimInt(ClaimTypes.PrimarySid)));
        }

        [Route("TimeSheetLedgerDropdown"), HttpGet]
        public async Task<IHttpActionResult> TimeSheetLedgerDropdown()
        {
            return Ok(await service.TimeSheetLedgerDropdown(RequestContext.GetClaimInt(ClaimTypes.PrimarySid)));
        }

        [HttpPost, Route("import")]
        public async Task<string> ImportData(TimesheetViewModel t)
        {
            try
            {
                return await service.Importdata(t, RequestContext.GetClaimInt(ClaimTypes.PrimarySid));
            }
            catch (Exception ex)
            {
                return $"An error occurred during data import: {ex.Message}";
            }
        }

        [Route("getledgerreport"), HttpGet]
        public async Task<IEnumerable<GetLedgerReport_Result>> GetLedgerReport(DateTime reportStartDate, DateTime reportEndDate, int projectId, int? personId = null, int? subProjectId = null)
    => await service.GetLedgerReport(reportStartDate, reportEndDate, projectId, personId, subProjectId);

        [Route("getsummaryreport"), HttpGet]
        public async Task<IEnumerable<GetSummaryReport_Result>> GetSummaryReport(DateTime reportStartDate, DateTime reportEndDate, int personId)
    => await service.GetSummaryReport(reportStartDate, reportEndDate, personId);

        [Route("UpdationRecords/{personID}"), HttpPost]
        public async Task<List<TimesheetViewModel>> UpdationRecords(int personID, List<TimesheetViewModel> model)
        {
            var context = new PhoenixEntities();
            foreach (var item in model)
            {
                var result = context.PMSTimesheet.Where(x => x.ID == item.ID).FirstOrDefault();
                if (result != null)
                {
                    result.TicketID = item.TicketID;
                    result.Time = item.TotalHours;
                    result.NonBillableTime = item.NonBillableTime;
                    result.Description = item.Description;
                    result.NonBillableDescription = item.NonBillableDescription;
                    await context.SaveChangesAsync();
                }
            }
            return model;
        }

        [HttpGet, Route("show-myteam-tab")]
        public async Task<IHttpActionResult> ShowMyTeamTab()
        {
            try
            {
                var isSuccess = await service.ShowMyTeamTab(RequestContext.GetClaimInt(ClaimTypes.PrimarySid));
                return Ok(new
                {
                    isSuccess,
                });
            }
            catch (Exception ex)
            {
                Log4Net.Error("Error while loading Show My Team Tab Page: Inner Exception: " + ex.InnerException + "StackTrace :" + ex.StackTrace);
                return Content(HttpStatusCode.InternalServerError, "Error while loading Show My Team Tab Page: Inner Exception" + ex.InnerException);
            }
        }

        [HttpGet, Route("employee-list-by-projectManager")]
        public async Task<IHttpActionResult> GetEmpListByProjectManager()
        {
            try
            {
                var empListbyPM = await service.GetEmpListByProjectManager(RequestContext.GetClaimInt(ClaimTypes.PrimarySid));
                return Ok(new
                {
                    empListbyPM,
                });
            }
            catch (Exception ex)
            {
                Log4Net.Error("Error while retriving Employee list filter by Project Manager: Inner Exception: " + ex.InnerException + "StackTrace :" + ex.StackTrace);
                return Content(HttpStatusCode.InternalServerError, "Error while retriving Employee list filter by Project Manager: Inner Exception" + ex.InnerException);
            }
        }
        [Route("get-search-list-bymanagerprojectid"), HttpPost]
        public async Task<IEnumerable<TimesheetViewModel>> SearchTimesheetListByManagerProjectID(TimesheetMyTeamTabViewModel model)
        {
            return await service.SearchTimesheetListByManagerProjectID(model, RequestContext.GetClaimInt(ClaimTypes.PrimarySid));
        }

        [Route("TimeSheetLedgerReportDropdown"), HttpGet]
        public async Task<IHttpActionResult> TimeSheetLedgerReportDropdown()
        {
            return Ok(await service.TimeSheetLedgerReportDropdown(RequestContext.GetClaimInt(ClaimTypes.PrimarySid)));
        }
        [Route("TotalHoursAvailableReported"), HttpGet]
        public async Task<IEnumerable<GetTotalHours_Available_Reported_Result>> TotalHoursAvailableReported(int deliveryUnitId, DateTime startDate,DateTime endDate)
        {
            return await service.TotalHoursAvailableReported(deliveryUnitId, startDate,endDate);
        }

        [Route("get-hours-clientCompetency"), HttpGet]
        public async Task<IEnumerable<GetHoursForClientCompetency_Result>> GetHoursForClientCompetency(int deliveryUnitId, DateTime startDate, DateTime endDate)
        {
            return await service.GetHoursForClientCompetency(deliveryUnitId, startDate, endDate);
        }

        [Route("get-hours-orgActivity"), HttpGet]
        public async Task<IEnumerable<GetHoursForOrgActivity_Result>> GetHoursForOrgActivity(int deliveryUnitId, DateTime startDate, DateTime endDate)
        {
            return await service.GetHoursForOrgActivity(deliveryUnitId, startDate, endDate);
        }

        [Route("get-hours-invProject"), HttpGet]
        public async Task<IEnumerable<GetHoursForInvProject_Result>> GetHoursForInvProject(int deliveryUnitId, DateTime startDate, DateTime endDate)
        {
            return await service.GetHoursForInvProject(deliveryUnitId, startDate, endDate);
        }

        [Route("get-hours-forLearning"), HttpGet]
        public async Task<IEnumerable<GetHoursForLearning_Result>> GetHoursForLearning(int deliveryUnitId, DateTime startDate, DateTime endDate)
        {
            return await service.GetHoursForLearning(deliveryUnitId, startDate, endDate);
        }
        [Route("TotalHoursAvailableReported_Compliance_Percentage"), HttpGet]
        public async Task<IEnumerable<GetCompliance_Hours_Percentage_Result>> TotalHoursAvailableReported_Compliance_Percentage(int deliveryUnitId, DateTime startDate, DateTime endDate)
        {
            return await service.TotalHoursAvailableReported_Compliance_Percentage(deliveryUnitId, startDate, endDate);
        }
        [Route("TotalLeaveCount"), HttpGet]
        public async Task<IEnumerable<TimesheetLeaveCount>> TotalLeaveCount(int deliveryUnitId, DateTime startDate, DateTime endDate)
        {
            return await service.TotalLeaveCount(deliveryUnitId, startDate, endDate);
        }
    }
}
