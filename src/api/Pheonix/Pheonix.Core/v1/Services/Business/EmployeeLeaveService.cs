using AutoMapper;
using Pheonix.Core.v1.Builders;
using Pheonix.Core.v1.Services.Approval;
using Pheonix.Core.v1.Services.Email;
using Pheonix.DBContext;
using Pheonix.Models.Models;
using Pheonix.Models.Models.AdminConfig;
using Pheonix.Models.VM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Configuration;
using System.Data;
using Microsoft.VisualBasic.FileIO;
using System.Text;
using System.IO;

namespace Pheonix.Core.v1.Services.Business
{
    public class EmployeeLeaveService : IEmployeeLeaveService
    {
        #region Class level variables

        private IBasicOperationsService service;
        private IEmailService _emailService;

        #endregion Class level variables

        #region Constructor

        public EmployeeLeaveService(IBasicOperationsService opsService, IEmailService opsEmailService)
        {
            service = opsService;
            _emailService = opsEmailService;
        }

        #endregion Constructor

        #region Public Contracts

        public async Task<LeaveViewModel<T>> GetLeaveDetails<T>(int userID, int location, int leaveType/*RS:not in use*/, DateTime fromDate, DateTime toDate, Func<List<PersonLeave>, List<PersonLeave>> trim = null)
        {
            EmployeeLeaveBuilder<T> builder = new EmployeeLeaveDetailBuilder<T>(service);

            return await builder.GetViewModel(userID, fromDate, toDate, fromDate.Year);
        }

        public async Task<LeaveViewModel<T>> GetLeaveDetailsForApproval<T>(int userID)
        {
            EmployeeLeaveBuilder<T> builder = new EmployeeApprovalLeaveDetailBuilder<T>(service);

            return await builder.GetViewModel(userID, null, null, 0);
        }

        public async Task<LeaveViewModel<T>> GetCompOffDetailsForApproval<T>(int userID)
        {
            EmployeeLeaveBuilder<T> builder = new EmployeeApprovalCompOffDetailBuilder<T>(service);

            return await builder.GetViewModel(userID, null, null, 0);
        }

        public async Task<IEnumerable<HolidayListViewModel>> GetHolidayList(int location, int year)
        {
            return await Task.Run(() =>
            {
                var lstHolidayCurrentYear = service.Top<HolidayList>(0, a => a.Location == location && a.IsDeleted == false && a.HolidayYear == year).OrderBy(x => x.Date).ToList();
                var lstHolidayNextYear = service.Top<HolidayList>(0, a => a.Location == location && a.IsDeleted == false && a.HolidayYear == year + 1).OrderBy(x => x.Date).ToList();
                var lstHoliday = lstHolidayCurrentYear.Union(lstHolidayNextYear);
                var objHolidayList = Mapper.Map<IEnumerable<HolidayList>, IEnumerable<HolidayListViewModel>>(lstHoliday);
                return objHolidayList;
            });
        }

        public async Task<HolidayListViewModel> GetUpcomingHoliday(int location, DateTime todayDate)
        {
            return await Task.Run(() =>
            {
                var lstHoliday = service.Top<HolidayList>(0, a => a.Location == location && a.IsDeleted == false && a.Date >= todayDate && a.HolidayYear == todayDate.Year).OrderBy(x => x.Date).ToList();
                if (lstHoliday.Count > 0)
                {
                    var objHolidayList = Mapper.Map<HolidayList, HolidayListViewModel>(lstHoliday.First());
                    return objHolidayList;
                }
                return null;
            });
        }

        public async Task<EnvelopeModel<EmployeeLeaveViewModel>> ApplyOrUpdateLeave(int userID, EmployeeLeaveViewModel model, int location)
        {
            return await Task.Run(async () =>
            {
                /*
                 * fetch All leaves Between apploed dates
                 * Check Leave Application Type (new or update)
                 * Validate If email is not conflicting
                 * Calculate the Leave Days
                 * Submit the leave
                 * Save to Approval only if this is a new Leave
                 */
                model.FromDate = new DateTime(model.FromDate.Year, model.FromDate.Month, model.FromDate.Day, 0, 0, 0);
                model.ToDate = new DateTime(model.ToDate.Year, model.ToDate.Month, model.ToDate.Day, 0, 0, 0);
                var isError = false;
                var errorMessage = string.Empty;
                bool isEdit = false;

                var leave = CreateLeaveApplication(model, userID, location);
                if (leave.Status == 1)
                    errorMessage = ValidateLeaves(userID, leave, location);

                if (errorMessage != string.Empty)
                    isError = true;
                else
                {
                    if (leave.Status == 2)
                    {
                        for (DateTime index = leave.FromDate; index <= leave.ToDate; index = index.AddDays(1))
                        {
                            IEnumerable<SignInSignOut> userList = service.Top<SignInSignOut>(0, a => a.UserID == model.UserId);
                            var oldObj = userList.Where(a => a.SignInTime.HasValue && a.SignInTime.Value.Date == index).FirstOrDefault();
                            if (oldObj != null)
                            {
                                if (leave.LeaveType == 8)
                                {
                                    var isDelete = service.Remove<SignInSignOut>(oldObj, a => a.SignInSignOutID == oldObj.SignInSignOutID);
                                    if (isDelete)
                                        service.Finalize(true);
                                }
                                else if (oldObj.DayNotation == "A")
                                {
                                    var isDelete = service.Remove<SignInSignOut>(oldObj, a => a.SignInSignOutID == oldObj.SignInSignOutID);
                                    if (isDelete)
                                        service.Finalize(true);
                                }
                            }
                        }
                    }
                    if (leave.Status == 3)
                    {
                        var approverPerson = service.First<Person>(x => x.ID == userID);
                        string approverName = approverPerson.FirstName + " " + approverPerson.LastName;
                        leave.Narration = leave.Narration + " <br><b>" + approverName + ":</b>" + model.StatusComment;
                    }
                    if (leave.ID != 0)
                        isEdit = true;
                    var success = SubmitLeave(leave, userID);
                    if (success)
                    {
                        service.Finalize(true);
                        Person person = null, approverPerson = null; int approverIDs;

                        if (leave.ID != 0 && leave.Status == 1 && leave.Status != 4)
                        {
                            person = leave.Person;
                            if (person == null)
                                person = service.First<Person>(x => x.ID == userID);
                            var approval = service.First<Pheonix.DBContext.Approval>(x => x.RequestType == 1 && x.RequestID == leave.ID && x.Status == 0);
                            if (approval != null && leave.Status == 1)
                            {
                                bool isRemoveApproval = service.Remove<Pheonix.DBContext.Approval>(approval, x => x.ID == approval.ID);
                                var approvalDetails = service.First<ApprovalDetail>(x => x.ApprovalID == approval.ID);
                                approverIDs = (int)approvalDetails.ApproverID;
                                bool isRemoveApprovalDetail = service.Remove<ApprovalDetail>(approvalDetails, x => x.ID == approvalDetails.ID);
                                if (isRemoveApproval && isRemoveApprovalDetail)
                                    service.Finalize(true);
                            }
                            approverIDs = HookApproval(person.ID, leave.ID);
                            approverPerson = service.First<Person>(x => x.ID == approverIDs);
                            UpdateLedger(userID, leave, isEdit, location);
                            string empName = person.FirstName + " " + person.LastName;
                            string approverName = approverPerson.FirstName + " " + approverPerson.LastName;

                            EmailLeaveDetails objEmailLeaveDetails = new EmailLeaveDetails();

                            objEmailLeaveDetails.personID = person.ID;
                            objEmailLeaveDetails.personName = empName;
                            objEmailLeaveDetails.personOrganizationEmail = person.PersonEmployment.First().OrganizationEmail;
                            objEmailLeaveDetails.approverName = approverName;
                            objEmailLeaveDetails.approverOrganizationEmail = approverPerson.PersonEmployment.First().OrganizationEmail;
                            objEmailLeaveDetails.FromDate1 = leave.FromDate.Date.ToString("MM/dd/yyyy");
                            objEmailLeaveDetails.ToDate1 = leave.ToDate.Date.ToString("MM/dd/yyyy");
                            objEmailLeaveDetails.leaveCount = (int)leave.Leaves;
                            objEmailLeaveDetails.personImage = person.Image;
                            objEmailLeaveDetails.ID = leave.ID;
                            objEmailLeaveDetails.Narration = leave.Narration;
                            objEmailLeaveDetails.LeaveType = leave.LeaveType.GetValueOrDefault();

                            _emailService.SendLeaveApproval(objEmailLeaveDetails);

                            //To trigger notice period extended mail for On Notice employee
                            if (person.PersonEmployment.First().EmploymentStatus == 8)
                            {
                                int EPMId = person.PersonEmployment.First().ExitProcessManager.Value;//service.First<Separation>(x => x.PersonID == userID && x.StatusID != 1).ApprovalID;
                                Person reporting = service.First<Person>(x => x.ID == EPMId);

                                _emailService.SendResignationEmail(null, "Leave utilized during Notice period by " + empName + " (" + userID.ToString() + ")", userID.ToString() + "," + leave.ID.ToString(), Convert.ToString(ConfigurationManager.AppSettings["helpdeskEmailId"]), reporting.PersonEmployment.First().OrganizationEmail, false, false, 17, "", "Vibrant Desk");
                            }
                        }
                        else
                        {
                            leave = service.First<PersonLeave>(x => x.ID == leave.ID);
                            person = leave.Person;
                            string empName = person.FirstName + " " + person.LastName;

                            var approval = service.First<Pheonix.DBContext.Approval>(x => x.RequestType == 1 && x.RequestID == leave.ID);

                            if (leave.Status == 4)
                            {
                                if (approval != null)
                                {
                                    approval.Status = 4;
                                    var approvalId = approval.ID;
                                    bool isRemoveApproval = service.Remove<Pheonix.DBContext.Approval>(approval, x => x.ID == approvalId);

                                    var approvalDetails = service.First<ApprovalDetail>(x => x.ApprovalID == approvalId);
                                    bool isRemoveApprovalDetail = service.Remove<ApprovalDetail>(approvalDetails, x => x.ID == approvalDetails.ID);

                                    if (isRemoveApproval && isRemoveApprovalDetail)
                                    {
                                        service.Finalize(true);
                                        UpdateLedger(person.ID, leave, isEdit, location);
                                        _emailService.SendLeaveCancellationStatus(person.ID, empName, person.PersonEmployment.First().OrganizationEmail, leave.FromDate.Date.ToString("MM/dd/yyyy"), leave.ToDate.Date.ToString("MM/dd/yyyy"), (int)leave.Leaves, leave.Narration, "cancelled", person.Image, leave.LeaveType);

                                    }
                                }
                            }
                            else
                            {
                                approverIDs = (int)service.First<ApprovalDetail>(x => x.ApprovalID == approval.ID).ApproverID;
                                await UpdateHookedApproval(person.ID, 1, leave.ID, (int)leave.Status - 1, model.StatusComment);

                                approverPerson = service.First<Person>(x => x.ID == approverIDs);
                                string approverName = approverPerson.FirstName + " " + approverPerson.LastName;
                                UpdateLedger(person.ID, leave, isEdit, location);
                                if (leave.Status != 4)
                                {
                                    _emailService.SendLeaveApprovalStatus(person.ID, empName, person.PersonEmployment.First().OrganizationEmail, approverName, approverPerson.PersonEmployment.First().OrganizationEmail, leave.FromDate.Date.ToString("MM/dd/yyyy"), leave.ToDate.Date.ToString("MM/dd/yyyy"), (int)leave.Leaves, (int)leave.Status - 1 == 1 ? "Approved" : "Rejected", person.Image, leave.LeaveType);
                                }
                            }

                            //To trigger notice period extended mail for On Notice employee
                            if (person.PersonEmployment.First().EmploymentStatus == 8 && leave.Status != 4)
                                _emailService.SendResignationEmail(null, "Leave utilized during Notice period by " + empName + " (" + userID.ToString() + ")", userID.ToString() + "," + leave.ID.ToString(), Convert.ToString(ConfigurationManager.AppSettings["helpdeskEmailId"]), person.PersonEmployment.First().OrganizationEmail, false, false, 17, "", "Vibrant Desk");
                        }
                    }
                }
                return CreateEnvelope(GetLatestModel(userID, model), isError, errorMessage);
            });
        }

