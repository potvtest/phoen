using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Pheonix.Models.VM;

namespace Pheonix.Core.v1.Services.Business
{
    public interface ISISOService
    {
        Task<AttendanceViewModel> GetAttendanceDetails(int userID, DateTime start, DateTime end);
        Task<AttendanceViewModel> GetTodaysAttendance(int userID, string timezone);
        Task<int> Add(int userId, SISOManualAutoViewModel model);
        Task<bool> AddBulkEntries(int userId, SISOManualAutoViewModel model);
        Task<IEnumerable<EmployeeSISOViewModel>> GetPendingListDateWise(int userID);
        Task<IEnumerable<EmployeeSISOViewModel>> GetPendingListUserWise(int userID);
        Task<PersonARStatus> ApproveSISO(int userID, SISOApprovalViewModel model);
        Task<PersonARStatus> RejectSISO(int userID, SISORejectViewModel model);
        Task<IEnumerable<SISOAttendanceRptDTOModel>> GetSISOAttendanceRpt(SISOAttendanceRptModel model);
        HttpResponseMessage DownloadSISOAttendanceRpt(SISOAttendanceRptModel model);
        HttpResponseMessage DownloadReport(int userID);
    }
}