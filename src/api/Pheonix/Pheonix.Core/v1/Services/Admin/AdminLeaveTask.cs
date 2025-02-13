using AutoMapper;
using Pheonix.DBContext;
using Pheonix.DBContext.Repository;
using Pheonix.Models.Models.Admin;
using Pheonix.Models.VM;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace Pheonix.Core.v1.Services.Admin
{
    public class AdminLeaveTask : IAdminTask
    {
        //Admin Leave Application

        Models.Models.Admin.AdminActionResult IAdminTask.TakeActionOn(IContextRepository repo, Models.Models.Admin.AdminActionModel model)
        {
            AdminActionResult result = new AdminActionResult();

            var ledger = repo.FirstOrDefault<PersonLeaveLedger>(t => t.PersonID == model.EmployeeID && t.Year == DateTime.Now.Year);
            int days = CalculateLeavesApplied(repo, model.EmployeeID, model.LocationID, model.From, model.To);

            if (model.Validated == 1)
            {
                if (!CheckLeaveConflicts(repo, model.From, model.To, model.EmployeeID))
                {
                    //model.Subtype = 1 means 'Privilege leave'
                    //model.Subtype = 2 means 'comp-off'
                    //model.Subtype = 3 means 'LWP'
                    //model.Subtype = 4 means 'Long Leave'
                    //model.Subtype = 5 means 'Maternity Leave'
                    //model.Subtype = 6 means 'Paternity Leave'
                    //model.Subtype = 8 means 'SFH Leave'
                    //model.Subtype = 9 means 'Casual Leave'
                    //model.Subtype = 10 means 'MTP Leave'
                    //model.Subtype = 11 means 'Sick Leave'              
                    //model.Subtype = 12 means 'Election Holiday Leave'
                    if (days != 0)
                    {
                        if (Convert.ToInt16(model.SubType) == 1)
                        {
                            result = Saveleaveapplication(model, days, ledger, repo);
                        }
                        else if (Convert.ToInt16(model.SubType) == 2)
                        {
                            result = SaveCompoffapplication(model, days, ledger, repo);
                        }
                        else if (Convert.ToInt16(model.SubType) == 3)
                        {
                            result = SaveLWPleaveapplication(model, days, repo);
                        }
                        else if (Convert.ToInt16(model.SubType) == 4)
                        {
                            result = SaveLLleaveapplication(model, days, repo);
                        }
                        else if (Convert.ToInt16(model.SubType) == 5)
                        {
                            result = SaveMLleaveapplication(model, days, repo);
                        }
                        else if (Convert.ToInt16(model.SubType) == 6)
                        {
                            result = SavePLleaveapplication(model, days, repo);
                        }
                        else if (Convert.ToInt16(model.SubType) == 8)
                        {
                            result = SaveSHFleaveapplication(model, days, repo);
                        }
                        else if (Convert.ToInt16(model.SubType) == 9)
                        {
                            result = SaveCLeaveapplication(model, days, ledger, repo);
                        }
                        else if (Convert.ToInt16(model.SubType) == 10)
                        {
                            result = SaveMTPLeaveapplication(model, days, repo);
                        }
                        else if (Convert.ToInt16(model.SubType) == 11)
                        {
                            result = SaveSLeaveapplication(model, days, ledger, repo);
                        }
                        else if (Convert.ToInt16(model.SubType) == 12)
                        {
                            result = SaveEHleaveapplication(model, days, repo);
                        }

                        if (result.isActionPerformed == true)
                        {
                            using (PhoenixEntities context = new PhoenixEntities())
                            {
                                var data = (from x in context.SignInSignOut
                                            where (x.UserID == model.EmployeeID && DbFunctions.TruncateTime(x.AttendanceDate) >= model.From && DbFunctions.TruncateTime(x.AttendanceDate) <= model.To && x.DayNotation == "A")
                                            select x).ToList();
                                foreach (var item in data)
                                {
                                    context.Entry(item).State = EntityState.Deleted;
                                }
                                context.SaveChanges();
                            }
                        }

                    }
                    else
                    {
                        result.isActionPerformed = false;
                        //result.message = "User is present for selected Date Range"; //For #147994489
                        result.message = "Leave cannot be applied for Weekends and Holidays OR User is present for selected Date Range.";
                    }
                }
                else
                {
                    result.isActionPerformed = false;
                    result.message = "An overlapping leave record already exists";
                }
            }
            else
            {
                // This result is used as a variable to send the no of days so that it can be used in the admin leave application section
                result.isActionPerformed = true;
                result.message = string.Format((days).ToString());
            }
            return result;
        }

        private AdminActionResult SaveCompoffapplication(AdminActionModel model, int days, PersonLeaveLedger ledger, IContextRepository repo)
        {
            AdminActionResult result = new AdminActionResult();
            using (PhoenixEntities context = new PhoenixEntities())
            {
                var data = (from x in context.CompOff
                            where (x.PersonID == model.EmployeeID && x.Year == DateTime.Now.Year && x.ExpiresOn >= model.From && x.IsApplied != true && x.Status == 1)
                            select x).ToList();

                List<int> empLeavesTaken = new List<int>();
                DateTime currDate = DateTime.Now;
                for (DateTime index = model.From; index <= model.To; index = index.AddDays(1))
                {
                    foreach (var _compOffList in data)
                    {
                        if (index.Date <= _compOffList.ExpiresOn.Value.Date)
                        {
                            if (!empLeavesTaken.Contains(_compOffList.ID) && currDate != index.Date)
                            {
                                empLeavesTaken.Add(_compOffList.ID);
                                currDate = index.Date;
                            }
                        }
                    }
                }

                if (days > empLeavesTaken.Count)
                {
                    result.isActionPerformed = false;
                    result.message = string.Format("Applied leave is more than available comp-off balance OR Your comp-off is expired");
                    return result;
                }
            }
            using (PhoenixEntities context = new PhoenixEntities())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        //Add person Leave
                        EmployeeLeaveViewModel emp = new EmployeeLeaveViewModel();
                        emp.UserId = model.EmployeeID;
                        emp.FromDate = model.From;
                        emp.ToDate = model.To;
                        emp.Leaves = days;
                        emp.Narration = "Admin Leave Application :" + model.Comments;
                        emp.LeaveType = 0;
                        emp.Status = 2;


                        var leave = Mapper.Map<EmployeeLeaveViewModel, PersonLeave>(emp);

                        leave.Person = repo.FirstOrDefault<Person>(x => x.ID == model.EmployeeID);
                        leave.Person.ID = model.EmployeeID;
                        leave.RequestDate = DateTime.Now;
                        leave.NotConsumed = 0;
                        repo.Create(leave, null);
                        repo.Save();

                        //This check is implemented to consumed Comp-Off order wise and Expiry date wise FOR: #148824553
                        var holidayList = repo.GetAll<HolidayList>().Where(x => x.Location == model.LocationID && (x.Date >= model.From.Date && x.Date <= model.To.Date) && x.IsDeleted == false && x.HolidayYear == model.To.Date.Year).ToList();
                        var compOffList = repo.GetAll<CompOff>().Where(x => x.Person.ID == model.EmployeeID && x.IsApplied == false && x.Status == 1 && x.ExpiresOn >= model.From).OrderBy(d => d.ExpiresOn).ToList();
                        DateTime currDate = DateTime.Now;
                        for (DateTime index = leave.FromDate.Date; index <= leave.ToDate.Date; index = index.AddDays(1))
                        {
                            if (index.DayOfWeek != DayOfWeek.Saturday && index.DayOfWeek != DayOfWeek.Sunday)
                            {
                                bool excluded = false;
                                foreach (var holiday in holidayList)
                                {
                                    if (index.Date.CompareTo(holiday.Date) == 0)
                                    {
                                        excluded = true;
                                        break;
                                    }
                                }

                                foreach (var _compOffList in compOffList)
                                {
                                    if (index.Date <= _compOffList.ExpiresOn.Value.Date && currDate != index.Date && !excluded && _compOffList.IsApplied == false)
                                    {
                                        var firstCompOff = repo.FirstOrDefault<CompOff>(t => t.PersonID == model.EmployeeID && t.Year == DateTime.Now.Year && t.ExpiresOn >= model.From && t.IsApplied != true && t.Status == 1 && t.ID == _compOffList.ID);

                                        firstCompOff.IsApplied = true;
                                        firstCompOff.Status = 2;
                                        firstCompOff.LeaveRequestID = leave.ID;
                                        repo.Update(firstCompOff);
                                        repo.Save();

                                        currDate = index.Date;
                                    }
                                }
                            }
                        }

                        if (ledger != null)
                        {
                            //Update person leave ledger
                            //ledger.CompOffUtilized += days;
                            ledger.CompOffUtilized = (ledger?.CompOffUtilized ?? 0) + days;
                            repo.Update(ledger);
                            repo.Save();
                        }
                        transaction.Commit();

                        result.isActionPerformed = true;
                        result.message = string.Format("Comp-Off applied Successfully");
                    }
                    catch
                    {
                        transaction.Rollback();
                        result.isActionPerformed = false;
                    }
                }
            }
            return result;
        }

        private AdminActionResult Saveleaveapplication(AdminActionModel model, int days, PersonLeaveLedger ledger, IContextRepository repo)
        {
            AdminActionResult result = new AdminActionResult();
            using (PhoenixEntities context = new PhoenixEntities())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        //Add Person Leave
                        EmployeeLeaveViewModel emp = new EmployeeLeaveViewModel();
                        emp.UserId = model.EmployeeID;
                        emp.FromDate = model.From;
                        emp.ToDate = model.To;
                        emp.Leaves = days;
                        emp.Narration = "Admin Leave Application :" + model.Comments;
                        emp.LeaveType = 1;
                        emp.Status = 2;



                        var leave = Mapper.Map<EmployeeLeaveViewModel, PersonLeave>(emp);

                        leave.Person = repo.FirstOrDefault<Person>(x => x.ID == model.EmployeeID);
                        leave.Person.ID = model.EmployeeID;
                        leave.RequestDate = DateTime.Now;
                        repo.Create(leave, null);
                        repo.Save();

                        if (ledger != null)
                        {
                            //Update Person Leave ledger
                            //ledger.LeaveUtilized += days;
                            ledger.LeaveUtilized = (ledger?.LeaveUtilized ?? 0) + days;
                            repo.Update(ledger);
                            repo.Save();
                        }
                        transaction.Commit();

                        result.isActionPerformed = true;
                        result.message = string.Format("Privilege Leave applied Successfully");
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        result.isActionPerformed = false;
                        result.message = ex.Message.ToString();
                    }
                }
            }
            return result;
        }

        private AdminActionResult SavePLleaveapplication(AdminActionModel model, int days, IContextRepository repo)
        {
            AdminActionResult result = new AdminActionResult();
            try
            {
                var PLcount = repo.FirstOrDefault<PhoenixConfig>(x => x.Year == model.From.Year && x.Location == model.LocationID && x.ConfigKey == "PL"); // For: #149290431 - Change done to apply PL leave by calculating working days. 

                //int PLcount = Convert.ToInt32((model.To - model.From).TotalDays) + 1;

                EmployeeLeaveViewModel emp = new EmployeeLeaveViewModel();
                emp.UserId = model.EmployeeID;
                emp.FromDate = model.From;
                emp.ToDate = model.To;
                emp.Leaves = days;//int.Parse(PLcount.ConfigValue); // For: #149290431 - Change done to apply PL leave by calculating working days. 
                emp.Narration = "Admin Leave Application :" + model.Comments;
                emp.LeaveType = 4;
                emp.Status = 2;


                var leave = Mapper.Map<EmployeeLeaveViewModel, PersonLeave>(emp);
                leave.Person = repo.FirstOrDefault<Person>(x => x.ID == model.EmployeeID);
                leave.Person.ID = model.EmployeeID;
                leave.RequestDate = DateTime.Now;
                repo.Create(leave, null);
                repo.Save();

                result.isActionPerformed = true;
                result.message = "Paternity Leave applied Successfully";
            }
            catch (Exception ex)
            {
                result.isActionPerformed = false;
                result.message = ex.Message.ToString();
            }

            return result;
        }
        private AdminActionResult SaveMTPLeaveapplication(AdminActionModel model, int days, IContextRepository repo)
        {
            AdminActionResult result = new AdminActionResult();
            try
            {
                int MTPcount = Convert.ToInt32((model.To - model.From).TotalDays) + 1;

                //int PLcount = Convert.ToInt32((model.To - model.From).TotalDays) + 1;

                EmployeeLeaveViewModel emp = new EmployeeLeaveViewModel();
                emp.UserId = model.EmployeeID;
                emp.FromDate = model.From;
                emp.ToDate = model.To;
                emp.Leaves = MTPcount;
                emp.Narration = "Admin Leave Application :" + model.Comments;
                emp.LeaveType = 10;
                emp.Status = 2;


                var leave = Mapper.Map<EmployeeLeaveViewModel, PersonLeave>(emp);
                leave.Person = repo.FirstOrDefault<Person>(x => x.ID == model.EmployeeID);
                leave.Person.ID = model.EmployeeID;
                leave.RequestDate = DateTime.Now;
                repo.Create(leave, null);
                repo.Save();

                result.isActionPerformed = true;
                result.message = "MTP Leave applied Successfully";
            }
            catch (Exception ex)
            {
                result.isActionPerformed = false;
                result.message = ex.Message.ToString();
            }

            return result;
        }

        private AdminActionResult SaveMLleaveapplication(AdminActionModel model, int days, IContextRepository repo)
        {
            AdminActionResult result = new AdminActionResult();
            try
            {

                //var configData = repo.FirstOrDefault<PhoenixConfig>(x => x.Year == DateTime.Now.Year && x.Location == model.LocationID && x.ConfigKey == "ML");

                int MLcount = Convert.ToInt32((model.To - model.From).TotalDays) + 1;
                using (PhoenixEntities context = new PhoenixEntities())
                {
                    var ChkMLCount = (from x in context.PersonLeave
                                      where (x.Person.ID == model.EmployeeID && (x.Status != 4 && x.Status != 3) && x.IsDeleted == false && x.LeaveType == 3)
                                      select x).ToList();

                    if (ChkMLCount != null && ChkMLCount.Count() < 2)
                    {
                        EmployeeLeaveViewModel emp = new EmployeeLeaveViewModel();
                        emp.UserId = model.EmployeeID;
                        emp.FromDate = model.From;
                        emp.ToDate = model.To;
                        emp.Leaves = MLcount;
                        emp.Narration = "Admin Leave Application :" + model.Comments;
                        emp.LeaveType = 3;
                        emp.Status = 2;


                        var leave = Mapper.Map<EmployeeLeaveViewModel, PersonLeave>(emp);
                        leave.Person = repo.FirstOrDefault<Person>(x => x.ID == model.EmployeeID);
                        leave.Person.ID = model.EmployeeID;
                        leave.RequestDate = DateTime.Now;
                        repo.Create(leave, null);
                        repo.Save();

                        result.isActionPerformed = true;
                        result.message = "Maternity Leave applied Successfully";
                    }
                    else
                    {
                        result.isActionPerformed = false;
                        result.message = "Maternity Leave Application has been exceeded as per the count";

                    }
                }
            }
            catch (Exception ex)
            {
                result.isActionPerformed = false;
                result.message = ex.Message.ToString();
            }

            return result;
        }

        private AdminActionResult SaveEHleaveapplication(AdminActionModel model, int days, IContextRepository repo)
        {
            AdminActionResult result = new AdminActionResult();
            try
            {                                                
                var EHLeavecount = repo.FirstOrDefault<PhoenixConfig>(x => x.Year == DateTime.Now.Year && x.Location == model.LocationID && x.ConfigKey == "EHLeave");
              
                using (PhoenixEntities context = new PhoenixEntities())
                {
                    var ChkEHCount = (from x in context.PersonLeave
                                      where (x.Person.ID == model.EmployeeID && (x.Status != 4 && x.Status != 3) && x.IsDeleted == false && x.LeaveType == 12)
                                      select x).ToList();

                    if (ChkEHCount != null && ChkEHCount.Count() < 1)
                    {
                        EmployeeLeaveViewModel emp = new EmployeeLeaveViewModel();
                        emp.UserId = model.EmployeeID;
                        emp.FromDate = model.From;
                        emp.ToDate = model.From;
                        emp.Leaves = days;
                        emp.Narration = "Admin Leave Application :" + model.Comments;
                        emp.LeaveType = 12;
                        emp.Status = 2;

                        var leave = Mapper.Map<EmployeeLeaveViewModel, PersonLeave>(emp);
                        leave.Person = repo.FirstOrDefault<Person>(x => x.ID == model.EmployeeID);
                        leave.Person.ID = model.EmployeeID;
                        leave.RequestDate = DateTime.Now;
                        repo.Create(leave, null);
                        repo.Save();

                        result.isActionPerformed = true;
                        result.message = "Election Holiday Leave applied Successfully.";
                    }
                    else
                    {
                        result.isActionPerformed = false;
                        result.message = "Election Holiday Leave Application has been exceeded as per the count.";

                    }
                }
            }
            catch (Exception ex)
            {
                result.isActionPerformed = false;
                result.message = ex.Message.ToString();
            }

            return result;
        }

        private AdminActionResult SaveLLleaveapplication(AdminActionModel model, int days, IContextRepository repo)
        {
            AdminActionResult result = new AdminActionResult();
            try
            {
                EmployeeLeaveViewModel emp = new EmployeeLeaveViewModel();
                emp.UserId = model.EmployeeID;
                emp.FromDate = model.From;
                emp.ToDate = model.To;
                emp.Leaves = days;
                emp.Narration = "Admin Leave Application :" + model.Comments;
                emp.LeaveType = 5;
                emp.Status = 2;


                var leave = Mapper.Map<EmployeeLeaveViewModel, PersonLeave>(emp);
                leave.Person = repo.FirstOrDefault<Person>(x => x.ID == model.EmployeeID);
                leave.Person.ID = model.EmployeeID;
                leave.RequestDate = DateTime.Now;
                repo.Create(leave, null);
                repo.Save();

                result.isActionPerformed = true;
                result.message = "Long leave applied Successfully";
            }
            catch (Exception ex)
            {
                result.isActionPerformed = false;
                result.message = ex.Message.ToString();
            }

            return result;
        }

        private AdminActionResult SaveLWPleaveapplication(AdminActionModel model, int days, IContextRepository repo)
        {
            AdminActionResult result = new AdminActionResult();
            try
            {
                EmployeeLeaveViewModel emp = new EmployeeLeaveViewModel();
                emp.UserId = model.EmployeeID;
                emp.FromDate = model.From;
                emp.ToDate = model.To;
                emp.Leaves = days;
                emp.Narration = "Admin Leave Application :" + model.Comments;
                emp.LeaveType = 2;
                emp.Status = 2;


                var leave = Mapper.Map<EmployeeLeaveViewModel, PersonLeave>(emp);
                leave.Person = repo.FirstOrDefault<Person>(x => x.ID == model.EmployeeID);
                leave.Person.ID = model.EmployeeID;
                leave.RequestDate = DateTime.Now;
                repo.Create(leave, null);
                repo.Save();

                result.isActionPerformed = true;
                result.message = "LWP applied Successfully";
            }
            catch (Exception ex)
            {
                result.isActionPerformed = false;
                result.message = ex.Message.ToString();
            }

            return result;
        }

        private AdminActionResult SaveSHFleaveapplication(AdminActionModel model, int days, IContextRepository repo)
        {
            AdminActionResult result = new AdminActionResult();
            using (PhoenixEntities context = new PhoenixEntities())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        //Add Person Leave
                        EmployeeLeaveViewModel emp = new EmployeeLeaveViewModel();
                        emp.UserId = model.EmployeeID;
                        emp.FromDate = model.From;
                        emp.ToDate = model.From;
                        emp.Leaves = days;
                        emp.Narration = "Admin Leave Application :" + model.Comments;
                        emp.LeaveType = 8;
                        emp.Status = 2;



                        var leave = Mapper.Map<EmployeeLeaveViewModel, PersonLeave>(emp);

                        leave.Person = repo.FirstOrDefault<Person>(x => x.ID == model.EmployeeID);
                        leave.Person.ID = model.EmployeeID;
                        leave.RequestDate = DateTime.Now;
                        repo.Create(leave, null);
                        repo.Save();


                        ////Update Person Leave ledger
                        //ledger.LeaveUtilized += days;
                        //repo.Update(ledger);
                        //repo.Save();

                        transaction.Commit();

                        result.isActionPerformed = true;
                        result.message = string.Format("SFH Leave applied Successfully");
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        result.isActionPerformed = false;
                        result.message = ex.Message.ToString();
                    }
                }
            }
            return result;
        }

        private AdminActionResult SaveCLeaveapplication(AdminActionModel model, int days, PersonLeaveLedger ledger, IContextRepository repo)
        {
            AdminActionResult result = new AdminActionResult();
            try
            {
                EmployeeLeaveViewModel emp = new EmployeeLeaveViewModel();
                emp.UserId = model.EmployeeID;
                emp.FromDate = model.From;
                emp.ToDate = model.To;
                emp.Leaves = days;
                emp.Narration = "Admin Casual Leave Application :" + model.Comments;
                emp.LeaveType = 9;
                emp.Status = 2;


                var leave = Mapper.Map<EmployeeLeaveViewModel, PersonLeave>(emp);
                leave.Person = repo.FirstOrDefault<Person>(x => x.ID == model.EmployeeID);
                leave.Person.ID = model.EmployeeID;
                leave.RequestDate = DateTime.Now;
                repo.Create(leave, null);
                repo.Save();

                if (ledger != null)
                {
                    //ledger.CLUtilized += days;
                    ledger.CLUtilized = (ledger?.CLUtilized ?? 0) + days;

                    repo.Update(ledger);
                    repo.Save();
                }
                result.isActionPerformed = true;
                result.message = "Casual Leave applied Successfully";
            }
            catch (Exception ex)
            {
                result.isActionPerformed = false;
                result.message = ex.Message.ToString();
            }

            return result;
        }

        private AdminActionResult SaveSLeaveapplication(AdminActionModel model, int days, PersonLeaveLedger ledger, IContextRepository repo)
        {
            AdminActionResult result = new AdminActionResult();
            try
            {
                EmployeeLeaveViewModel emp = new EmployeeLeaveViewModel();
                emp.UserId = model.EmployeeID;
                emp.FromDate = model.From;
                emp.ToDate = model.To;
                emp.Leaves = days;
                emp.Narration = "Admin Sick Leave Application :" + model.Comments;
                emp.LeaveType = 11;
                emp.Status = 2;


                var leave = Mapper.Map<EmployeeLeaveViewModel, PersonLeave>(emp);
                leave.Person = repo.FirstOrDefault<Person>(x => x.ID == model.EmployeeID);
                leave.Person.ID = model.EmployeeID;
                leave.RequestDate = DateTime.Now;
                repo.Create(leave, null);
                repo.Save();

                if (ledger != null)
                {
                    //ledger.SLUtilized += days;
                    ledger.SLUtilized = (ledger?.SLUtilized ?? 0)+ days;
                    repo.Update(ledger);
                    repo.Save();
                }
                result.isActionPerformed = true;
                result.message = "Sick Leave applied Successfully";
            }
            catch (Exception ex)
            {
                result.isActionPerformed = false;
                result.message = ex.Message.ToString();
            }

            return result;
        }

        public AdminActionResult Delete(IContextRepository repo, AdminActionModel model)
        {
            AdminActionResult result = new AdminActionResult();
            try
            {
                var personLeave = repo.FirstOrDefault<PersonLeave>(t => t.ID == model.ID);

                personLeave.IsDeleted = true;
                personLeave.Status = 4; //For: #149796668 To update Status as '4' if leave deleted
                repo.Update(personLeave);
                repo.Save();

                //reduce from ledger table

                //int countDays = CalculateLeavesApplied(repo, model.EmployeeID, 0, model.From, model.To);

                var ledger = repo.FirstOrDefault<PersonLeaveLedger>(t => t.PersonID == model.EmployeeID && t.Year == DateTime.Now.Year);
                if (model.LeaveType == 0)
                {
                    if (ledger != null)
                    {
                        ledger.CompOffUtilized = (ledger?.CompOffUtilized ?? 0) - personLeave.Leaves;
                        repo.Update(ledger);
                        repo.Save();
                    }
                    //var compoff = repo.FirstOrDefault<CompOff>(t => t.LeaveRequestID == personLeave.ID);

                    //compoff.IsApplied = false;
                    //compoff.LeaveRequestID = 0;
                    //repo.Update(compoff);
                    //repo.Save();

                    //Changes done for #148821553 --To credit deleted comp-off to user
                    var empLeavesTaken = repo.GetAll<CompOff>().Where(a => a.Person.ID == model.EmployeeID && a.LeaveRequestID == personLeave.ID).ToList();
                    foreach (var item in empLeavesTaken)
                    {
                        if (item.ExpiresOn >= DateTime.Now)
                            item.Status = 1;
                        else
                            item.Status = 4;

                        item.IsApplied = false;
                        item.LeaveRequestID = 0;
                        repo.Update(item);
                        repo.Save();
                    }

                    result.isActionPerformed = true;
                    result.message = string.Format("Comp - Off Deleted Successfully");

                }
                else if (model.LeaveType == 1)
                {
                    if (ledger != null)
                    {
                        ledger.LeaveUtilized = (ledger?.LeaveUtilized ?? 0) - personLeave.Leaves;
                        repo.Update(ledger);
                        repo.Save();
                    }
                    result.isActionPerformed = true;
                    result.message = string.Format("Privilege Leave Deleted Successfully");
                }
                else if (model.LeaveType == 2)
                {
                    result.isActionPerformed = true;
                    result.message = string.Format("Leave Without Pay Deleted Successfully");
                }
                else if (model.LeaveType == 3)
                {
                    result.isActionPerformed = true;
                    result.message = string.Format("Maternity Deleted Successfully");
                }
                else if (model.LeaveType == 4)
                {
                    result.isActionPerformed = true;
                    result.message = string.Format("Paternity Leave Deleted Successfully");
                }
                else if (model.LeaveType == 5)
                {
                    result.isActionPerformed = true;
                    result.message = string.Format("Long Leave Deleted Successfully");
                }
                else if (model.LeaveType == 8)
                {
                    result.isActionPerformed = true;
                    result.message = string.Format("SFH Leave Deleted Successfully");
                }
                else if (model.LeaveType == 9)
                {
                    if (ledger != null)
                    {
                        ledger.CLUtilized = (ledger?.CLUtilized ?? 0) - personLeave.Leaves;
                        repo.Update(ledger);
                        repo.Save();
                    }
                    result.isActionPerformed = true;
                    result.message = string.Format("Casual Leave Deleted Successfully");
                }
                else if (model.LeaveType == 10)
                {
                    result.isActionPerformed = true;
                    result.message = string.Format("MTP Leave Deleted Successfully");
                }
                else if (model.LeaveType == 11)
                {
                    if (ledger != null)
                    {
                        ledger.SLUtilized = (ledger?.SLUtilized ?? 0) - personLeave.Leaves;
                        repo.Update(ledger);
                        repo.Save();
                    }
                    result.isActionPerformed = true;
                    result.message = string.Format("Sick Leave Deleted Successfully");
                }
                else if (model.LeaveType == 12)
                {
                    if (ledger != null)
                    {
                        ledger.LeaveUtilized = (ledger?.LeaveUtilized ?? 0) - personLeave.Leaves;
                        repo.Update(ledger);
                        repo.Save();
                    }
                    result.isActionPerformed = true;
                    result.message = string.Format("Election Holiday Leave Deleted Successfully");
                }
                if (result.isActionPerformed)
                {
                    using (PhoenixEntities context = new PhoenixEntities())
                    {
                        var holidayList = context.HolidayList.Where(a => a.Location == model.LocationID && a.IsDeleted == false && a.Date.Value.Month == model.From.Date.Month && a.HolidayYear == model.To.Year);
                        IEnumerable<SignInSignOut> lstEmployeeSISOResult = new List<SignInSignOut>();
                        lstEmployeeSISOResult = context.SignInSignOut.Where(x => (x.UserID == model.EmployeeID) && (x.AttendanceDate.Value >= model.From.Date && x.AttendanceDate.Value <= model.To.Date) && (x.DayNotation.ToUpper() == "P")).ToList();
                        //lstEmployeeSISOResult = Mapper.Map<IEnumerable<SignInSignOut>, IEnumerable<EmployeeSISOViewModel>>(empSISO.Where(x => x.DayNotation.ToUpper() == "P" || x.DayNotation == null));
                        for (DateTime date = model.From; date <= model.To; date = date.AddDays(1))
                        {
                            if (date.Date < DateTime.Now.Date && date.DayOfWeek >= DayOfWeek.Monday && date.DayOfWeek <= DayOfWeek.Friday && !holidayList.Any(d => d.Date.Value.Month == date.Month && d.Date.Value.Day == date.Day) && !lstEmployeeSISOResult.Any(d => d.AttendanceDate.Value.Month == date.Month && d.AttendanceDate.Value.Day == date.Day))
                            {
                                SignInSignOut emp = new SignInSignOut();
                                emp.UserID = model.EmployeeID;
                                emp.SignInTime = date;
                                emp.statusID = 2;
                                emp.IsBulk = true;
                                emp.LastModified = DateTime.Now;
                                emp.FirstModified = DateTime.Now;
                                emp.DayNotation = "A";
                                emp.AttendanceDate = date;
                                repo.Create(emp, null);
                            }
                        }
                        repo.Save();
                    }
                }
            }
            catch (Exception ex)
            {
                result.isActionPerformed = false;
                result.message = ex.Message.ToString();
            }

            return result;
        }

        public int CalculateLeavesApplied(IContextRepository repo, int userId, int location, DateTime fromDate, DateTime toDate)
        {
            var holidayList = repo.FindBy<HolidayList>(x => x.Location == location && (x.Date >= fromDate && x.Date <= toDate) && x.IsDeleted == false && x.HolidayYear == fromDate.Year);

            var signInSignOut = repo.FindBy<SignInSignOut>(x => x.UserID == userId && x.SignInTime.Value.Month == fromDate.Month);
            Boolean isDayShift = Convert.ToBoolean(repo.FirstOrDefault<PersonEmployment>(a => a.Person.ID == userId).IsDayShift);

            int count = 0;
            for (DateTime index = fromDate; index <= toDate; index = index.AddDays(1))
            {
                if (index.DayOfWeek != DayOfWeek.Saturday && index.DayOfWeek != DayOfWeek.Sunday)
                {
                    bool excluded = false;
                    foreach (var holiday in holidayList)
                    {
                        if (index.Date.CompareTo(holiday.Date) == 0)
                        {
                            excluded = true;
                            break;
                        }
                    }

                    foreach (var signin in signInSignOut)
                    {
                        if (index.Date.CompareTo(signin.SignInTime.Value.Date) == 0)
                        {
                            if (signin.SignOutTime != null)
                            {
                                if ((signin.SignInTime.Value.Date == signin.SignOutTime.Value.Date && !isDayShift) || (signin.SignInTime.Value.Date != signin.SignOutTime.Value.Date && isDayShift))
                                {

                                }
                                else if (signin.DayNotation == "P")
                                {
                                    excluded = true;
                                    //count = count - 1;
                                    //return count;
                                    return 0;
                                }
                            }
                            else if (signin.SignOutTime == null && !isDayShift)
                            {

                            }
                            else if (signin.DayNotation == "P")
                            {
                                excluded = true;
                                return 0;
                                //count = count - 1;
                                //return count;
                            }
                        }
                    }

                    if (!excluded)
                    {
                        count++;
                    }
                }
            }
            return count;
        }

        private bool CheckLeaveConflicts(IContextRepository repo, DateTime fromdate, DateTime to, int userID)
        {
            using (PhoenixEntities context = new PhoenixEntities())
            {
                var data = (from x in context.PersonLeave
                            where (x.Person.ID == userID && x.IsDeleted == false && ((fromdate >= x.FromDate && fromdate <= x.ToDate) || (to >= x.FromDate && to <= x.ToDate)) && (x.Status != 4 && x.Status != 3))
                            select x).ToList();

                if (data != null && data.Count() == 0)
                {
                    data = (from x in context.PersonLeave
                            where (x.Person.ID == userID && x.IsDeleted == false && fromdate <= x.FromDate && to >= x.ToDate && (x.Status != 4 && x.Status != 3))
                            select x).ToList();
                }
                return data != null && data.Count() > 0 ? true : false;
            }
        }
    }
}
