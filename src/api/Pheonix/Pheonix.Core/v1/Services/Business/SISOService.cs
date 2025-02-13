using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using AutoMapper;
using Pheonix.Core.Helpers;
using Pheonix.Core.v1.Services.Email;
using Pheonix.DBContext;
using Pheonix.Models.VM;

namespace Pheonix.Core.v1.Services.Business
{
    public class SISOService : ISISOService
    {
        #region Class level variables

        private IBasicOperationsService service;
        private readonly IEmailService _emailService;

        #endregion Class level variables

        #region Constructor

        public SISOService(IBasicOperationsService opsService, IEmailService opsEmailService)
        {
            service = opsService;
            _emailService = opsEmailService;
        }

        #endregion Constructor

        #region Public Contracts

        public async Task<AttendanceViewModel> GetAttendanceDetails(int userID, DateTime start, DateTime end)
        {
            return await Task.Run(() =>
            {
                var viewModel = new AttendanceViewModel();

                int personLocation = (int)service.First<Person>(x => x.ID == userID).PersonEmployment.FirstOrDefault().OfficeLocation;
                var empSISO = service.Top<SignInSignOut>(0, x => (x.UserID == userID) && (DbFunctions.TruncateTime(x.AttendanceDate) >= start.Date && DbFunctions.TruncateTime(x.AttendanceDate) <= end.Date))
                             .OrderByDescending(a => a.SignInTime.HasValue ? a.SignInTime : a.SignOutTime).ToList();
                var empTodaySignIn = empSISO.Where(x => x.SignInTime != null ? x.SignInTime.Value.Date == DateTime.Now.Date : DateTime.MinValue == DateTime.Now.Date).ToList();
                var empTodaySignOut = empSISO.Where(a => a.SignOutTime != null ? a.SignOutTime.Value.Date == DateTime.Now.Date : DateTime.MinValue == DateTime.Now.Date).ToList();
                IEnumerable<EmployeeSISOViewModel> lstEmployeeSISOResult = new List<EmployeeSISOViewModel>();
                if (personLocation == 2)
                {
                    var siso = empSISO.Where(x => String.IsNullOrEmpty(x.DayNotation) || x.DayNotation.ToUpper() == "P" || x.DayNotation.ToUpper() == "R").ToList();
                    if (siso.Count() != 0)
                    {
                        lstEmployeeSISOResult = Mapper.Map<IEnumerable<SignInSignOut>, IEnumerable<EmployeeSISOViewModel>>(siso);
                    }
                }
                else
                {
                    var siso = empSISO.Where(x => String.IsNullOrEmpty(x.DayNotation) || x.DayNotation.ToUpper() == "P" || x.DayNotation.ToUpper() == "R" || x.DayNotation == "A").ToList();
                    if (siso.Count() != 0)
                    {
                        lstEmployeeSISOResult = Mapper.Map<IEnumerable<SignInSignOut>, IEnumerable<EmployeeSISOViewModel>>(siso);
                    }
                }

                viewModel.TodayStatus = new TodayStatus();
                viewModel.TodayStatus.IsSignIn = empTodaySignIn.Count() > 0 ? true : false;
                viewModel.TodayStatus.IsSignOut = empTodaySignOut.Count() > 0 ? true : false;
                viewModel.JoiningDate = (DateTime)service.First<Person>(x => x.ID == userID).PersonEmployment.FirstOrDefault().JoiningDate;
                var holidayList = service.Top<HolidayList>(0, a => a.Location == personLocation && a.IsDeleted == false && a.Date.Value.Month == start.Date.Month && a.HolidayYear == start.Year);
                viewModel.CurrentMonthHolidays = Mapper.Map<IEnumerable<HolidayList>, IEnumerable<HolidayListViewModel>>(holidayList);
                viewModel.HolidayCount = holidayList.Count();
                var personLeaveList = service.Top<PersonLeave>(0, x => x.Person.ID == userID && x.IsDeleted == false && x.FromDate.Month == start.Date.Month && x.FromDate.Year == start.Date.Year && x.Status == 2);
                DateTime lastDayOfMonth = new DateTime(start.Date.Year, start.Date.Month, DateTime.DaysInMonth(start.Date.Year, start.Date.Month));
                DateTime firstDayOfMonth = new DateTime(start.Date.Year, start.Date.Month, 1);
                DateTime today = DateTime.Today;
                DateTime firstDayOfCurrentMonth = new DateTime(today.Date.Year, today.Date.Month, 1);
                List<DateTime> allDates = new List<DateTime>();
                List<EmployeeSISOViewModel> lstEmployeeSISOViewModel = new List<EmployeeSISOViewModel>();
                foreach (var item in personLeaveList.ToList())
                {
                    for (DateTime date = item.FromDate; date <= item.ToDate; date = date.AddDays(1))
                    {

                        if (date.DayOfWeek >= DayOfWeek.Monday && date.DayOfWeek <= DayOfWeek.Friday && !holidayList.Any(d => d.Date.Value.Month == date.Month && d.Date.Value.Day == date.Day) && !lstEmployeeSISOResult.Any(d => d.AttendanceDate.Month == date.Month && d.AttendanceDate.Day == date.Day) && personLocation != 2)
                        {
                            EmployeeSISOViewModel objEmployeeSISOViewModel = new EmployeeSISOViewModel();
                            objEmployeeSISOViewModel.DayNotation = "L";
                            objEmployeeSISOViewModel.AttendanceDate = date;
                            objEmployeeSISOViewModel.Narration = "On Leave";
                            lstEmployeeSISOViewModel.Add(objEmployeeSISOViewModel);
                        }
                        if (date.DayOfWeek >= DayOfWeek.Monday && date.DayOfWeek <= DayOfWeek.Friday && !holidayList.Any(d => d.Date.Value.Month == date.Month && d.Date.Value.Day == date.Day) && !lstEmployeeSISOResult.Any(d => d.AttendanceDate.Month == date.Month && d.AttendanceDate.Day == date.Day))
                        {
                            allDates.Add(date);
                        }
                    }
                    viewModel.DateOfLeave = allDates;
                    int businessDays = GetBusinessDays(item.FromDate, lastDayOfMonth, holidayList);
                    if (item.Leaves <= businessDays)
                    {
                        viewModel.LeaveCount += (int)item.Leaves;
                        for (DateTime index = item.FromDate; index <= item.ToDate; index = index.AddDays(1))
                        {
                            var tempSISO = service.First<SignInSignOut>(x => x.UserID == userID && x.SignInTime.Value.Day == index.Day && x.SignInTime.Value.Month == index.Month && x.SignInTime.Value.Year == index.Year && x.DayNotation == "P");
                            if (tempSISO != null)
                            {
                                viewModel.LeaveCount--;
                            }
                        }
                    }
                    else
                    {
                        viewModel.LeaveCount += businessDays;
                        for (DateTime index = item.FromDate; index <= item.ToDate; index = index.AddDays(1))
                        {
                            var tempSISO = service.First<SignInSignOut>(x => x.UserID == userID && x.SignInTime.Value.Day == index.Day && x.SignInTime.Value.Month == index.Month && x.SignInTime.Value.Year == index.Year && x.DayNotation == "P");
                            if (tempSISO != null)
                            {
                                viewModel.LeaveCount--;
                            }
                        }
                    }

                    if (end > firstDayOfCurrentMonth)
                    {
                        for (DateTime index = item.FromDate; index <= item.ToDate; index = index.AddDays(1))
                        {
                            if (index.DayOfWeek != DayOfWeek.Saturday && index.DayOfWeek != DayOfWeek.Sunday)
                            {
                                if (firstDayOfMonth.Day <= index.Day && lastDayOfMonth.Day >= index.Day)
                                {
                                    if (index <= today)
                                    {
                                        if (holidayList.Count() == 0)
                                        {
                                            //viewModel.LeaveCount--; //For: #143855025 on 18/08/2017--If employee present on approved leave it should not show leave count in negative on Attendance page
                                            viewModel.PreviousLeaveCount++;
                                        }
                                        else
                                        {
                                            //viewModel.LeaveCount--; //For: #143855025 on 18/08/2017--If employee present on approved leave it should not show leave count in negative on Attendance page
                                            viewModel.PreviousLeaveCount++;
                                            foreach (var holiday in holidayList)
                                            {
                                                if (holiday.Date == index)
                                                {
                                                    //viewModel.LeaveCount++;
                                                    viewModel.PreviousLeaveCount--;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        if (item.NotConsumed != null)
                        {
                            viewModel.PreviousLeaveCount = viewModel.PreviousLeaveCount - item.NotConsumed.Value;
                        }
                    }

                }
                var previousMonthLeaveList = service.Top<PersonLeave>(0, x => x.Person.ID == userID && x.IsDeleted == false && x.ToDate.Month == start.Date.Month && x.FromDate.Month != start.Date.Month && x.FromDate.Year == start.Date.Year && x.Status == 2);
                foreach (var item in previousMonthLeaveList)
                {
                    int businessDays = GetBusinessDays(firstDayOfMonth, item.ToDate, holidayList);
                    if (item.Leaves <= businessDays)
                        viewModel.LeaveCount += (int)item.Leaves;
                    else
                        viewModel.LeaveCount += businessDays;
                    if (end > firstDayOfCurrentMonth)
                    {
                        for (DateTime index = item.FromDate; index <= item.ToDate; index = index.AddDays(1))
                        {
                            if (index.DayOfWeek >= DayOfWeek.Monday && index.DayOfWeek <= DayOfWeek.Friday && !holidayList.Any(d => d.Date.Value.Month == index.Month && d.Date.Value.Day == index.Day) && !lstEmployeeSISOResult.Any(d => d.AttendanceDate.Month == index.Month && d.AttendanceDate.Day == index.Day) && personLocation != 2)
                            {
                                EmployeeSISOViewModel objEmployeeSISOViewModel = new EmployeeSISOViewModel();
                                objEmployeeSISOViewModel.DayNotation = "L";
                                objEmployeeSISOViewModel.AttendanceDate = index;
                                objEmployeeSISOViewModel.Narration = "On Leave";
                                lstEmployeeSISOViewModel.Add(objEmployeeSISOViewModel);
                            }
                            if (index.DayOfWeek != DayOfWeek.Saturday && index.DayOfWeek != DayOfWeek.Sunday)
                            {
                                if (firstDayOfMonth.Day <= index.Day && lastDayOfMonth.Day >= index.Day)
                                {
                                    if (index <= today)
                                    {
                                        if (holidayList.Count() == 0)
                                        {
                                            //viewModel.LeaveCount--;  //For: #143855025 on 18/08/2017--If employee present on approved leave it should not show leave count in negative on Attendance page
                                            viewModel.PreviousLeaveCount++;
                                        }
                                        else
                                        {
                                            //viewModel.LeaveCount--;  //For: #143855025 on 18/08/2017--If employee present on approved leave it should not show leave count in negative on Attendance page
                                            viewModel.PreviousLeaveCount++;
                                            foreach (var holiday in holidayList)
                                            {
                                                if (holiday.Date == index)
                                                {
                                                    //viewModel.LeaveCount++;
                                                    viewModel.PreviousLeaveCount--;
                                                }
                                            }
                                        }
                                    }
                                }
                                var tempSISO = service.First<SignInSignOut>(x => x.UserID == userID && x.SignInTime.Value == index.Date && x.DayNotation == "P");
                                if (tempSISO != null)
                                {
                                    viewModel.LeaveCount--;
                                }
                            }
                        }
                    }
                    if (item.NotConsumed != null)
                    {
                        viewModel.PreviousLeaveCount = viewModel.PreviousLeaveCount - item.NotConsumed.Value;
                        for (DateTime index = item.FromDate; index < firstDayOfCurrentMonth; index = index.AddDays(1))
                        {
                            var tempSISO = service.First<SignInSignOut>(x => x.UserID == userID && x.SignInTime.Value == index.Date && x.DayNotation == "P");
                            if (tempSISO != null)
                            {
                                viewModel.PreviousLeaveCount++;
                            }
                        }
                    }
                }

                viewModel.EmployeeSISOViewModels = lstEmployeeSISOViewModel.Concat(lstEmployeeSISOResult).Where(x => x.AttendanceDate <= DateTime.Now).OrderByDescending(x => x.AttendanceDate);

                viewModel.LeaveCount = 0;
                viewModel.PresentCount = 0;
                viewModel.AbsentCount = 0;
                if (viewModel.EmployeeSISOViewModels.Count() != 0)
                {
                    foreach (var item in viewModel.EmployeeSISOViewModels)
                    {
                        if (item.DayNotation == "P" && item.AttendanceDate.DayOfWeek >= DayOfWeek.Monday && item.AttendanceDate.DayOfWeek <= DayOfWeek.Friday && !holidayList.Any(d => d.Date.Value.Month == item.AttendanceDate.Month && d.Date.Value.Day == item.AttendanceDate.Day))
                        {
                            viewModel.PresentCount = viewModel.PresentCount + 1;
                        }
                        if (item.DayNotation == "L")
                        {
                            viewModel.LeaveCount = viewModel.LeaveCount + 1;
                        }
                    }
                }

                // ND: This value needs to be fetched from database once the absent job is done.

                viewModel.AbsentCount = empSISO.Where(x => x.DayNotation == "A" || x.DayNotation == "R").Count();
                return viewModel;
            });
        }

        public async Task<AttendanceViewModel> GetTodaysAttendance(int userID, string timezone)
        {
            return await Task.Run(() =>
            {
                var viewModel = new AttendanceViewModel();
                DateTime Today = DateTime.Now.ToThisTimeZone(timezone);
                var empSISO = service.Top<SignInSignOut>(0, x => (x.UserID == userID) && (x.AttendanceDate.Value >= Today.Date));
                var employeeSISOList = Mapper.Map<IEnumerable<SignInSignOut>, IEnumerable<EmployeeSISOViewModel>>(empSISO);

                var empTodaySignIn = empSISO.Where(a => a.SignInTime.Value.Date == DateTime.Now.Date).ToList();
                var empTodaySignOut = empSISO.Where(a => a.SignOutTime != null ? a.SignOutTime.Value.Date == DateTime.Now.Date : DateTime.MinValue == DateTime.Now.Date).ToList();
                var isOnLeave = service.Top<PersonLeave>(0, x => (x.Person.ID == userID) && (x.Status == 1 || x.Status == 2) && (x.FromDate <= DateTime.Now && x.ToDate >= DateTime.Now)).ToList();
                viewModel.EmployeeSISOViewModels = employeeSISOList;
                viewModel.TodayStatus = new TodayStatus();
                viewModel.TodayStatus.IsSignIn = empTodaySignIn.Count() > 0 ? true : false;
                viewModel.TodayStatus.IsSignOut = empTodaySignOut.Count() > 0 ? true : false;
                viewModel.TodayStatus.IsOnleaveToday = isOnLeave.Count() > 0 ? true : false;
                if (!viewModel.TodayStatus.IsSignIn)
                {
                    /// add empty entry for today
                    (viewModel.EmployeeSISOViewModels as List<EmployeeSISOViewModel>).Insert(0, new EmployeeSISOViewModel() { Date = DateTime.Now });
                }
                return viewModel;
            });
        }

        public async Task<bool> AddBulkEntries(int userID, SISOManualAutoViewModel model)
        {
            try
            {
                using (var _phoenixEntity = new PhoenixEntities())
                    _phoenixEntity.Bulk_SignInSignOut(userID, model.SignInTime, model.SignOutTime);

                return await Task.FromResult(true).ConfigureAwait(false);
            }
            catch (Exception)
            {
            }
            return await Task.FromResult(false).ConfigureAwait(false);
        }

        public async Task<int> Add(int userID, SISOManualAutoViewModel model)
        {
            //predicate parameter was not accepting Date part, was raising an exception. So I have added a condition whcih will never satisfy so that we can insert into table on temporary basis
            var obj = Mapper.Map<SISOManualAutoViewModel, SignInSignOut>(model);
            var employee = service.First<Person>(x => x.ID == userID);
            var ol = employee.PersonEmployment.First().OfficeLocation;

            if (model.IsSignIn && !model.IsManual)
            {
                if (ol == 2)
                {
                    if (model.TimeZoneName != null)
                        obj.SignInTime = DateTime.Now.ToThisTimeZone(model.TimeZoneName);
                    else
                    {
                        string employeeTimeZone = employee.PersonEmployment.First().TimeZone;
                        obj.SignInTime = DateTime.Now.ToThisTimeZone(employeeTimeZone);
                    }
                }
            }
            else if (!model.IsSignIn && !model.IsManual)
            {
                if (ol == 2)
                {
                    if (model.TimeZoneName != null)
                        obj.SignOutTime = DateTime.Now.ToThisTimeZone(model.TimeZoneName);
                    else
                    {
                        string employeeTimeZone = employee.PersonEmployment.First().TimeZone;
                        obj.SignOutTime = DateTime.Now.ToThisTimeZone(employeeTimeZone);
                    }
                }

            }

            bool returnVal = false;
            string currDate = DateTime.Now.Date.ToString();
            if (model.IsSignIn)
            {
                if (obj.SignOutTime == DateTime.MinValue)
                    obj.SignOutTime = null;
                obj.UserID = userID;
                obj.DayNotation = "P";
                obj.statusID = 1;
                obj.ApproverID = service.First<PersonReporting>(x => x.PersonID == userID && x.Active == true && x.IsDeleted == false).ReportingTo;
                obj.AttendanceDate = obj.SignInTime.Value.Date;
                IEnumerable<SignInSignOut> userList = service.Top<SignInSignOut>(0, a => a.UserID == userID);
                var isSignInEntry = userList.Where(a => a.SignInTime != null ? a.SignInTime.Value.Date == obj.SignInTime.Value.Date : DateTime.MinValue == obj.SignInTime.Value.Date).FirstOrDefault();
                if (isSignInEntry == null)
                {
                    returnVal = service.Create(obj, m => m.UserID == -1);
                }
                else
                {
                    obj.SignOutComment = isSignInEntry.SignOutComment;
                    if (isSignInEntry.SignOutTime == null)
                    {
                        returnVal = service.Update(obj, isSignInEntry);
                    }
                    else
                    {
                        if (isSignInEntry.SignOutTime > obj.SignInTime)
                        {
                            TimeSpan totalTime = isSignInEntry.SignOutTime.Value.TimeOfDay.Subtract(new TimeSpan(obj.SignInTime.Value.TimeOfDay.Hours, obj.SignInTime.Value.TimeOfDay.Minutes, 0));
                            obj.TotalHoursWorked = string.Format("{00:00}:{01:00}", totalTime.Hours, totalTime.Minutes);
                            returnVal = service.Update(obj, isSignInEntry);
                        }
                        else
                        {
                            return await Task.FromResult(2).ConfigureAwait(false);
                        }
                    }
                }
                service.Finalize(returnVal);
            }
            else
            {
                IEnumerable<SignInSignOut> userList = service.Top<SignInSignOut>(0, a => a.UserID == userID);
                var oldObj = userList.Where(a => a.SignInTime.HasValue && a.SignInTime.Value.Date == obj.SignOutTime.Value.Date).FirstOrDefault();
                if (oldObj != null)
                {
                    obj.SignInTime = oldObj.SignInTime;
                    obj.SignInSignOutID = oldObj.SignInSignOutID;
                    obj.UserID = userID;
                    obj.SignInComment = oldObj.SignInComment;
                    TimeSpan totalTime = obj.SignOutTime.Value.TimeOfDay.Subtract(new TimeSpan(oldObj.SignInTime.Value.TimeOfDay.Hours, oldObj.SignInTime.Value.TimeOfDay.Minutes, 0));
                    obj.TotalHoursWorked = string.Format("{00:00}:{01:00}", totalTime.Hours, totalTime.Minutes);
                    obj.AttendanceDate = oldObj.SignInTime.Value.Date;
                    if (oldObj.SignInTime < obj.SignOutTime)
                    {
                        returnVal = service.Update(obj, oldObj);
                    }
                    else
                    {
                        return await Task.FromResult(3).ConfigureAwait(false);
                    }
                }
                else
                {
                    return await Task.FromResult(0).ConfigureAwait(false);
                }
                service.Finalize(returnVal);

                //string empName = employee.FirstName + " " + employee.LastName;
                //emailService.SendAttendanceApproval(userID, empName, employee.PersonEmployment.First().OrganizationEmail, empName, employee.PersonEmployment.First().OrganizationEmail, obj.SignInTime.Value.Date.ToString("MM/dd/yyyy"), employee.Image);
            }
            return await Task.FromResult(1).ConfigureAwait(false);
        }

        public async Task<IEnumerable<EmployeeSISOViewModel>> GetPendingListDateWise(int userID)
        {
            try
            {
                var _tempList = new List<EmployeeSISOViewModel>();
                using (var _phoenixEntity = new PhoenixEntities())
                {
                    var personSisoViewModel = _phoenixEntity.GetSISOPendingApprovalDateWise(userID.ToString(), null, null).ToList();
                    foreach (var item in personSisoViewModel)
                    {
                        var tmpSignOutTime = TimeSpan.Zero;
                        if (item.SignOutTime != null)
                            tmpSignOutTime = item.SignOutTime.Value.TimeOfDay;

                        _tempList.Add(new EmployeeSISOViewModel
                        {
                            Id = item.SignInSignOutID,
                            Date = item.SignInTime.Value.Date,
                            SignInTime = item.SignInTime.Value.TimeOfDay,
                            SignOutTime = tmpSignOutTime,
                            TotalHoursWorked = item.TotalHoursWorked,
                            EmployeeProfile = new EmployeeBasicProfile
                            {
                                ID = (int)item.UserID,
                                ImagePath = item.Image,
                                FirstName = item.FirstName,
                                MiddleName = item.MiddleName,
                                LastName = item.LastName,
                                Extension = item.OfficeExtension,
                                Mobile = item.Mobile,
                                Email = item.OrganizationEmail,
                                SeatingLocation = item.SeatingLocation,
                                OLText = item.LocationName
                            }
                        });
                    }
                }
                return await Task.FromResult(_tempList).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new HttpException(500, ex.ToString());
            }
        }

        public async Task<IEnumerable<EmployeeSISOViewModel>> GetPendingListUserWise(int userID)
        {
            try
            {
                var _tempList = new List<EmployeeSISOViewModel>();
                using (var _phoenixEntity = new PhoenixEntities())
                {
                    var personSisoViewModel = _phoenixEntity.GetSISOPendingApprovalUserWise(userID.ToString(), null, null).ToList();
                    foreach (var item in personSisoViewModel)
                    {
                        _tempList.Add(new EmployeeSISOViewModel
                        {
                            SignInSignOutDate = item.SignInSignOutDate,
                            EmployeeProfile = new EmployeeBasicProfile
                            {
                                ID = (int)item.UserID,
                                ImagePath = item.Image,
                                FirstName = item.FirstName,
                                MiddleName = item.MiddleName,
                                LastName = item.LastName,
                                Extension = item.OfficeExtension,
                                Mobile = item.Mobile,
                                Email = item.OrganizationEmail,
                                SeatingLocation = item.SeatingLocation,
                                OLText = item.LocationName
                            }
                        });
                    }
                }
                return await Task.FromResult(_tempList).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new HttpException(500, ex.ToString());
            }
        }

        public async Task<PersonARStatus> ApproveSISO(int userId, SISOApprovalViewModel model)
        {
            try
            {
                using (var _phoenixEntity = new PhoenixEntities())
                {
                    if (model.IsBulkApprovals)
                        _phoenixEntity.ApproveePSISO_Person(userId, model.IDs);
                    else
                        _phoenixEntity.ApprovePSISO(userId, model.IDs);
                }
                var personARStatus = new PersonARStatus();
                personARStatus.status = 1;
                personARStatus.statusComment = "Data Approved Successfully!";
                return await Task.FromResult(personARStatus).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new HttpException(500, ex.ToString());
            }
        }

        public async Task<PersonARStatus> RejectSISO(int userId, SISORejectViewModel model)
        {
            try
            {
                using (var _phoenixEntity = new PhoenixEntities())
                {
                    if (model.IsBulkApprovals)
                        _phoenixEntity.RejectPSISO_Person(userId, model.IDs, model.RejectComment);
                    else
                        _phoenixEntity.RejectPSISO(userId, model.IDs, model.RejectComment);
                }
                var personARStatus = new PersonARStatus
                {
                    status = 2,
                    statusComment = "Data Rejected Successfully!"
                };
                return await Task.FromResult(personARStatus).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new HttpException(500, ex.ToString());
            }
        }

        public async Task<IEnumerable<SISOAttendanceRptDTOModel>> GetSISOAttendanceRpt(SISOAttendanceRptModel model)
        {
            try
            {
                var resultData = new List<SISOAttendanceRptDTOModel>();
                using (var _phoenixEntity = new PhoenixEntities())
                {
                    var _tempList = _phoenixEntity.GetSISOAttendanceRpt(model.StartDate, model.EndDate, model.FilterType,
                                                                        model.EmployeeId, model.ManagerId, model.DeliveryUnitId,
                                                                        model.DeliveryTeamId, model.WorkLocationId, model.OfficeLocationId).ToList();

                    foreach (var item in _tempList)
                    {
                        var tmpSignOutTime = TimeSpan.Zero;
                        if (item.EndDate != null)
                            tmpSignOutTime = item.EndDate.Value.TimeOfDay;

                        resultData.Add(new SISOAttendanceRptDTOModel
                        {
                            EmployeeCode = item.EmployeeCode,
                            EmployeeName = item.EmployeeName,
                            StartDate = item.StartDate,
                            EndDate = item.EndDate,
                            SignInTime = item.StartDate.Value.TimeOfDay,
                            SignOutTime = tmpSignOutTime,
                            TotalHoursWorked = item.TotalHoursWorked,
                            Status = item.Status,
                            ManagerCode = item.ManagerId,
                            ManagerName = item.ManagerName,
                            DeliveryTeamName = item.DeliveryTeamName,
                            DeliveryUnitName = item.DeliveryUnitName,
                            OrgUnit = item.OrgUnit,
                            WorkLocation = item.WorkLocation,
                            OfficeLocation = item.OfficeLocation
                        });
                    }
                }

                return await Task.FromResult(resultData).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new HttpException(500, ex.ToString());
            }
        }

        public HttpResponseMessage DownloadSISOAttendanceRpt(SISOAttendanceRptModel model)
        {
            var sw = new StringBuilder();
            sw.Append("\"Employee Code\",");
            sw.Append("\"Employee Name\",");
            sw.Append("\"Sign-In Date\",");
            sw.Append("\"Sign-In Time\",");
            sw.Append("\"Sign-Out Date\",");
            sw.Append("\"Sign-Out Time\",");
            sw.Append("\"TotalHoursWorked\",");
            sw.Append("\"Status\",");
            sw.Append("\"Manager Code\",");
            sw.Append("\"Manager Name\",");
            sw.Append("\"Delivery Team Name\",");
            sw.Append("\"Delivery Unit Name\",");
            sw.Append("\"Org. Unit\",");
            sw.Append("\"Work Location\",");
            sw.Append("\r\n");

            using (var _phoenixEntity = new PhoenixEntities())
            {
                var resultData = _phoenixEntity.GetSISOAttendanceRpt(model.StartDate, model.EndDate, model.FilterType,
                                                                    model.EmployeeId, model.ManagerId, model.DeliveryUnitId,
                                                                    model.DeliveryTeamId, model.WorkLocationId, model.OfficeLocationId).ToList();

                foreach (var item in resultData)
                {
                    var startDate = "";
                    if (!string.IsNullOrEmpty(item.StartDate.Value.ToString()))
                        startDate = item.StartDate.Value.ToString("MM-dd-yyyy");

                    var endDate = "";
                    if (item.EndDate != null)
                        if (!string.IsNullOrEmpty(item.EndDate.Value.ToString()))
                            endDate = item.EndDate.Value.ToString("MM-dd-yyyy");

                    sw.AppendFormat("\"{0}\",", item.EmployeeCode);
                    sw.AppendFormat("\"{0}\",", item.EmployeeName);
                    sw.AppendFormat("\"{0}\",", startDate);
                    if (startDate != null && startDate != "")
                        sw.AppendFormat("\"{0}\",", item.StartDate.Value.ToString("hh:mm:ss"));
                    else
                        sw.AppendFormat("\"{0}\",", "-");

                    sw.AppendFormat("\"{0}\",", endDate);

                    if (endDate != null && endDate != "")
                        sw.AppendFormat("\"{0}\",", item.EndDate.Value.ToString("hh:mm:ss"));
                    else
                        sw.AppendFormat("\"{0}\",", "-");

                    sw.AppendFormat("\"{0}\",", item.TotalHoursWorked);
                    sw.AppendFormat("\"{0}\",", item.Status);
                    sw.AppendFormat("\"{0}\",", item.ManagerId);
                    sw.AppendFormat("\"{0}\",", item.ManagerName);
                    sw.AppendFormat("\"{0}\",", item.DeliveryTeamName);
                    sw.AppendFormat("\"{0}\",", item.DeliveryUnitName);
                    sw.AppendFormat("\"{0}\",", item.OrgUnit);
                    sw.AppendFormat("\"{0}\",", item.WorkLocation);
                    sw.AppendFormat("\r\n");
                }
            }


            HttpResponseMessage Response = new HttpResponseMessage(HttpStatusCode.OK);
            Response.Content = new StringContent(sw.ToString());
            Response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            Response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment"); //attachment will force download
            Response.Content.Headers.ContentDisposition.FileName = "SISOReport.csv";
            return Response;
        }

        public HttpResponseMessage DownloadReport(int userID)
        {
            var sw = new StringBuilder();
            sw.Append("\"Date\",");
            sw.Append("\"Employee Name\",");
            sw.Append("\"Sign-In Date & Time\",");
            sw.Append("\"Sign-Out Date & Time\",");
            sw.Append("\r\n");

            var _phoenixEntity = new PhoenixEntities();
            var resultData = Task.Run(() => _phoenixEntity.GetSISOPendingApprovalDateWise(userID.ToString(), null, null));
            foreach (var item in resultData.Result)
            {
                sw.AppendFormat("\"{0}\",", item.AttendanceDate);
                sw.AppendFormat("\"{0}\",", $"{item.FirstName} {item.MiddleName} {item.LastName}");
                sw.AppendFormat("\"{0}\",", item.SignInTime);
                sw.AppendFormat("\"{0}\",", item.SignOutTime);
                sw.AppendFormat("\r\n");
            }

            HttpResponseMessage Response = new HttpResponseMessage(HttpStatusCode.OK);
            Response.Content = new StringContent(sw.ToString());
            Response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            Response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment"); //attachment will force download
            Response.Content.Headers.ContentDisposition.FileName = "SISOReport.csv";
            return Response;
        }

        #endregion Public Contracts

        #region Helpers

        private int GetBusinessDays(DateTime startDate, DateTime endDate, IEnumerable<HolidayList> holidayList)
        {
            double calcBusinessDays = 1 + ((endDate - startDate).TotalDays * 5 - (startDate.DayOfWeek - endDate.DayOfWeek) * 2) / 7;

            if (endDate.DayOfWeek == DayOfWeek.Saturday) calcBusinessDays--;
            if (startDate.DayOfWeek == DayOfWeek.Sunday) calcBusinessDays--;

            foreach (var item in holidayList)
            {
                if (startDate.Day <= item.Date.Value.Day && endDate.Day >= item.Date.Value.Day)
                    calcBusinessDays--;
            }

            return (int)calcBusinessDays;
        }

        #endregion Helpers
    }
}