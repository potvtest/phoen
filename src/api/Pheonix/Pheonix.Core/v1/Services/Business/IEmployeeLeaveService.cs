using Pheonix.DBContext;
using Pheonix.Models.Models;
using Pheonix.Models.VM;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pheonix.Core.v1.Services.Business
{
    public interface IEmployeeLeaveService
    {
        Task<LeaveViewModel<T>> GetLeaveDetails<T>(int userID, int location, int leaveType, DateTime fromDate, DateTime toDate, Func<List<PersonLeave>, List<PersonLeave>> trim = null);
        Task<LeaveViewModel<T>> GetLeaveDetailsForApproval<T>(int userID);
        //Task<EmployeeLeaveViewModel> GetLeaveDetails(int leaveType, int year);
        Task<IEnumerable<HolidayListViewModel>> GetHolidayList(int Location, int year);
        Task<HolidayListViewModel> GetUpcomingHoliday(int location, DateTime todayDate);
        Task<EnvelopeModel<EmployeeLeaveViewModel>> ApplyOrUpdateLeave(int userID, EmployeeLeaveViewModel model, int location);
        Task<EnvelopeModel<EmployeeLeaveViewModel>> ApproveOrRejectLeave(int userID, EmployeeLeaveViewModel model, int location);
        Task<EmployeeLeaveViewModel> ApproveLeave(int userID, int approverID, EmployeeLeaveViewModel model);
        Task<LeaveViewModel<T>> GetCompOffDetailsForApproval<T>(int userID);
        Task<ApprovalCompOffViewModel> ApproveCompOff(int userID, int approverID, ApprovalCompOffViewModel model);
        Task<LeaveViewModel<ApprovalLeaveViewModel>> GetleaveApprovalHistory(int userId);
        Task<Boolean> IsApprover(int userID, int type);
        Task<EnvelopeModel<EmployeeLeaveViewModel>> ApplyBirthdayLeave(int userID, EmployeeLeaveViewModel model, int location);
        Task<HolidaysListViewModel> GetHolidays(int year, int id);
        Task<int> GetEmploymentStatus(int PersonId);
        Task<IEnumerable<int?>> GetHolidayYear();
        Task<List<FHLeaveViewModel>> CheckFHLeaveAvailability(int personId);
        Task<bool> CheckSFHLeaveAvailability(int PersonId);
        Task<LocationSpecificLeavesViewModel> GetLocationSpecificLeaves(int PersonId);
        Task<string> ImportLeavesdata(string fileName, string leaveType);
        Task<bool> AddFHCheckListData(FHCheckListViewModel model);
    }
}