        public async Task<EnvelopeModel<EmployeeLeaveViewModel>> ApproveOrRejectLeave(int userID, EmployeeLeaveViewModel model, int location)
        {
            return await Task.Run(async () =>
            {
                /*
                 * fetch All leaves Between apploed dates
                 * Check Leave Application Type (new or update)
                 * Validate If email is not conflicting
                 * Calculate the Leave Days
                 * Submit the leave
                 * Save to Approval only if this is a new Leave
                 */
                var isError = false;
                var errorMessage = string.Empty;
                bool isEdit = false;
                var count = 0;

                PersonLeave leave;
                leave = Mapper.Map<EmployeeLeaveViewModel, PersonLeave>(model);
                leave.Person = service.First<Person>(x => x.ID == model.UserId);
                leave.Person.ID = model.UserId;
                leave.RequestDate = DateTime.Now;

                if (leave.Status == 2)
                {
                    for (DateTime index = leave.FromDate; index <= leave.ToDate; index = index.AddDays(1))
                    {
                        IEnumerable<SignInSignOut> userList = service.Top<SignInSignOut>(0, a => a.UserID == model.UserId);
                        var oldObj = userList.Where(a => a.SignInTime.HasValue && a.SignInTime.Value.Date == index).FirstOrDefault();
                        if (oldObj != null)
                        {
                            if (leave.LeaveType == 8)
                            {
                                var isDelete = service.Remove<SignInSignOut>(oldObj, a => a.SignInSignOutID == oldObj.SignInSignOutID);
                                if (isDelete)
                                {
                                    service.Finalize(true);
                                    count++;
                                }
                            }
                            else if (oldObj.DayNotation == "A")
                            {
                                var isDelete = service.Remove<SignInSignOut>(oldObj, a => a.SignInSignOutID == oldObj.SignInSignOutID);
                                if (isDelete)
                                {
                                    service.Finalize(true);
                                    count++;
                                }
                            }
                        }
                    }
                }
                if (leave.Status == 3)
                {
                    var approverPerson = service.First<Person>(x => x.ID == userID);
                    string approverName = approverPerson.FirstName + " " + approverPerson.LastName;
                    leave.Narration = leave.Narration + " <br><b>" + approverName + ":</b>" + model.StatusComment;
                }
                if (leave.ID != 0)
                    isEdit = true;
                var isUpdate = false;
                var old = service.First<PersonLeave>(x => x.ID == leave.ID);
                if (count > 0)
                {
                    leave.NotConsumed = old.NotConsumed - count;
                }
                isUpdate = service.Update<PersonLeave>(leave, old);
                if (isUpdate)
                {
                    service.Finalize(true);
                    Person person = null, approverPerson = null; int approverIDs;
                    leave = service.First<PersonLeave>(x => x.ID == leave.ID);
                    person = service.First<Person>(x => x.ID == leave.Person.ID);
                    var approval = service.First<Pheonix.DBContext.Approval>(x => x.RequestType == 1 && x.RequestID == leave.ID && (x.Status == 0 || x.Status == 1));
                    approverIDs = (int)service.First<ApprovalDetail>(x => x.ApprovalID == approval.ID).ApproverID;
                    await UpdateHookedApproval(person.ID, 1, leave.ID, (int)leave.Status - 1, model.StatusComment);
                    approverPerson = service.First<Person>(x => x.ID == approverIDs);
                    if (leave.LeaveType != 6 && leave.LeaveType != 8)
                    {
                        UpdateLedger(person.ID, leave, isEdit);
                    }
                    string empName = person.FirstName + " " + person.LastName;
                    string approverName = approverPerson.FirstName + " " + approverPerson.LastName;
                    if (leave.Status != 4)
                        _emailService.SendLeaveApprovalStatus(person.ID, empName, person.PersonEmployment.First().OrganizationEmail, approverName, approverPerson.PersonEmployment.First().OrganizationEmail, leave.FromDate.Date.ToString("MM/dd/yyyy"), leave.ToDate.Date.ToString("MM/dd/yyyy"), (int)leave.Leaves, (int)leave.Status - 1 == 1 ? "Approved" : "Rejected", person.Image, leave.LeaveType);  //For: #149624435 - Change done to display leave From date, To date & No. of days
                }
                return CreateEnvelope(GetLatestModel(userID, model), isError, errorMessage);
            });
        }

        public async Task<EmployeeLeaveViewModel> ApproveLeave(int applicantID, int approverID, EmployeeLeaveViewModel model)
        {
            //var empLeave = Mapper.Map<EmployeeLeaveViewModel, PersonLeave>(model);
            return await Task.Run(() =>
            {
                var empLeaves = new List<PersonLeave>();

                var approveEmpLeaves = Mapper.Map<EmployeeLeaveViewModel, PersonLeave>(model);
                // Commented by Nilesh. As this was creating me conflict. Once done need to recheck this line.
                //approveEmpLeaves.ApproverID = approverID;
                var oldObj = service.First<PersonLeave>(x => x.Person.ID == applicantID && x.ID == approveEmpLeaves.ID);
                if (oldObj != null)
                {
                    service.Finalize(service.Update<PersonLeave>(approveEmpLeaves, oldObj));
                }
                var leaveStatus = service.Top<PersonLeave>(0, x => x.Person.ID == applicantID && x.ID == approveEmpLeaves.ID);
                if (leaveStatus.Count() > 0)
                    return Mapper.Map<PersonLeave, EmployeeLeaveViewModel>(leaveStatus.FirstOrDefault());

                return null;
            });
        }

        public async Task<ApprovalCompOffViewModel> ApproveCompOff(int applicantID, int approverID, ApprovalCompOffViewModel model)
        {
            var approveEmpLeaves = Mapper.Map<ApprovalCompOffViewModel, CompOff>(model);
            approveEmpLeaves.Person = service.First<Person>(x => x.ID == model.UserId);
            if (model.Status == 1)
            {
                approveEmpLeaves.Status = 1;
                var oldObj = service.First<CompOff>(x => x.Person.ID == approveEmpLeaves.Person.ID && x.ID == approveEmpLeaves.ID);
                if (oldObj != null)
                {
                    //approveEmpLeaves.ExpiresOn = DateTime.Now.AddMonths(3);
                    approveEmpLeaves.Year = DateTime.Now.Year;
                    bool isCompOffUpdated = service.Update<CompOff>(approveEmpLeaves, oldObj);
                    service.Finalize(isCompOffUpdated);
                    var oldPersonLeaveLedger = service.First<PersonLeaveLedger>(x => x.PersonID == approveEmpLeaves.Person.ID && x.Year == DateTime.Now.Year);
                    var updatePersonLeaveLedger = oldPersonLeaveLedger;
                    updatePersonLeaveLedger.CompOffs = updatePersonLeaveLedger.CompOffs + 1;
                    bool isLedgerUpdated = service.Update<PersonLeaveLedger>(updatePersonLeaveLedger, oldPersonLeaveLedger);
                    service.Finalize(isLedgerUpdated);
                    await UpdateHookedApproval(oldObj.Person.ID, 4, oldObj.ID, model.Status, model.StatusComment);
                }
            }
            else
            {
                approveEmpLeaves.Status = 3;
                approveEmpLeaves.Narration = model.StatusComment;
                var oldObj = service.First<CompOff>(x => x.Person.ID == approveEmpLeaves.Person.ID && x.ID == approveEmpLeaves.ID);
                bool isCompOffUpdated = service.Update<CompOff>(approveEmpLeaves, oldObj);
                service.Finalize(isCompOffUpdated);
                await UpdateHookedApproval(oldObj.Person.ID, 4, oldObj.ID, model.Status, model.StatusComment);
            }
            return await Task.Run(() =>
            {
                var leaveStatus = service.Top<CompOff>(0, x => x.Person.ID == applicantID && x.ID == approveEmpLeaves.ID);
                if (leaveStatus.Count() > 0)
                    return Mapper.Map<CompOff, ApprovalCompOffViewModel>(leaveStatus.FirstOrDefault());

                return null;
            });
        }

