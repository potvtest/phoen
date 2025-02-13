using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pheonix.Models.VM;

namespace Pheonix.Web.Controllers.v1
{
    public interface IEmployeeLeaveContract<T>
    {
        Task<LeaveViewModel<T>> GetLeaveDetails<T>(int leaveType, int month, int year);
        Task<LeaveViewModel<T>> GetLeaveDetails<T>();
        Task<IEnumerable<HolidayListViewModel>> GetHolidayList(int Location, int year);
        Task<EmployeeLeaveViewModel> ApplyOrUpdateLeave(EmployeeLeaveViewModel model);
        Task<EmployeeLeaveViewModel> ApproveLeave(int empidid, EmployeeLeaveViewModel model);
    }
}
