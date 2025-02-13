using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pheonix.Models.VM;
using System.Net.Http;

namespace Pheonix.Web.Controllers.v1
{
    public interface IAttendanceContract
    {
        Task<AttendanceViewModel> ForLoggedInUser(string month);
        Task<AttendanceViewModel> ForLoggedInUser(string month, string year, string today);
        //HttpRequestMessage ForLoggedInUser1(string month);
        Task<AttendanceViewModel> ForUserWithID(int ID, string month, string year);
        Task<AttendanceViewModel> ForUserWithID(int user, DateTime start, DateTime end);
        Task<int> SISOAutoManual(SISOManualAutoViewModel model);

    }
}