        public async Task<LeaveViewModel<ApprovalLeaveViewModel>> GetleaveApprovalHistory(int userId)
        {
            return await Task.Run(() =>
            {
                LeaveViewModel<ApprovalLeaveViewModel> data = new LeaveViewModel<ApprovalLeaveViewModel>();
                using (PhoenixEntities context = new PhoenixEntities())
                {
                    List<GetLeaveApprovalHistory_Result> approvalHistory = context.GetLeaveApprovalHistory(userId).ToList();
                    var ids = approvalHistory.Select(x => x.RequestId).ToArray();
                    var e = service.Top<PersonLeave>(0, a => ids.Contains(a.ID));
                    e = e.OrderByDescending(t => t.RequestDate);
                    var empLeaves = Mapper.Map<IEnumerable<PersonLeave>, IEnumerable<ApprovalLeaveViewModel>>(e);
                    foreach (var item in empLeaves)
                    {
                        item.StatusComment = approvalHistory.Where(x => x.RequestId == item.ID).Select(x => x.StatusComment).FirstOrDefault();
                    }
                    data.EmployeeLeaveViewModels = empLeaves;
                }

                return data;
            });
        }

        #endregion Public Contracts

        #region helpers

        private string ValidateLeaves(int userID, PersonLeave leave, int location)
        {
            var errorMessage = string.Empty;
            var empLeavesTaken = service.Top<PersonLeaveLedger>(0, a => (userID == -1 || a.Person.ID == userID) && a.Year == leave.FromDate.Year).ToList();
            var totalCompOffCount = service.All<CompOff>().Where(a => (userID == -1 || a.PersonID == userID) && a.Status == 1 && a.IsApplied == false && a.Year == DateTime.Now.Year).ToList(); //For:#149797764
            bool reportingMngr = service.All<PersonReporting>().Where(a => (a.PersonID == userID && a.ReportingTo > 0)).Count() > 0;
            if (!reportingMngr)
                return "You can not apply leave since approver detail is missing";
            if (leave.FromDate > leave.ToDate)
                return "To Date can not be less than From Date";
            if (leave.FromDate.Year != leave.ToDate.Year && (leave.LeaveType == 1 || leave.LeaveType == 9 || leave.LeaveType == 11))
                return "You can not apply any leave of combain year";
            if (leave.ToDate.Year > DateTime.Now.Year && (leave.LeaveType == 1 || leave.LeaveType == 9 || leave.LeaveType == 11))
                return "You can not apply leave on next year. Apply LWP it will convert in to leave according next year leave balance.";
            if (leave.Leaves == 0)
                return "Leaves on Holidays/Weekends are not allowed";
            else if (leave.Leaves == -1)
                return "You can not apply a leave when you are already marked as present";
            else if (leave.ToDate < leave.FromDate)
                return "To date must be greater than From date";
            else
            {
                if (CheckLeaveConflicts(leave.FromDate, leave.ToDate, leave.ID, userID))
                    return "An overlapping leave record already exists";

                if (leave.LeaveType == 0)
                {
                    return ValidateCompOff(userID, leave, totalCompOffCount);

                }
                else if (leave.LeaveType == 1)
                {
                    if (empLeavesTaken.Count <= 0)
                    {
                        //return "There is no data of PL for current year.";
                        return ValidatePriviledgeLeave(userID, leave);
                    }
                    return ValidatePriviledgeLeave(userID, leave, empLeavesTaken[0]);

                }
                else if (leave.LeaveType == 9)
                {

                    if (empLeavesTaken.Count <= 0)
                    {
                        //return "There is no data of CL for current year.";
                        return ValidateCasualLeave(userID, leave);
                    }
                    return ValidateCasualLeave(userID, leave, empLeavesTaken[0]);

                }
                else if (leave.LeaveType == 11)
                {
                    if (empLeavesTaken.Count <= 0)
                    {
                        return ValidateSickLeave(userID, leave);
                        //return "There is no data of SL for current year.";
                    }
                    return ValidateSickLeave(userID, leave, empLeavesTaken[0]);
                }
                else if (leave.LeaveType == 3)
                {
                    return ValidateMaternityLeave(userID);
                }
                else if (leave.LeaveType == 2 && leave.ID != 0)
                {
                    return ValidateLWP(leave);
                }
                else if (leave.LeaveType == 8)
                {
                    return ValidateSFH(userID, leave);
                }

            }
            return errorMessage;
        }

        private string ValidateLWP(PersonLeave leave)
        {
            string resultMessage = string.Empty;

            DateTime actualdate = leave.FromDate.AddMonths(2);
            if (leave.ToDate > actualdate)
                resultMessage = "Number of applied leave days are exceeded as per the LWP count";
            return resultMessage;
        }

        private string ValidateSFH(int userID, PersonLeave leave)
        {
            string resultMessage = string.Empty;

            DateTime today = DateTime.Now.Date;
            var sfhExist = service.Top<PersonLeave>(0, p => (p.Person.ID == userID && p.LeaveType == 8 && p.FromDate.Year == today.Year && (p.Status == 1 || p.Status == 2) && (p.NotConsumed == null || p.NotConsumed == 0)));
            if (sfhExist != null && sfhExist.Count() > 0)
            {
                resultMessage = "SFH already availed for this year. Please connect to HR for clarifications (if any)";
            }
            var isPresent = service.Top<SignInSignOut>(0, s => s.UserID == userID && s.AttendanceDate == leave.FromDate && s.DayNotation == "P");
            if (isPresent != null && isPresent.Count() > 0)
            {
                resultMessage = "You can not apply a leave when you are already marked as present. Please connect to HR for clarifications (if any)";
            }
            return resultMessage;

        }

        private string ValidateMaternityLeave(int userID)
        {
            string resultMessage = string.Empty;

            var leaveCount = service.Top<PersonLeave>(0, x => x.LeaveType == 3 && x.Person.ID == userID && (x.Status == 2 || x.Status == 1) && x.IsDeleted == false).Count();
            if (leaveCount >= 2)
                resultMessage = "Maternity Leave Application has been exceeded as per the count";
            return resultMessage;
        }

        private string ValidateCompOff(int userID, PersonLeave leave, List<CompOff> totalCompOffCount)
        {
            string resultMessage = string.Empty;

            if (leave.ID != 0)
            {
                var totalLeavesList = service.First<PersonLeave>(x => x.Person.ID == userID && x.ID == leave.ID && x.Leaves != null && (x.Status == 1 || x.Status == 2) && x.FromDate.Year == leave.FromDate.Year);
                int totalLeaves = totalLeavesList.Leaves.Value - (totalLeavesList.NotConsumed == null ? 0 : totalLeavesList.NotConsumed.Value);

                if (leave.Leaves > (totalCompOffCount.Count + totalLeaves))
                    resultMessage = "Number of applied leave days are more than the available CompOff balance";
            }
            else if (leave.Leaves > totalCompOffCount.Count)
                resultMessage = "Number of applied leave days are more than the available CompOff balance";

            int count = (int)leave.Leaves - 1;
            if (CheckMaxCompOffExpirationDate(userID, leave.FromDate, count, leave.ToDate, leave.ID))
                resultMessage = "Applied leave is more than available comp-off balance for selected date range OR Your comp-off is expired";

            return resultMessage;
        }

        private string ValidatePriviledgeLeave(int userID, PersonLeave leave, PersonLeaveLedger empLeavesTaken = null)
        {
            string resultMessage = string.Empty;

            var creditPL = service.Top<PersonLeaveCredit>(0, a => (userID == -1 || a.Person.ID == userID) && a.Year == leave.FromDate.Year).Sum(x => x.CreditBalance);
            if (leave.ID != 0)
            {
                var totalPLList = service.First<PersonLeave>(x => x.LeaveType == 1 && x.Person.ID == userID && x.ID == leave.ID && x.Leaves != null && (x.Status == 1 || x.Status == 2) && x.FromDate.Year == leave.FromDate.Year);
                int totalPLLeaves = totalPLList.Leaves.Value - (totalPLList.NotConsumed == null ? 0 : totalPLList.NotConsumed.Value);
                //if ((empLeavesTaken[0].LeaveUtilized + leave.Leaves - totalLeaves) > empLeavesTaken[0].OpeningBalance + GetOrgLeaves(location) + creditLeaves) //SA: As per discuss with Amit GetOrgLeaves() function not in use
                //if ((empLeavesTaken.LeaveUtilized + leave.Leaves - totalPLLeaves) > empLeavesTaken.OpeningBalance + creditPL)
                if (((empLeavesTaken?.LeaveUtilized ?? 0) + leave.Leaves - totalPLLeaves) > (empLeavesTaken?.OpeningBalance ?? 0) + creditPL)
                    resultMessage = "Number of applied leave day(s) is/are more than the available leave balance";
            }
            //else if (leave.Leaves + empLeavesTaken[0].LeaveUtilized > empLeavesTaken[0].OpeningBalance + GetOrgLeaves(location) + creditLeaves) //SA: As per discuss with Amit GetOrgLeaves() function not in use
            //else if (leave.Leaves + empLeavesTaken.LeaveUtilized > empLeavesTaken.OpeningBalance + creditPL)
            else if (leave.Leaves + (empLeavesTaken?.LeaveUtilized ?? 0) > (empLeavesTaken?.OpeningBalance ?? 0) + creditPL)
                resultMessage = "Number of applied leave day(s) is/are more than the available leave balance.";
            return resultMessage;
        }

        private string ValidateCasualLeave(int userID, PersonLeave leave, PersonLeaveLedger empLeavesTaken = null)
        {
            string resultMessage = string.Empty;

            var creditCL = service.Top<PersonCLCredit>(0, a => (userID == -1 || a.Person.ID == userID) && a.Year == leave.FromDate.Year).Sum(x => x.CreditBalance);
            if (leave.ID != 0)
            {
                var totalCLList = service.First<PersonLeave>(x => x.LeaveType == 9 && x.Person.ID == userID && x.ID == leave.ID && x.Leaves != null && (x.Status == 1 || x.Status == 2) && x.FromDate.Year == leave.FromDate.Year);
                int totalCLLeaves = totalCLList.Leaves.Value - (totalCLList.NotConsumed == null ? 0 : totalCLList.NotConsumed.Value);
                //if ((empLeavesTaken[0].LeaveUtilized + leave.Leaves - totalLeaves) > empLeavesTaken[0].OpeningBalance + GetOrgLeaves(location) + creditLeaves) //SA: As per discuss with Amit GetOrgLeaves() function not in use
                //if ((empLeavesTaken.CLUtilized + leave.Leaves - totalCLLeaves) > creditCL)
                if (((empLeavesTaken?.CLUtilized ?? 0) + leave.Leaves - totalCLLeaves) > creditCL)
                    resultMessage = "Number of applied Casual leave day(s) is/are more than the available leave balance";
            }
            //else if (leave.Leaves + empLeavesTaken[0].LeaveUtilized > empLeavesTaken[0].OpeningBalance + GetOrgLeaves(location) + creditLeaves) //SA: As per discuss with Amit GetOrgLeaves() function not in use
            //else if (leave.Leaves + empLeavesTaken.CLUtilized > creditCL)
            else if (leave.Leaves + (empLeavesTaken?.CLUtilized ?? 0) > creditCL)
                resultMessage = "Number of applied Casual leave day(s) is/are more than the available leave balance.";

            return resultMessage;
        }

