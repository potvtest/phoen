using Pheonix.DBContext;
using Pheonix.DBContext.Repository;
using Pheonix.Models.Models.Admin;
using Pheonix.Models.VM.Classes.Employee;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pheonix.Core.v1.Services.Admin
{
    public class AdminSignInSignOutTask : IAdminTask
    {
        //SISO

        public AdminActionResult TakeActionOn(IContextRepository repo, Models.Models.Admin.AdminActionModel model)
        {
            AdminActionResult result = new AdminActionResult();
            var dayStart = new DateTime(model.From.Year, model.From.Month, model.From.Day, 0, 0, 0);
            var time = dayStart.AddHours(24).AddMinutes(-1);
            
            string message = string.Empty;
            result.isActionPerformed = true;
            //Check SISO entry for that date
            var signInSignOutEntry = repo.FirstOrDefault<SignInSignOut>(t => (t.SignInTime > dayStart && t.SignInTime <= time) && t.UserID == model.EmployeeID);

            if (model.Validated == 0)
            {
                // Check if leave is present on that enetered date.
                // Status 2 means Leave is approved             
                var leave = repo.FirstOrDefault<PersonLeave>(t => dayStart >= t.FromDate && dayStart <= t.ToDate && t.Person.ID == model.EmployeeID && t.Status == 2 && t.IsDeleted == false);

                if (leave == null)
                {
                    if (CheckHolidayOrWeekend(repo, model.EmployeeID, model.LocationID, model.From, out message))
                    {
                        if (signInSignOutEntry != null && signInSignOutEntry.DayNotation == "P")
                        {
                            result.isActionPerformed = false;
                            result.message = "SISO: Employee is present for selected date. Do you want to continue? ";
                        }
                        else if (signInSignOutEntry != null && signInSignOutEntry.DayNotation == "A")
                        {
                            result.isActionPerformed = false;
                            result.message = "SISO: Employee is absent for selected date. Do you want to continue? ";
                        }
                    }
                    else
                    {
                        result.isActionPerformed = false;
                        result.message = message;
                    }
                }
                else
                {
                    result.isActionPerformed = false;
                    result.message = string.Format("SISO: {0}-{1} has approved leave for the given period please check if you have selected right date. \n\r Otherwise ask {0} to cancel leave and then perform this action.", leave.Person.FirstName, leave.Person.ID);
                }
            }
            else
            {
                if (signInSignOutEntry != null)
                {
                    UpdateLeaveDetail(repo, dayStart, signInSignOutEntry, model);
                    result.isActionPerformed = true;
                    result.message = string.Format("SISO Updated Successfully");
                }
                else
                {
                    AddLeaveDetail(repo, dayStart, signInSignOutEntry, model);
                    result.isActionPerformed = true;
                    result.message = string.Format("SISO Added Successfully");
                }
            }


            return result;
        }

        private void AddLeaveDetail(IContextRepository repo, DateTime dayStart, SignInSignOut signInSignOutEntry, AdminActionModel model)
        {
            signInSignOutEntry = new SignInSignOut();
            signInSignOutEntry.SignInComment = model.Comments;
            signInSignOutEntry.SignInTime = dayStart.AddHours(9).AddMinutes(30);
            signInSignOutEntry.SignOutTime = dayStart.AddHours(18).AddMinutes(30);
            signInSignOutEntry.TotalHoursWorked = "09:00";
            signInSignOutEntry.ApproverID = model.AdminID;
            signInSignOutEntry.DayNotation = "P";
            signInSignOutEntry.AttendanceDate = dayStart;
            signInSignOutEntry.FirstModified = DateTime.Now;
            signInSignOutEntry.LastModified = DateTime.Now;
            signInSignOutEntry.UserID = model.EmployeeID;
            signInSignOutEntry.IsBulk = true;
            signInSignOutEntry.statusID = 2;

            repo.Create(signInSignOutEntry, null);
            repo.Save();
        }

        private void UpdateLeaveDetail(IContextRepository repo, DateTime dayStart, SignInSignOut signInSignOutEntry, AdminActionModel model)
        {
            signInSignOutEntry.SignInComment = model.Comments;
            signInSignOutEntry.SignInTime = dayStart.AddHours(9).AddMinutes(30);
            signInSignOutEntry.SignOutTime = dayStart.AddHours(18).AddMinutes(30);
            signInSignOutEntry.TotalHoursWorked = "09:00";
            signInSignOutEntry.ApproverID = model.AdminID;
            signInSignOutEntry.DayNotation = "P";
            signInSignOutEntry.AttendanceDate = dayStart;
            signInSignOutEntry.LastModified = DateTime.Now;
            signInSignOutEntry.UserID = model.EmployeeID;
            signInSignOutEntry.IsBulk = true;
            signInSignOutEntry.statusID = 2;

            repo.Update(signInSignOutEntry);
            repo.Save();
        }

        public List<DateTime> CreateDateRange(DateTime from, DateTime to, bool skipWeekends, List<DateTime> except)
        {
            List<DateTime> range = new List<DateTime>();
            int days = to.Subtract(from).Days;
            while (days > 0)
            {
                range.Add(from.AddDays(days--));
            }

            if (skipWeekends)
                range = range.Where(t => t.DayOfWeek != DayOfWeek.Sunday && t.DayOfWeek != DayOfWeek.Saturday).ToList();

            return range;
        }

        public bool CheckHolidayOrWeekend(IContextRepository repo, int userId, int location, DateTime fromDate, out string message)
        {
            var holidayList = repo.FindBy<HolidayList>(x => x.Location == location && (x.Date >= fromDate) && x.IsDeleted == false && x.HolidayYear == fromDate.Year);

            bool result = true;

            string errorMessage = string.Empty;

            if ((fromDate.DayOfWeek != DayOfWeek.Saturday) && (fromDate.DayOfWeek != DayOfWeek.Sunday))
            {
                foreach (var holiday in holidayList)
                {
                    if (fromDate.Date == holiday.Date)
                    {
                        errorMessage = "SISO: This is " + holiday.Description + " holiday . Do you want to continue?";
                        result = false;
                    }
                }
            }
            else
            {
                errorMessage = "SISO: Selected Day is a " + fromDate.DayOfWeek + " . Do you want to continue?";
                result = false;
            }

            message = errorMessage;
            return result;
        }


        //Hard delete the record from Sign In Sign Out table
        public AdminActionResult Delete(IContextRepository repo, AdminActionModel model)
        {
            AdminActionResult result = new AdminActionResult();

            var siso = repo.FirstOrDefault<SignInSignOut>(x => x.SignInSignOutID == model.ID);
            repo.HardRemove<SignInSignOut>(siso, x => x.SignInSignOutID == model.ID);
            repo.Save();

            result.isActionPerformed = true;
            result.message = string.Format("SISO Deleted Successfully");
            return result;
        }
    }
}
