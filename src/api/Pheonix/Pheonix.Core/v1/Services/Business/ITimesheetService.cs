using Pheonix.DBContext;
using Pheonix.Models;
using Pheonix.Models.VM;
using Pheonix.Models.VM.Classes.Timesheet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Core.v1.Services.Business
{
    public interface ITimesheetService
    {
        Task<bool> SaveOrUpdate(TimesheetViewModel model, int id);

        Task<IEnumerable<GetTimesheetList_Result>> GetTimesheetList(int id);

        Task<TimesheetViewModel> GetTimesheet(int TaskId);
        Task<IEnumerable<BillableVsNonBillableViewModel>> GetBillableVsNonBillableClientRep(int ?duId, DateTime startDate, DateTime endDate);

        Task<IEnumerable<TimesheetCustomReportViewModel>> GetTimesheetCustomReportDefault(TimesheetCustomReportObject customReportObject);

        Task<IEnumerable<TimesheetCustomReportViewModel>> GetTimesheetCustomReportIndividualOverall(TimesheetCustomReportObject customReportObject);

        Task<IEnumerable<TimesheetCustomReportViewModel>> GetTimesheetCustomReportIndividualWithProject(TimesheetCustomReportObject customReportObject);

        Task<IEnumerable<TimesheetCustomReportViewModel>> GetTimesheetCustomReportDULevel(TimesheetCustomReportObject customReportObject);

        Task<IEnumerable<TimesheetCustomReportViewModel>> GetTimesheetCustomReportProjectLevel(TimesheetCustomReportObject customReportObject);

        Task<bool> SaveOrUpdateTimesheetForm(TimesheetViewModel model, int id);
        Task<bool> SaveMultipleEntryTimesheetForm(IEnumerable<TimesheetMultipleEntryViewModel> modelList, int id);

        HttpResponseMessage GetDownload(List<object> reportQueryParams);

        HttpResponseMessage GetTemplate(List<object> reportQueryParams, int userId);

        Task<string> Importdata(TimesheetViewModel tmmodel, int id);

        Task<IEnumerable<TimesheetViewModel>> SearchTimesheetList(TimesheetViewModel model, int id);
        Task<Dictionary<string, List<DropdownItems>>> GetDropdowns(int userId);
        Task<Dictionary<string, List<DropdownItems>>> TimeSheetLedgerDropdown(int userId);

        Task<IEnumerable<EmployeeProfileViewModel>> GetEmpList();

        Task<IEnumerable<GetLedgerReport_Result>> GetLedgerReport(DateTime reportStartDate, DateTime reportEndDate, int projectId, int? personId = null, int? subProjectId = null);

        Task<IEnumerable<GetSummaryReport_Result>> GetSummaryReport(DateTime reportStartDate, DateTime reportEndDate, int personId);
        Task<bool> ShowMyTeamTab(int id);
        Task<IEnumerable<EmployeeListForMyTeam>> GetEmpListByProjectManager(int id);
        Task<IEnumerable<TimesheetViewModel>> SearchTimesheetListByManagerProjectID(TimesheetMyTeamTabViewModel model, int id);

        Task<Dictionary<string, List<DropdownItems>>> TimeSheetLedgerReportDropdown(int userId);
        Task<IEnumerable<GetTotalHours_Available_Reported_Result>> TotalHoursAvailableReported(int deliveryUnitId, DateTime startDate, DateTime endDate);

        Task<IEnumerable<GetHoursForClientCompetency_Result>> GetHoursForClientCompetency(int deliveryUnitId, DateTime startDate, DateTime endDate);

        Task<IEnumerable<GetHoursForOrgActivity_Result>> GetHoursForOrgActivity(int deliveryUnitId, DateTime startDate, DateTime endDate);

        Task<IEnumerable<GetHoursForInvProject_Result>> GetHoursForInvProject(int deliveryUnitId, DateTime startDate, DateTime endDate);

        Task<IEnumerable<GetHoursForLearning_Result>> GetHoursForLearning(int deliveryUnitId, DateTime startDate, DateTime endDate);
        Task<IEnumerable<GetCompliance_Hours_Percentage_Result>> TotalHoursAvailableReported_Compliance_Percentage(int deliveryUnitId, DateTime startDate, DateTime endDate);
        Task<IEnumerable<TimesheetLeaveCount>> TotalLeaveCount(int deliveryUnitId, DateTime startDate, DateTime endDate);
    }
}