        private string ValidateSickLeave(int userID, PersonLeave leave, PersonLeaveLedger empLeavesTaken = null)
        {
            string resultMessage = string.Empty;

            var creditSL = service.Top<PersonSLCredit>(0, a => (userID == -1 || a.Person.ID == userID) && a.Year == leave.FromDate.Year).Sum(x => x.CreditBalance);
            if (leave.ID != 0)
            {
                var totalSLList = service.First<PersonLeave>(x => x.LeaveType == 11 && x.Person.ID == userID && x.ID == leave.ID && x.Leaves != null && (x.Status == 1 || x.Status == 2) && x.FromDate.Year == leave.FromDate.Year);
                int totalSLLeaves = totalSLList.Leaves.Value - (totalSLList.NotConsumed == null ? 0 : totalSLList.NotConsumed.Value);
                //if ((empLeavesTaken[0].LeaveUtilized + leave.Leaves - totalLeaves) > empLeavesTaken[0].OpeningBalance + GetOrgLeaves(location) + creditLeaves) //SA: As per discuss with Amit GetOrgLeaves() function not in use
                //if ((empLeavesTaken.SLUtilized + leave.Leaves - totalSLLeaves) > creditSL)
                if (((empLeavesTaken?.SLUtilized ?? 0) + leave.Leaves - totalSLLeaves) > creditSL)
                    resultMessage = "Number of applied Sick leave day(s) is/are more than the available leave balance";
            }
            //else if (leave.Leaves + empLeavesTaken.SLUtilized > creditSL)
            else if (leave.Leaves + (empLeavesTaken?.SLUtilized ?? 0) > creditSL)
                resultMessage = "Number of applied Sick leave day(s) is/are more than the available leave balance.";

            return resultMessage;
        }

        private bool CheckMaxCompOffExpirationDate(int userID, DateTime date, int count, DateTime toDate, int leaveID = 0)
        //private bool CheckMaxCompOffExpirationDate(int userID, DateTime date, int count, int leaveID = 0)
        {
            List<int> empLeavesTaken = new List<int>();

            //    empLeavesTaken = service.All<CompOff>().Where(a => (userID == -1 || a.Person.ID == userID) && ((a.IsApplied == false) || (a.IsApplied == true && a.LeaveRequestID == leaveID)) && a.Status == 1 && a.ExpiresOn >= date).OrderBy(x => x.ExpiresOn).ToList();
            //        var ExpiresOn = empLeavesTaken[count].ExpiresOn;
            //        var id = empLeavesTaken[count].ID;
            //        if (date > ExpiresOn)
            //            return true;
            //        else
            //            return false;          

            var empLeavesTaken1 = service.All<CompOff>().Where(a => (userID == -1 || a.Person.ID == userID) && ((a.IsApplied == false) || (a.IsApplied == true && a.LeaveRequestID == leaveID)) && a.Status == 1).OrderBy(x => x.ExpiresOn).ToList();
            DateTime currDate = DateTime.Now;
            for (DateTime index = date; index <= toDate; index = index.AddDays(1))
            {
                foreach (var signin in empLeavesTaken1)
                {
                    if (index.Date <= signin.ExpiresOn.Value.Date)
                    {
                        if (!empLeavesTaken.Contains(signin.ID) && currDate != index.Date)
                        {
                            empLeavesTaken.Add(signin.ID);
                            currDate = index.Date;
                        }
                    }
                }
            }

            if (empLeavesTaken.Count - 1 < count)
                return true;
            else
                return false;


        }

        private EnvelopeModel<EmployeeLeaveViewModel> CreateEnvelope(EmployeeLeaveViewModel model, bool isError, string errorMessage)
        {
            return new EnvelopeModel<EmployeeLeaveViewModel>(model, isError, errorMessage);
        }

        private bool CheckLeaveConflicts(DateTime from, DateTime to, int id, int userID)
        {
            var range = service.Top<PersonLeave>(0, x => (id != x.ID && x.Person.ID == userID) && ((from >= x.FromDate && from <= x.ToDate) || (to >= x.FromDate && to <= x.ToDate)) && (x.Status != 4 && x.Status != 3) && x.IsDeleted == false).ToList(); // 30-6-17 for #147997217
            if (range != null && range.Count() == 0)
            {
                range = service.Top<PersonLeave>(0, x => id != x.ID && x.Person.ID == userID && from <= x.FromDate && to >= x.ToDate && (x.Status != 4 && x.Status != 3) && x.IsDeleted == false).ToList();
            }
            return range != null && range.Count() > 0 ? true : false;
        }

        private PersonLeave CreateLeaveApplication(EmployeeLeaveViewModel model, int userID, int location)
        {
            PersonLeave leave;
            leave = GetLeavePersonByID(model, userID);
            leave = GetLeaveCountByType(model, location, leave);

            return leave;
        }

        private PersonLeave GetLeavePersonByID(EmployeeLeaveViewModel model, int userID)
        {
            PersonLeave leave;
            leave = Mapper.Map<EmployeeLeaveViewModel, PersonLeave>(model);
            //if (model.ID == 0)
            //{
            leave.Person = service.First<Person>(x => x.ID == userID);
            leave.Person.ID = userID;
            leave.RequestDate = DateTime.Now;
            //}
            return leave;
        }

        private PersonLeave GetLeaveCountByType(EmployeeLeaveViewModel model, int location, PersonLeave leave)
        {
            if (model.LeaveType == 3)
            {
                //SA: As per discuss with Amit, now ML value fetch as per define in Admin Configuration hence commented below code
                //var MLCountry = "MLIndia";
                //if (location == 2)
                //{
                //    MLCountry = "MLUS";
                //}
                //var MLCount = service.First<PhoenixConfig>(x => x.ConfigKey == MLCountry);

                var MLCount = service.First<PhoenixConfig>(x => x.ConfigKey == "ML" && x.Location == location && x.Year == model.FromDate.Year);
                var signInSignOut = service.Top<SignInSignOut>(0, x => x.UserID == leave.Person.ID && x.SignInTime.Value.Month == model.FromDate.Month).ToList();
                DateTime index = model.FromDate;
                var empPresent = 0;
                Boolean isDayShift = Convert.ToBoolean(service.First<PersonEmployment>(a => a.Person.ID == leave.Person.ID).IsDayShift);

                for (int i = 0; i < signInSignOut.Count; i++)
                {
                    if (index.Date.CompareTo(signInSignOut[i].SignInTime.Value.Date) == 0)
                    {
                        if (signInSignOut[i].SignOutTime != null)
                        {
                            if ((signInSignOut[i].SignInTime.Value.Date == signInSignOut[i].SignOutTime.Value.Date && !isDayShift) || (signInSignOut[i].SignInTime.Value.Date != signInSignOut[i].SignOutTime.Value.Date && isDayShift))
                            {

                            }
                            else if (signInSignOut[i].DayNotation == "P")
                            {
                                empPresent = -1;
                            }
                        }
                        else if (signInSignOut[i].SignOutTime == null && !isDayShift)
                        {

                        }
                        else if (signInSignOut[i].DayNotation == "P")
                        {
                            empPresent = -1;
                        }
                    }
                }

                if (empPresent == -1)
                    leave.Leaves = -1;
                else
                    leave.Leaves = int.Parse(MLCount.ConfigValue);

                leave.ToDate = model.FromDate.AddDays(int.Parse(MLCount.ConfigValue) - 1);
            }
            else if (model.LeaveType == 4)
            {
                //SA: As per discuss with Amit, now PL value fetch as per define in Admin Configuration hence commented below code
                //leave.Leaves = 2;
                //leave.ToDate = CalculatePaternityLeave(location, model.FromDate);

                var PLCount = service.First<PhoenixConfig>(x => x.ConfigKey == "PL" && x.Location == location && x.Year == model.FromDate.Year);
                leave.Leaves = int.Parse(PLCount.ConfigValue);
                leave.ToDate = model.FromDate.AddDays(int.Parse(PLCount.ConfigValue) - 1);
            }
            else if (model.LeaveType == 6)
            {
                leave.Leaves = 1;
            }
            else if (model.LeaveType == 7)
            {
                leave.Leaves = 1;
            }
            else if (model.LeaveType == 8)
            {
                leave.Leaves = 1;
                leave.ToDate = model.FromDate;
            }
            else
                leave.Leaves = CalculateLeavesApplied(leave.Person.ID, location, model.FromDate, model.ToDate);

            return leave;
        }

        private bool SubmitLeave(PersonLeave leave, int userID)
        {
            if (leave.ID == 0)
            {
                return service.Create<PersonLeave>(leave, null);
            }
            else
            {
                var old = service.First<PersonLeave>(x => x.ID == leave.ID);
                leave.Person1 = service.First<Person>(x => x.ID == userID);
                return service.Update<PersonLeave>(leave, old);
            }
        }

        private int HookApproval(int userId, int recordID)
        {
            var strategy = ApprovalStrategyFactory.GetStrategy(ApprovalStrategy.OneLevelOnly, userId);
            strategy.opsService = this.service;
            ApprovalService service = new ApprovalService(this.service);
            int[] fetchedApprovers = strategy.FetchApprovers();
            service.SendForApproval(userId, 1, recordID, fetchedApprovers);
            return fetchedApprovers.FirstOrDefault();
        }

        private int GetOrgLeaves(int location)
        {
            string leaveKey = "";
            switch (location)
            {
                case 0:
                    leaveKey = "GlobalLeaveIndia";
                    break;

                case 1:
                    leaveKey = "GlobalLeaveIndia";
                    break;

                case 2:
                    leaveKey = "GlobalLeaveUS";
                    break;
            }
            return int.Parse(service.First<PhoenixConfig>(t => t.ConfigKey == leaveKey).ConfigValue);
        }

        private async Task<int> UpdateHookedApproval(int userId, int requestType, int recordID, int statusID, string statusComment)
        {
            //var strategy = ApprovalStrategyFactory.GetStrategy(ApprovalStrategy.OneLevelOnly, userId);
            //strategy.opsService = this.service;
            ApprovalService service = new ApprovalService(this.service);
            return await service.UpdateApproval(userId, requestType, recordID, statusID, statusComment);
        }

        private void UpdateLedger(int userID, PersonLeave leave, bool isEdit = false, int location = 0)
        {
            var oldLedger = service.First<PersonLeaveLedger>(x => x.Person.ID == userID && x.Year == DateTime.Now.Year);
            var Applyedleaves = service.First<PersonLeave>(x => x.ID == leave.ID);
            //var Applyedleaves = service.All<PersonLeave>().Where(x => x.ID == leave.ID).ToList();

            //var newLedger = oldLedger;
            var totalLeavesList = service.Top<PersonLeave>(0, x => x.Person.ID == userID && x.Leaves != null && (x.Status == 1 || x.Status == 2) && x.FromDate.Year == DateTime.Now.Year && x.IsDeleted == false).ToList();
            int totalLeaves = totalLeavesList.Where(a => a.LeaveType == 1).Sum(a => a.Leaves.Value);
            int totalCL = totalLeavesList.Where(a => a.LeaveType == 9).Sum(a => a.Leaves.Value);
            int totalSL = totalLeavesList.Where(a => a.LeaveType == 11).Sum(a => a.Leaves.Value);
            int totalCompOff = totalLeavesList.Where(a => a.LeaveType == 0).Sum(a => a.Leaves.Value);
            int totalNotConsumedLeaves = totalLeavesList.Where(a => a.LeaveType == 1 && a.NotConsumed != null).Sum(a => a.NotConsumed.Value);
            int totalNotConsumedCL = totalLeavesList.Where(a => a.LeaveType == 9 && a.NotConsumed != null).Sum(a => a.NotConsumed.Value);
            int totalNotConsumedSL = totalLeavesList.Where(a => a.LeaveType == 11 && a.NotConsumed != null).Sum(a => a.NotConsumed.Value);
            int totalNotConsumedCompOff = totalLeavesList.Where(a => a.LeaveType == 0 && a.NotConsumed != null).Sum(a => a.NotConsumed.Value);
            Boolean updateledger = true;
            var newLedger = new PersonLeaveLedger();
            if (leave.LeaveType == 9)
            {
                newLedger = new PersonLeaveLedger
                {
                    CLUtilized = totalCL - totalNotConsumedCL
                };
            }
            else if (leave.LeaveType == 11)
            {
                newLedger = new PersonLeaveLedger
                {
                    SLUtilized = totalSL - totalNotConsumedSL
                };
            }
            else
            {
                newLedger = new PersonLeaveLedger
                {
                    LeaveUtilized = totalLeaves - totalNotConsumedLeaves,
                    CompOffUtilized = oldLedger?.CompOffUtilized ?? 0, //For: #149880643 on 02/08/2017
                    CompOffs = oldLedger?.CompOffs ?? 0
                };

            }

            if (leave.LeaveType == 0)
            {
                if (leave.Status == 4 || leave.Status == 3)
                {
                    newLedger.CompOffUtilized = oldLedger.CompOffUtilized - leave.Leaves;
                    for (var compLeave = 0; compLeave <= ((leave.Leaves) - 1); compLeave++)
                    {
                        var compOff = service.First<CompOff>(x => x.Person.ID == userID && x.IsApplied == true && (x.Status == 1 || x.Status == 2) && x.LeaveRequestID == leave.ID);

                        if (compOff == null)
                            updateledger = false;

                        var newCompOff = new CompOff
                        {
                            IsApplied = false,
                            Status = 1,
                            LeaveRequestID = null
                        };
                        if (updateledger)
                        {
                            bool isUpdateCompoff = service.Update<CompOff>(newCompOff, compOff);
                            if (isUpdateCompoff)
                                service.Finalize(true);
                        }
                    }
                }
                else if (isEdit && leave.Status == 1)
                {

                    //newLedger.CompOffUtilized = oldLedger.CompOffUtilized + Applyedleaves.Leaves;
                    //newLedger.CompOffUtilized = oldLedger.CompOffUtilized - leave.Leaves;
                    //newLedger.CompOffUtilized = newLedger.CompOffUtilized - totalNotConsumedCompOff - Applyedleaves.Leaves; //Commented on 02/08/2017 For: #149880643
                    //newLedger.CompOffUtilized = newLedger.CompOffUtilized - Applyedleaves.Leaves; //For: #149880643 on 02/08/2017
                    //newLedger.CompOffUtilized = newLedger.CompOffUtilized + leave.Leaves;
                    var totalCompOffCount = service.All<CompOff>().Where(x => x.Person.ID == userID && ((x.IsApplied == true && (x.Status == 1 || x.Status == 2)) || (x.IsApplied == false && x.Status == 4)) && x.Year == leave.FromDate.Year).Count();
                    var compOff1 = service.All<CompOff>().Where(x => x.Person.ID == userID && x.IsApplied == true && x.LeaveRequestID == leave.ID).Count();

                    newLedger.CompOffUtilized = totalCompOffCount - compOff1 + leave.Leaves;

                    //for (var compLeave = 0; compLeave <= ((Applyedleaves.Leaves) - 1); compLeave++)
                    for (var compLeave = 0; compLeave <= (Convert.ToInt32(compOff1) - 1); compLeave++)
                    {
                        var compOff = service.First<CompOff>(x => x.Person.ID == userID && x.IsApplied == true && x.Status == 1 && x.LeaveRequestID == leave.ID);
                        var newCompOff = new CompOff
                        {
                            IsApplied = false,
                            Status = 1,
                            LeaveRequestID = null
                        };
                        bool isUpdateCompoff = service.Update<CompOff>(newCompOff, compOff);
                        if (isUpdateCompoff)
                            service.Finalize(true);
                    }
                    #region OldLogic
                    //for (var compLeave = 0; compLeave <= ((leave.Leaves) - 1); compLeave++)
                    //{
                    //    var compOff = service.All<CompOff>().Where(x => x.Person.ID == userID && x.IsApplied == false && x.Status == 1 && x.ExpiresOn.Value.Date >= leave.FromDate.Date).OrderBy(d => d.ExpiresOn).First();
                    //    var newCompOff = new CompOff
                    //    {
                    //        IsApplied = true,
                    //        Status = 1,
                    //        LeaveRequestID = leave.ID
                    //    };
                    //    bool isUpdateCompoff = service.Update<CompOff>(newCompOff, compOff);
                    //    if (isUpdateCompoff)
                    //        service.Finalize(true);
                    //}
                    #endregion

                    //This check is implemented to consumed Comp-Off order wise and Expiry date wise 
                    var holidayList = service.Top<HolidayList>(0, x => x.Location == location && (x.Date >= leave.FromDate.Date && x.Date <= leave.ToDate.Date) && x.IsDeleted == false && x.HolidayYear == leave.ToDate.Date.Year).ToList();
                    var compOffList = service.All<CompOff>().Where(x => x.Person.ID == userID && x.IsApplied == false && x.Status == 1 && x.ExpiresOn.Value.Date >= leave.FromDate.Date).OrderBy(d => d.ExpiresOn).ToList();
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
                                    var compOff = service.All<CompOff>().Where(x => x.Person.ID == userID && x.IsApplied == false && x.Status == 1 && x.ID == _compOffList.ID).OrderBy(d => d.ExpiresOn).First();
                                    var newCompOff = new CompOff
                                    {
                                        IsApplied = true,
                                        Status = 1,
                                        LeaveRequestID = leave.ID
                                    };

                                    bool isUpdateCompoff = service.Update<CompOff>(newCompOff, compOff);

                                    if (isUpdateCompoff)
                                        service.Finalize(true);

                                    currDate = index.Date;
                                }
                            }
                        }
                    }
                }
                else if (leave.Status == 1)
                {
                    //newLedger.CompOffUtilized = oldLedger.CompOffUtilized + leave.Leaves;
                    //newLedger.CompOffUtilized = newLedger.CompOffUtilized - totalNotConsumedCompOff; //Commented on 02/08/2017 For: #149880643
                    newLedger.CompOffUtilized = newLedger.CompOffUtilized + leave.Leaves; //For: #149880643 on 02/08/2017

                    #region OldLogic
                    //for (var compLeave = 0; compLeave <= ((leave.Leaves) - 1); compLeave++)
                    //{
                    //    var compOff = service.All<CompOff>().Where(x => x.Person.ID == userID && x.IsApplied == false && x.Status == 1 && x.ExpiresOn.Value.Date >= leave.FromDate.Date).OrderBy(d => d.ExpiresOn).First();
                    //    var newCompOff = new CompOff
                    //    {
                    //        IsApplied = true,
                    //        Status = 1,
                    //        LeaveRequestID = leave.ID
                    //    };
                    //    bool isUpdateCompoff = service.Update<CompOff>(newCompOff, compOff);
                    //    if (isUpdateCompoff)
                    //        service.Finalize(true);
                    //}
                    #endregion

                    //This check is implemented to consumed Comp-Off order wise and Expiry date wise 
                    var holidayList = service.Top<HolidayList>(0, x => x.Location == location && (x.Date >= leave.FromDate.Date && x.Date <= leave.ToDate.Date) && x.IsDeleted == false && x.HolidayYear == leave.ToDate.Date.Year).ToList();
                    var compOffList = service.All<CompOff>().Where(x => x.Person.ID == userID && x.IsApplied == false && x.Status == 1 && x.ExpiresOn.Value.Date >= leave.FromDate.Date).OrderBy(d => d.ExpiresOn).ToList();
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
                                    var compOff = service.All<CompOff>().Where(x => x.Person.ID == userID && x.IsApplied == false && x.Status == 1 && x.ID == _compOffList.ID).OrderBy(d => d.ExpiresOn).First();
                                    var newCompOff = new CompOff
                                    {
                                        IsApplied = true,
                                        Status = 1,
                                        LeaveRequestID = leave.ID
                                    };

                                    bool isUpdateCompoff = service.Update<CompOff>(newCompOff, compOff);

                                    if (isUpdateCompoff)
                                        service.Finalize(true);

                                    currDate = index.Date;
                                }
                            }
                        }
                    }

                }
                else if (leave.Status == 2)
                {
                    for (var compLeave = 0; compLeave <= ((leave.Leaves) - 1); compLeave++)
                    {
                        var compOff = service.First<CompOff>(x => x.Person.ID == userID && x.IsApplied == true && x.Status == 1 && x.LeaveRequestID == leave.ID);
                        var newCompOff = new CompOff
                        {
                            Status = 2
                        };
                        if (compOff != null)
                        {
                            bool isUpdateCompoff = service.Update<CompOff>(newCompOff, compOff);
                            if (isUpdateCompoff)
                                service.Finalize(true);
                        }
                        else
                        {
                            updateledger = false;
                        }
                    }
                }
            }
            if (leave.FromDate.Year < DateTime.Now.Year)
            {

                oldLedger = service.First<PersonLeaveLedger>(x => x.Person.ID == userID && x.Year == leave.FromDate.Year);
                var currentYearLedger = service.First<PersonLeaveLedger>(x => x.Person.ID == userID && x.Year == DateTime.Now.Year);
                var UpdateCurrentYearLedger = new PersonLeaveLedger();
                if (leave.Status != 2)
                    UpdateCurrentYearLedger.OpeningBalance = currentYearLedger.OpeningBalance + leave.Leaves;
                else
                    UpdateCurrentYearLedger.OpeningBalance = currentYearLedger.OpeningBalance - leave.Leaves;
                bool isUpdateCurrentLeaveUtilized = service.Update<PersonLeaveLedger>(UpdateCurrentYearLedger, currentYearLedger);
                if (isUpdateCurrentLeaveUtilized)
                    service.Finalize(true);

            }
            if (updateledger && oldLedger != null)
            {
                bool isUpdateLeaveUtilized = service.Update<PersonLeaveLedger>(newLedger, oldLedger);
                if (isUpdateLeaveUtilized)
                    service.Finalize(true);
            }
        }

        private EmployeeLeaveViewModel GetLatestModel(int userID, EmployeeLeaveViewModel model)
        {
            var newModel = service.First<PersonLeave>(a => (a.Person.ID == userID) && (a.FromDate >= model.FromDate && a.ToDate <= model.ToDate));
            return Mapper.Map<PersonLeave, EmployeeLeaveViewModel>(newModel);
        }

        private int CalculateLeavesApplied(int userId, int location, DateTime fromDate, DateTime toDate)
        {
            var holidayList = service.Top<HolidayList>(0, x => x.Location == location && (x.Date >= fromDate && x.Date <= toDate) && x.IsDeleted == false && x.HolidayYear == fromDate.Year).ToList();
            var signInSignOut = service.Top<SignInSignOut>(0, x => x.UserID == userId && x.SignInTime.Value.Month == fromDate.Month).ToList();
            int count = 0;
            Boolean isDayShift = Convert.ToBoolean(service.First<PersonEmployment>(a => a.Person.ID == userId).IsDayShift);

            var floatingHolidayStatus = service.First<FloatingHolidayStatus>(fh => fh.PersonID == userId);

            for (DateTime index = fromDate; index <= toDate; index = index.AddDays(1))
            {
                if (index.DayOfWeek != DayOfWeek.Saturday && index.DayOfWeek != DayOfWeek.Sunday)
                {
                    bool excluded = false;
                    foreach (var holiday in holidayList)
                    {
                        if (index.Date == holiday.Date)
                        {
                            if (holiday.HolidayType == 2 && (floatingHolidayStatus == null || floatingHolidayStatus.HolidayID != holiday.ID))
                            {
                                excluded = false;
                                break;
                            }
                            excluded = true;
                            break;
                        }
                    }
                    if (!excluded)
                    {
                        foreach (var signInOut in signInSignOut)
                        {
                            if (index.Date == signInOut.SignInTime.Value.Date)
                            {
                                if (signInOut.SignOutTime != null)
                                {
                                    if ((signInOut.SignInTime.Value.Date == signInOut.SignOutTime.Value.Date && !isDayShift) || (signInOut.SignInTime.Value.Date != signInOut.SignOutTime.Value.Date && isDayShift))
                                    {

                                    }
                                    else if (signInOut.DayNotation == "P")
                                    {
                                        excluded = true;
                                        count = -1;
                                        return count;
                                    }
                                }
                                else if (signInOut.SignOutTime == null && !isDayShift)
                                {

                                }
                                else if (signInOut.DayNotation == "P")
                                {
                                    excluded = true;
                                    count = -1;
                                    return count;
                                }
                            }
                        }
                        if (!excluded)
                        {
                            count++;
                        }
                    }
                }
            }
            return count;
        }

        private DateTime CalculatePaternityLeave(int location, DateTime fromDate)
        {
            var holidayList = service.Top<HolidayList>(0, x => x.Location == location && (x.Date >= fromDate) && x.IsDeleted == false).ToList();
            int count = 0;
            int workingDays = 0;
            DateTime toDate = fromDate;
            while (workingDays <= 0)
            {
                toDate = fromDate.AddDays(count + 1);
                if (toDate.DayOfWeek != DayOfWeek.Saturday && toDate.DayOfWeek != DayOfWeek.Sunday)
                {
                    bool excluded = false;
                    for (int i = 0; i < holidayList.Count; i++)
                    {
                        if (toDate.Date.CompareTo(holidayList[i].Date) == 0)
                        {
                            toDate = fromDate;
                            count++;
                            break;
                        }
                    }
                    if (!excluded)
                    {
                        workingDays++;
                    }
                }
                else
                    count++;
            }
            return toDate;
        }

        public async Task<Boolean> IsApprover(int userID, int type)
        {
            return await Task.Run(() =>
            {
                var result = false;
                var ApprovelList = service.Top<Pheonix.DBContext.Approval>(0, x => x.RequestType == type && x.ApprovalDetail.Where(t => t.ApprovalID == x.ID && t.ApproverID == userID).Count() > 0).ToList();
                if (ApprovelList.Count() > 0)
                {
                    result = true;
                }
                return result;
            });
        }

        public async Task<EnvelopeModel<EmployeeLeaveViewModel>> ApplyBirthdayLeave(int userID, EmployeeLeaveViewModel model, int location)
        {
            return await Task.Run(async () =>
            {
                var isError = false;
                var errorMessage = string.Empty;

                var leave = CreateLeaveApplication(model, userID, location);
                if (leave.Status == 1)
                    errorMessage = ValidateLeaves(userID, leave, location);

                if (errorMessage != string.Empty)
                    isError = true;
                else
                {
                    var success = SubmitLeave(leave, userID);
                    if (success)
                    {
                        service.Finalize(true);
                        Person person = null, approverPerson = null; int approverIDs;
                        if (leave.ID != 0 && leave.Status == 1)
                        {
                            person = leave.Person;
                            if (person == null)
                                person = service.First<Person>(x => x.ID == userID);
                            var approval = service.First<Pheonix.DBContext.Approval>(x => x.RequestType == 1 && x.RequestID == leave.ID && x.Status == 0);
                            if (approval != null && leave.Status == 1)
                            {
                                bool isRemoveApproval = service.Remove<Pheonix.DBContext.Approval>(approval, x => x.ID == approval.ID);
                                var approvalDetails = service.First<ApprovalDetail>(x => x.ApprovalID == approval.ID);
                                approverIDs = (int)approvalDetails.ApproverID;
                                bool isRemoveApprovalDetail = service.Remove<ApprovalDetail>(approvalDetails, x => x.ID == approvalDetails.ID);
                                if (isRemoveApproval && isRemoveApprovalDetail)
                                    service.Finalize(true);
                            }
                            approverIDs = HookApproval(person.ID, leave.ID);
                            approverPerson = service.First<Person>(x => x.ID == approverIDs);
                            string empName = person.FirstName + " " + person.LastName;
                            string approverName = approverPerson.FirstName + " " + approverPerson.LastName;
                            //_emailService.SendLeaveApproval(person.ID, empName, person.PersonEmployment.First().OrganizationEmail, approverName, approverPerson.PersonEmployment.First().OrganizationEmail, leave.FromDate.Date.ToString("MM/dd/yyyy"), leave.ToDate.Date.ToString("MM/dd/yyyy"), (int)leave.Leaves, person.Image);
                        }
                        else
                        {
                            leave = service.First<PersonLeave>(x => x.ID == leave.ID);
                            person = leave.Person;
                            var approval = service.First<Pheonix.DBContext.Approval>(x => x.RequestType == 1 && x.RequestID == leave.ID && (x.Status == 0 || x.Status == 1));
                            if (approval != null && leave.Status == 4)
                            {
                                bool isRemoveApproval = service.Remove<Pheonix.DBContext.Approval>(approval, x => x.ID == approval.ID);
                                var approvalDetails = service.First<ApprovalDetail>(x => x.ApprovalID == approval.ID);
                                approverIDs = (int)approvalDetails.ApproverID;
                                bool isRemoveApprovalDetail = service.Remove<ApprovalDetail>(approvalDetails, x => x.ID == approvalDetails.ID);
                                if (isRemoveApproval && isRemoveApprovalDetail)
                                    service.Finalize(true);
                            }
                            else
                            {
                                approverIDs = (int)service.First<ApprovalDetail>(x => x.ApprovalID == approval.ID).ApproverID;
                                await UpdateHookedApproval(person.ID, 1, leave.ID, (int)leave.Status - 1, model.StatusComment);
                            }
                            approverPerson = service.First<Person>(x => x.ID == approverIDs);
                            string empName = person.FirstName + " " + person.LastName;
                            string approverName = approverPerson.FirstName + " " + approverPerson.LastName;
                            if (leave.Status != 4)
                                _emailService.SendLeaveApprovalStatus(person.ID, empName, person.PersonEmployment.First().OrganizationEmail, approverName, approverPerson.PersonEmployment.First().OrganizationEmail, leave.FromDate.Date.ToString("MM/dd/yyyy"), leave.ToDate.Date.ToString("MM/dd/yyyy"), (int)leave.Leaves, (int)leave.Status - 1 == 1 ? "Approved" : "Rejected", person.Image, leave.LeaveType); //For: #149624435 - Change done to display leave From date, To date & No. of days
                        }
                    }
                }
                return CreateEnvelope(GetLatestModel(userID, model), isError, errorMessage);
            });
        }

        private int[] GetSelectedHolidays(int personID, int holidayYear, int location)
        {          
            List<int> returnIds = new List<int>();      
            using (PhoenixEntities context = new PhoenixEntities())
            {
                var FHList = context.GetFloatingHolidayStatusByPersonId(personID, holidayYear, location).ToList();
                foreach (var item in FHList)
                {
                    returnIds.Add(item.Value);
                }
            }
            return returnIds.ToArray(); 
        }

        public async Task<HolidaysListViewModel> GetHolidays(int year,int id)
        {
            return await Task.Run(() =>
            {
                HolidaysListViewModel result = new HolidaysListViewModel();

                // Load data for Mumbai
                var holidays = service.Top<HolidayList>(0, a => a.IsDeleted == false && a.HolidayYear == year).OrderBy(x => x.Date).ToList();
                IEnumerable<PhoenixConfig> configList = service.Top<PhoenixConfig>(0, x => x.Year == year && x.ConfigKey == "HN");
                IEnumerable<PhoenixConfig> configNote = service.Top<PhoenixConfig>(0, x => x.Year == year && x.ConfigKey == "DLN");

                var mumbaiHolidays = holidays.Where(t => t.Location == 0).ToList();
                var list = Mapper.Map<List<HolidayList>, List<HolidayListViewModel>>(mumbaiHolidays);
                result.MumbaiHolidayDate = list;

                IEnumerable<PhoenixConfig> configListforMumbai = configList.Where(x => x.Location == 0);
                var data = Mapper.Map<IEnumerable<PhoenixConfig>, List<AdminLeaveConfigModel>>(configListforMumbai);
                if (data.Count != 0)
                {
                    result.MumbaiHolidayNote = data[0].ConfigValue;
                }

                IEnumerable<PhoenixConfig> configNoteMUM = configNote.Where(x => x.Location == 0);
                var DaylightdataMUM = Mapper.Map<IEnumerable<PhoenixConfig>, List<AdminLeaveConfigModel>>(configNoteMUM);
                if (DaylightdataMUM.Count != 0)
                {
                    result.MumbaiDayLightSavingNote = DaylightdataMUM[0].ConfigValue;
                }
                result.MumbaiSelectedHolidays = GetSelectedHolidays(id, year, 0);

                // Load data for Banglore
                var bangloreHolidays = holidays.Where(t => t.Location == 1).ToList();
                list = Mapper.Map<List<HolidayList>, List<HolidayListViewModel>>(bangloreHolidays);
                result.BangloreHolidayDate = list;

                IEnumerable<PhoenixConfig> configListforBanglore = configList.Where(x => x.Location == 1);
                data = Mapper.Map<IEnumerable<PhoenixConfig>, List<AdminLeaveConfigModel>>(configListforBanglore);
                if (data.Count != 0)
                {
                    result.BangloreHolidayNote = data[0].ConfigValue;
                }

                IEnumerable<PhoenixConfig> configNoteBanglore = configNote.Where(x => x.Location == 1);
                var DaylightdataBang = Mapper.Map<IEnumerable<PhoenixConfig>, List<AdminLeaveConfigModel>>(configNoteBanglore);
                if (DaylightdataBang.Count != 0)
                {
                    result.BangloreDayLightSavingNote = DaylightdataBang[0].ConfigValue;
                }
                result.BangloreSelectedHolidays = GetSelectedHolidays(id, year, 1);

                // Load data for Udaipur
                var udaipurHolidays = holidays.Where(t => t.Location == 10).ToList();
                list = Mapper.Map<List<HolidayList>, List<HolidayListViewModel>>(udaipurHolidays);
                result.UdaipurHolidayDate = list;

                IEnumerable<PhoenixConfig> configListforUdaipur = configList.Where(x => x.Location == 10);
                data = Mapper.Map<IEnumerable<PhoenixConfig>, List<AdminLeaveConfigModel>>(configListforUdaipur);
                if (data.Count != 0)
                {
                    result.UdaipurHolidayNote = data[0].ConfigValue;
                }

                IEnumerable<PhoenixConfig> configNoteUdaipur = configNote.Where(x => x.Location == 10);
                var DaylightdataUdaipur = Mapper.Map<IEnumerable<PhoenixConfig>, List<AdminLeaveConfigModel>>(configNoteUdaipur);
                if (DaylightdataUdaipur.Count != 0)
                {
                    result.UdaipurDayLightSavingNote = DaylightdataUdaipur[0].ConfigValue;
                }
                result.UdaipurSelectedHolidays = GetSelectedHolidays(id, year, 10);

                // Load data for USA
                var usaHolidays = holidays.Where(t => t.Location == 2).ToList();
                list = Mapper.Map<List<HolidayList>, List<HolidayListViewModel>>(usaHolidays);
                result.USAHolidayDate = list;

                IEnumerable<PhoenixConfig> configListforUsa = configList.Where(x => x.Location == 2);
                data = Mapper.Map<IEnumerable<PhoenixConfig>, List<AdminLeaveConfigModel>>(configListforUsa);
                if (data.Count != 0)
                {
                    result.BangloreHolidayNote = data[0].ConfigValue;
                }

                IEnumerable<PhoenixConfig> configNoteUSA = configNote.Where(x => x.Location == 2);
                var DaylightdataUSA = Mapper.Map<IEnumerable<PhoenixConfig>, List<AdminLeaveConfigModel>>(configNoteUSA);
                if (DaylightdataUSA.Count != 0)
                {
                    result.BangloreDayLightSavingNote = DaylightdataUSA[0].ConfigValue;
                }
                result.USSelectedHolidays = GetSelectedHolidays(id, year, 2);

                // Load data for Vadodara
                var vadodaraHolidays = holidays.Where(t => t.Location == 12).ToList();
                list = Mapper.Map<List<HolidayList>, List<HolidayListViewModel>>(vadodaraHolidays);
                result.VadodaraHolidayDate = list;

                IEnumerable<PhoenixConfig> configListforVadodara = configList.Where(x => x.Location == 12);
                data = Mapper.Map<IEnumerable<PhoenixConfig>, List<AdminLeaveConfigModel>>(configListforVadodara);
                if (data.Count != 0)
                {
                    result.VadodaraHolidayNote = data[0].ConfigValue;
                }

                IEnumerable<PhoenixConfig> configNoteVadodara = configNote.Where(x => x.Location == 12);
                var DaylightdataVadodara = Mapper.Map<IEnumerable<PhoenixConfig>, List<AdminLeaveConfigModel>>(configNoteVadodara);
                if (DaylightdataVadodara.Count != 0)
                {
                    result.VadodaraDayLightSavingNote = DaylightdataVadodara[0].ConfigValue;
                }
                result.VadodaraSelectedHolidays = GetSelectedHolidays(id, year, 12);
                return result;
            });          
        }

        //To get employment status of employee
        public async Task<int> GetEmploymentStatus(int PersonId)
        {
            return await Task.Run(() =>
            {
                PhoenixEntities dbContext = new PhoenixEntities();
                var user = dbContext.PersonEmployment.Where(t => t.PersonID == PersonId).First();
                return Convert.ToInt32(user.EmploymentStatus);
            });
        }

        //To get Get Holiday Year
        public async Task<IEnumerable<int?>> GetHolidayYear()
        {
            return await Task.Run(() =>
            {
                PhoenixEntities dbContext = new PhoenixEntities();
                int year = DateTime.Now.Year;
                //var holidayYear = (from r in dbContext.HolidayList
                //                   where (r.HolidayYear >= year)
                //                   orderby r.HolidayYear
                //                   select r.HolidayYear).Distinct();

                var holidayYear = dbContext.HolidayList.Where(x => x.HolidayYear >= year).Select(x => x.HolidayYear).Distinct().OrderBy(x => x);
                return holidayYear;
            });
        }


        public Task<List<FHLeaveViewModel>> CheckFHLeaveAvailability(int personId)
        {
            PhoenixEntities dbContext = new PhoenixEntities();
            return Task.Run(() =>
            {
                DateTime today = DateTime.Now.Date;
                DateTime oneMonthAgo = today.AddMonths(+1);
                //int objConsumedLeave = 0;
                //Get UpComming FH
                List<FHLeaveViewModel> optinalLeaveDates = (from p in dbContext.HolidayList
                                                            where (p.HolidayType == 2 && p.Date <= oneMonthAgo && p.Date >= today)
                                                            select new FHLeaveViewModel
                                                            {
                                                                HolidayDate = p.Date,
                                                                HolydayAvailability = false,
                                                                HolydayName = p.Description
                                                            }).ToList();

                if (optinalLeaveDates != null)
                {
                    //Check FH is not on Saturday and Sunday
                    for (int i = 0; optinalLeaveDates.Count > i; i++)
                    {
                        DateTime? optinalLeaveDate = optinalLeaveDates[i].HolidayDate;
                        optinalLeaveDates[i].HolydayAvailability = true;
                        if ((optinalLeaveDates[i].HolidayDate.Value.DayOfWeek != DayOfWeek.Saturday) || (optinalLeaveDates[i].HolidayDate.Value.DayOfWeek != DayOfWeek.Sunday))
                        {
                            //Get Person Leave Detail of user where FH Leave status is Approved
                            List<PersonLeaveViewModel> objFHApprovedConsumedLeave = (from p in dbContext.PersonLeave
                                                                                     where (p.LeaveType == 7 && p.NotConsumed == null && p.Person.ID == personId && p.Status == 2 && p.FromDate.Year == optinalLeaveDate.Value.Year)
                                                                                     select new PersonLeaveViewModel
                                                                                     {
                                                                                         FromDate = p.FromDate,
                                                                                         LeaveType = p.LeaveType,
                                                                                         NotConsumed = p.NotConsumed,
                                                                                         Status = p.Status
                                                                                     }).ToList();

                            //Get Person Leave Detail of user where FH Leave status is Rejected
                            //List<PersonLeaveViewModel> objFHRejectedConsumedLeave = (from p in dbContext.PersonLeave
                            //                                                 where (p.LeaveType == 7 && p.NotConsumed == null && p.Person.ID == personId && p.Status == 3 && p.FromDate.Year == optinalLeaveDate.Value.Year)
                            //                                                 select new PersonLeaveViewModel
                            //                                                 {
                            //                                                     FromDate = p.FromDate,
                            //                                                     LeaveType = p.LeaveType,
                            //                                                     NotConsumed = p.NotConsumed,
                            //                                                     Status = p.Status
                            //                                                 }).ToList();

                            // Total Consumed Leave
                            //objConsumedLeave = objFHApprovedConsumedLeave.Count() + objFHRejectedConsumedLeave.Count();
                            //check user take 2 FH   
                            //As per #155245858 check user take 1 FH
                            if (objFHApprovedConsumedLeave.Count >= 1)
                            {
                                optinalLeaveDates[i].HolydayAvailability = false;
                            }
                            else
                            {
                                //Get Person Leave Detail of user where status is Pending
                                PersonLeaveViewModel objFHPendingLeave = (from p in dbContext.PersonLeave
                                                                          where (p.LeaveType == 7 && p.Status == 1 && p.Person.ID == personId && p.FromDate == optinalLeaveDate)
                                                                          select new PersonLeaveViewModel
                                                                          {
                                                                              FromDate = p.FromDate,
                                                                              LeaveType = p.LeaveType,
                                                                              NotConsumed = p.NotConsumed,
                                                                              Status = p.Status
                                                                          }).FirstOrDefault();
                                if (objFHPendingLeave != null)
                                {

                                    //check user's Pending FH
                                    if (objFHPendingLeave.FromDate.Date == optinalLeaveDates[i].HolidayDate.Value.Date)
                                    {
                                        optinalLeaveDates[i].HolydayAvailability = false;
                                    }
                                }
                                //Get Person Leave Detail of user where status is Rejected
                                PersonLeaveViewModel objFHRejectedLeave = (from p in dbContext.PersonLeave
                                                                           where (p.LeaveType == 7 && p.Status == 3 && p.Person.ID == personId && p.FromDate == optinalLeaveDate)
                                                                           select new PersonLeaveViewModel
                                                                           {
                                                                               FromDate = p.FromDate,
                                                                               LeaveType = p.LeaveType,
                                                                               NotConsumed = p.NotConsumed,
                                                                               Status = p.Status
                                                                           }).FirstOrDefault();
                                if (objFHRejectedLeave != null)
                                {
                                    //check user's Reject FH
                                    if (objFHRejectedLeave.Status == 3)
                                    {
                                        optinalLeaveDates[i].HolydayAvailability = false;
                                    }
                                }
                                //Get Person Leave Detail of user where status is Approved
                                PersonLeaveViewModel objApprovedLeave = (from p in dbContext.PersonLeave
                                                                         where (p.LeaveType == 7 && p.Status == 2 && p.Person.ID == personId && p.FromDate == optinalLeaveDate)
                                                                         select new PersonLeaveViewModel
                                                                         {
                                                                             FromDate = p.FromDate,
                                                                             LeaveType = p.LeaveType,
                                                                             NotConsumed = p.NotConsumed,
                                                                             Status = p.Status
                                                                         }).FirstOrDefault();
                                //check user's Approved FH
                                if (objApprovedLeave != null)
                                {
                                    if (objApprovedLeave.FromDate.Date == optinalLeaveDates[i].HolidayDate.Value.Date)
                                    {
                                        optinalLeaveDates[i].HolydayAvailability = false;
                                    }
                                }
                            }
                        }
                    }
                }

                return optinalLeaveDates;
            });
        }

        public async Task<bool> CheckSFHLeaveAvailability(int PersonId)
        {
            return await Task.Run(() =>
            {
                bool HolydayAvailability = false;
                DateTime today = DateTime.Now.Date;
                PhoenixEntities dbContext = new PhoenixEntities();
                var user = dbContext.PersonLeave.Where(p => (p.Person.ID == PersonId && p.LeaveType == 8 && p.FromDate.Year == today.Year && (p.Status == 1 || p.Status == 2) && (p.NotConsumed == null || p.NotConsumed == 0)));
                if (!user.Any())
                {
                    HolydayAvailability = true;
                }
                return HolydayAvailability;
            });
        }
        #endregion helpers

        //To get location specific leaves details
        public async Task<LocationSpecificLeavesViewModel> GetLocationSpecificLeaves(int PersonId)
        {
            return await Task.Run(() =>
            {
                PhoenixEntities dbContext = new PhoenixEntities();
                LocationSpecificLeavesViewModel LocationSpecificLeavesVM = new LocationSpecificLeavesViewModel();
                int location = (from l in dbContext.PersonEmployment where (l.PersonID == PersonId) select l.OfficeLocation.Value).FirstOrDefault();

                var objLocationLeaves = (from p in dbContext.LocationLeaves
                                         where (p.WorkLocationID == location)
                                         select new LocationSpecificLeavesViewModel
                                         {
                                             ID = p.ID,
                                             OfficeLocation = dbContext.WorkLocation.Where(x => x.ID == location).FirstOrDefault().LocationName,
                                             IsLeaveApplicable = p.IsLeaveApplicable,
                                             IsCasualLeaveApplicable = p.IsCLApplicable,
                                             IsSickLeaveApplicable = p.IsSLApplicable,
                                             IsSpecialFloatingHolidayApplicable = p.IsSFHApplicable,
                                             IsMaternityLeaveApplicable = p.IsMLApplicable,
                                             IsPaternityLeaveApplicable = p.IsPLApplicable
                                         }).FirstOrDefault();

                return objLocationLeaves;
            });
        }

        public async Task<string> ImportLeavesdata(string fileName, string leaveType)
        {
            return await Task.Run(() =>
            {
                if (fileName.EndsWith(".csv"))
                {
                    bool AreLeavesCredited = false;
                    FileInfo fi = new FileInfo(fileName);
                    string uploadFolder = ConfigurationManager.AppSettings["UploadFolder"].ToString();
                    string whole_file = uploadFolder + @"\leaves\" + fi.Name;
                    DataTable dt = GetDataTableFromCSVFile(whole_file);

                    if (dt.Rows.Count <= 0 || !dt.Columns.Contains("EmployeeID") || !dt.Columns.Contains("Narration") || !dt.Columns.Contains("LeaveCredit"))
                        return "false";

                    DataRow[] filteredDataRows = dt.Select("LeaveCredit > 0 AND EmployeeID is not null");

                    if (filteredDataRows.Count() <= 0) return "false";

                    if (leaveType.Trim().ToUpper().Equals("PRL"))
                    {
                        foreach (DataRow row in filteredDataRows)
                        {
                            LeavesCreditViewModel objLeavesCreditViewModel = GenerateLeaveCredit(row);
                            PersonLeaveCredit objPLCredit = Mapper.Map<LeavesCreditViewModel, PersonLeaveCredit>(objLeavesCreditViewModel);
                            AreLeavesCredited = service.Create<PersonLeaveCredit>(objPLCredit, null);
                        } //end For Loop Here
                    }
                    else if (leaveType.Trim().ToUpper().Equals("CL"))
                    {
                        foreach (DataRow row in filteredDataRows)
                        {
                            LeavesCreditViewModel objLeavesCreditViewModel = GenerateLeaveCredit(row);
                            PersonCLCredit objCLCredit = Mapper.Map<LeavesCreditViewModel, PersonCLCredit>(objLeavesCreditViewModel);
                            AreLeavesCredited = service.Create<PersonCLCredit>(objCLCredit, null);
                        } //end For Loop Here

                    }
                    else if (leaveType.Trim().ToUpper().Equals("SL"))
                    {
                        try
                        {
                            foreach (DataRow row in filteredDataRows)
                            {
                                LeavesCreditViewModel objLeavesCreditViewModel = GenerateLeaveCredit(row);
                                PersonSLCredit objSLCredit = Mapper.Map<LeavesCreditViewModel, PersonSLCredit>(objLeavesCreditViewModel);
                                AreLeavesCredited = service.Create<PersonSLCredit>(objSLCredit, null);
                            } //end For Loop Here
                        }
                        catch (Exception ex)
                        {
                            return ex.Message;
                        }
                    }

                    if (AreLeavesCredited)
                        service.Finalize(true);

                    FileInfo fi1 = new FileInfo(whole_file);
                    if (fi1.Exists)
                    {
                        fi1.Delete();
                    }
                    return "true";
                }
                else
                {
                    return "false";
                }
            });
        }

        private LeavesCreditViewModel GenerateLeaveCredit(DataRow row)
        {
            LeavesCreditViewModel objLeavesCreditVM = new LeavesCreditViewModel();
            objLeavesCreditVM.PersonID = Convert.ToInt32(row["EmployeeID"]);
            objLeavesCreditVM.Narration = Convert.ToString(row["Narration"]);
            objLeavesCreditVM.CreditBalance = Convert.ToInt32(row["LeaveCredit"]);
            objLeavesCreditVM.CreditedBy = Convert.ToInt32(ConfigurationManager.AppSettings["AdminID"]);
            objLeavesCreditVM.Year = DateTime.Now.Year;
            objLeavesCreditVM.DateEffective = DateTime.Now;
            return objLeavesCreditVM;
        }

        private static DataTable GetDataTableFromCSVFile(string csv_file_path)
        {
            DataTable csvData = new DataTable();
            try
            {
                using (TextFieldParser csvReader = new TextFieldParser(csv_file_path))
                {
                    csvReader.SetDelimiters(new string[] { "," });
                    csvReader.HasFieldsEnclosedInQuotes = true;
                    string[] colFields = csvReader.ReadFields();

                    foreach (string column in colFields)
                    {
                        DataColumn datecolumn = new DataColumn(column);
                        datecolumn.AllowDBNull = true;
                        csvData.Columns.Add(datecolumn);
                    }

                    while (!csvReader.EndOfData)
                    {
                        string[] fieldData = csvReader.ReadFields();
                        //Making empty value as null
                        for (int i = 0; i < fieldData.Length; i++)
                        {
                            if (fieldData[i] == "")
                            {
                                fieldData[i] = null;
                            }
                        }
                        csvData.Rows.Add(fieldData);
                    }
                }
            }
            catch
            {
            }
            return csvData;
        }
        public async Task<bool> AddFHCheckListData(FHCheckListViewModel model)
        {
            return await Task.Run(() =>
            {
                var fHList = new FloatingHolidayStatus
                {
                    PersonID = model.PersonID,
                    HolidayID = model.HolidayID
                };
                using (PhoenixEntities context = new PhoenixEntities())
                {
                    context.FloatingHolidayStatus.Add(fHList);
                    context.SaveChanges();
                }
                return true;              
            });
        }      
    }
}