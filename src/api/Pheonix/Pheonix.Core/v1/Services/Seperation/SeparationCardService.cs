using AutoMapper;
using Pheonix.Core.Helpers;
using Pheonix.Core.Repository.SeparationCard;
using Pheonix.Core.v1.Services.Approval;
using Pheonix.Core.v1.Services.Email;
using Pheonix.Core.v1.Services.Business;
using Pheonix.DBContext;
using Pheonix.Models;
using Pheonix.Models.ViewModels;
using Pheonix.Models.VM;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using System.Net;
using System.IO;
using System.Net.Http.Headers;
using System.IO.Compression;
using Pheonix.DBContext.Repository;

namespace Pheonix.Core.v1.Services.Seperation
{
    public class SeparationCardService : ISeparationCardService
    {
        private ISeparationCardRepository _separationCardRepository;

        private int _ApprovalRequestType = 10;
        private IApprovalService _ApprovalService;
        private IBasicOperationsService _BasicService;
        private IEmailService _EmailService;

        public int UserId { get; set; }

        public SeparationCardService(ISeparationCardRepository separationCardRepository, IApprovalService approvalService, IEmailService emailService, IBasicOperationsService basicService)
        {
            _separationCardRepository = separationCardRepository;
            _ApprovalService = approvalService;
            _BasicService = basicService;
            _EmailService = emailService;
        }

        #region Resignation
        public async Task<ActionResult> Add(SeperationViewModel model)
        {
            ActionResult result = new ActionResult();

            try
            {
                SeperationViewModel spModel = model;
                model = _separationCardRepository.Add(model, 0);

                //send approval for group head.
                SendForApproval(model).Wait();

                model = Mapper.Map<SeperationViewModel, SeperationViewModel>(spModel);

                //TODO: Need to remove after testing
                var person = _BasicService.First<Person>(x => x.ID == UserId && x.Active == true);
                model.EmployeeProfile = Mapper.Map<Person, EmployeeBasicProfile>(person);

                //initiate email notification of seperation 
                //string groupHeadEmail = GetGroupHead(model.PersonID).organizationemail;

                string exitProcessManager = GetPersonEmail(model.ApprovalID);

                string sub = "Separation Process request by " + model.EmployeeProfile.FirstName + " " + model.EmployeeProfile.LastName + "(" + model.PersonID + ")";
                //SendResignationProcessEmails(model, "Resignation Applied", "Resignation applied successfully.", "");

                Task.Run(() =>
                {
                    RaiseEmailForResignation(model, sub, "Resignation applied successfully.", model.EmployeeProfile.Email, exitProcessManager, false, true, 1, model.EmployeeProfile.FirstName + " " + model.EmployeeProfile.LastName);
                });

                //TO update employment status of Employee as On Notice
                //UpdatePersonEmploymentStatus(model.PersonID, 8);

                result.isActionPerformed = true;
                result.message = string.Format("Seperation detail Added Successfully");
            }
            catch
            {
                result.isActionPerformed = false;
                result.message = string.Format("Action Failed");
            }
            return await Task.FromResult(result).ConfigureAwait(false);
        }

        //To withdraw separation
        public ActionResult Update(SeperationViewModel model)
        {
            var db1 = new PhoenixEntities();
            var resultData = db1.Separation.Where(x => x.ID == model.ID).Select(k => k.ApprovalDate).ToList();
            if (string.IsNullOrEmpty(Convert.ToString(resultData[0])) || resultData[0] == null)
            {
                model.isApprovedByHR = false;
                model.StatusID = 1;
            }
            else
                model.isApprovedByHR = true;

            model.OldEmploymentStatus = db1.Separation.FirstOrDefault(x => x.ID == model.ID).OldEmploymentStatus;

            var statusCode = _separationCardRepository.Update(model);

            //To send req. of employee withdrawal to Mgr and HR
            if (model.isApprovedByHR)
                SendForApproval(model).Wait();
            else
            {
                //TO update previous employment status of Employee
                //Commented On 06/02/2018 To get status from Separation table
                //var status = _BasicService.Top<PersonEmploymentChangeLog>(0, x => x.PersonID == model.PersonID).OrderByDescending(x => x.ActionDate).ToList();
                //int empStatus = Convert.ToInt32(status[0].EmploymentStatus);
                int empStatus = db1.Separation.FirstOrDefault(x => x.ID == model.ID).OldEmploymentStatus;
                UpdatePersonEmploymentStatus(model.PersonID, empStatus);
                Boolean isCreditback = creditBackLeaves(model);
            }

            //string groupHeadEmail = GetGroupHead(model.PersonID).organizationemail;
            //int groupHeadID = GetGroupHead(model.PersonID).ID;

            //Commented on 15/10/2017
            //TO update previous employment status of Employee
            //var personDetl = _BasicService.Top<PersonEmploymentChangeLog>(0, x => x.PersonID == model.PersonID).OrderBy(x => x.ID).Last();
            //UpdatePersonEmploymentStatus(model.PersonID, Convert.ToInt32(personDetl.EmploymentStatus));



            string roles = "";
            using (var db = new PhoenixEntities())
            {
                //TODO: Need to remove after testing
                //var grpHeadRoleID = db.PersonInRole.Where(g => g.PersonID == groupHeadID).Select(i => i.RoleID);
                //var roleIds = db.SeperationProcess.Where(x => x.SeperationID == model.ID).Select(k => k.RoleID).Distinct().ToList();
                //roleIds.Add(Convert.ToInt32(grpHeadRoleID));//to send withdraw mail Group Head

                var roleIds = db.SeperationProcess.Where(t => t.SeperationID == model.ID).Select(k => k.RoleID).ToList();
                if (roleIds.Count != 0)
                    roles = string.Join(",", roleIds.ToArray());

                //SendResignationProcessEmails(model, "Resignation Withdraw", "Resignation withdrawn successfully.", roles);

                //TODO: Need to remove after testing
                //else

                string sub = "Separation withdrawal request by " + model.EmployeeProfile.FirstName + " " + model.EmployeeProfile.LastName + " (" + model.PersonID + ")";

                string exitProcessManager = GetPersonEmail(model.ApprovalID);
                RaiseEmailForResignation(model, sub, "Resignation withdraw successfully.", model.EmployeeProfile.Email, exitProcessManager, false, true, 4, model.EmployeeProfile.FirstName + "" + model.EmployeeProfile.LastName);
            }
            return statusCode;
        }

        public IEnumerable<SeperationViewModel> GetEmpSeperationDetl(int empId)
        {
            var statusCode = _separationCardRepository.GetEmpSeperationDetl(empId);
            return statusCode;
        }

        public int GetNoticePeriod(int id)
        {
            var statusCode = _separationCardRepository.GetNoticePeriod(id);
            return statusCode;
        }

        //To update Employment status
        public void UpdatePersonEmploymentStatus(int PersonId, int CurrStatus)
        {
            var personDetl = _BasicService.All<PersonEmployment>().Where(x => x.Person.ID == PersonId).First();
            //PhoenixEntities dbContext = new PhoenixEntities();
            //var personDetl = dbContext.PersonEmployment.Where(t => t.PersonID == PersonId).First();
            var newpersonDetl = new PersonEmployment
            {
                EmploymentStatus = CurrStatus,
                SeparationRequestDate = null
            };
            bool isUpdateStatus = _BasicService.Update<PersonEmployment>(newpersonDetl, personDetl);

            if (isUpdateStatus)
                _BasicService.Finalize(true);
        }
        #endregion

        //Resignation approval request will send to Reporting Manager
        async Task SendForApproval(SeperationViewModel model)
        {
            //var approver = GetGroupHead(model.PersonID);

            //if (string.IsNullOrEmpty(Convert.ToString(model.ApprovalDate)))
            if (model.ApprovalDate == default(DateTime))
            {
                model.isApprovedByHR = false;
                model.StatusID = 1;
            }
            else
                model.isApprovedByHR = true;


            PhoenixEntities context = new PhoenixEntities();
            Boolean isHRResignation = false;

            var _personRoleID = context.PersonInRole.Where(t => t.PersonID == model.PersonID).ToList();// .FirstOrDefault(t => t.PersonID == model.PersonID).RoleID;
            int[] hrRoles = new int[3];
            hrRoles[0] = 12;
            hrRoles[1] = 24;
            hrRoles[2] = 38;

            if (_personRoleID.Count > 0)
            {
                if (hrRoles.Contains(_personRoleID[0].RoleID))
                    isHRResignation = true;
            }

            var strategy = ApprovalStrategyFactory.GetStrategy(ApprovalStrategy.OneLevelOnly, model.PersonID, model.ApprovalID);

            strategy.opsService = this._BasicService;
            int[] approvalList = new int[2];
            int[] approvalListNew = new int[1];

            //To add HR for Resignation Approval

            int hrRole = Convert.ToInt32(ConfigurationManager.AppSettings["HRRoleId"]);
            var _hrID = context.PersonInRole.FirstOrDefault(t => t.RoleID == hrRole);

            approvalList[0] = _hrID.PersonID;
            approvalList[1] = model.ApprovalID;//approver.ID;

            ApprovalService service = new ApprovalService(this._BasicService);

            //To send approval request to HR as well as Exit Process Manager simultaneously --26/09/2017
            //service.SendForApproval(UserId, _ApprovalRequestType, model.ID, approvalList);
            if (model.isApprovedByHR || isHRResignation)//to send req. for Separation Req.       
            {
                approvalListNew[0] = model.ApprovalID;

                //Note: If employee withdraw resignation after HR acceptance and before Exit Process manager approval then in such case, pending separation request of Manager's will be mark as bydefault Accepted and commented as 'FREEZ' temporary.
                var approvalPendingReq = context.Approval.Where(t => t.RequestType == 10 && t.Status == 0 && t.RequestID == model.ID).ToList();
                if (approvalPendingReq.Count > 0)
                {
                    int _id = approvalPendingReq[0].ID;
                    var _approvalPendingReq = context.ApprovalDetail.Where(t => t.ApprovalID == _id && t.Status == 0).ToList();
                    approvalPendingReq[0].Status = 1;
                    approvalPendingReq[0].StatusDate = DateTime.Now;
                    _approvalPendingReq[0].Status = 1;
                    _approvalPendingReq[0].StatusComment = "FREEZ";
                    _approvalPendingReq[0].ApprovalDate = DateTime.Now;
                    context.SaveChanges();
                }

                service.SendForApproval(UserId, _ApprovalRequestType, model.ID, approvalListNew);
            }
            else
            {
                foreach (var item in approvalList)
                {
                    approvalListNew[0] = item;
                    service.SendForApproval(UserId, _ApprovalRequestType, model.ID, approvalListNew);
                }
            }
        }

        //TODO: Need to remove after testing
        void RaiseEmailForResignation(SeperationViewModel model, string subject, string body, string emailFrom, string emailTo, bool isHR, bool isMgr, int CurrStatus, string LogInUser)//string grpHeadEmail
        {
            _EmailService.SendResignationEmail(model, subject, body, emailFrom, emailTo, isHR, isMgr, CurrStatus, "", LogInUser);
        }

        GetGroupHeadEmail_Result GetGroupHead(int personId)
        {
            var manager = QueryHelper.GetGroupHeadEmail(personId, 6);
            return manager;
        }

        int GetApprovalBasedOnRole(int roleId)
        {
            using (var db = new PhoenixEntities())
            {
                if (roleId != 0)
                {
                    var personInRole = db.PersonInRole.FirstOrDefault(t => t.RoleID == roleId);
                    if (personInRole != null)
                        return personInRole.PersonID;
                }
            }
            return 0;
        }

        #region Manager Approval
        public ActionResult Approve(SeperationViewModel model, Boolean IsHR)
        {
            ActionResult result = new ActionResult();
            try
            {
                Boolean isHRResignation = false;
                _separationCardRepository.Approve(model.ID, model.ApprovalDate, UserId, model.PersonID, IsHR, model.ExitDateRemark);

                using (var db = new PhoenixEntities())
                {
                    var _personRoleID = db.PersonInRole.Where(t => t.PersonID == model.PersonID).ToList();// .FirstOrDefault(t => t.PersonID == model.PersonID).RoleID;
                    int[] hrRoles = new int[3];
                    hrRoles[0] = 12;
                    hrRoles[1] = 24;
                    hrRoles[2] = 38;

                    if (_personRoleID.Count > 0)
                    {
                        if (hrRoles.Contains(_personRoleID[0].RoleID))
                            isHRResignation = true;
                    }
                }

                if (!IsHR) //To restrict HR to update Separation table
                {
                    model.StatusID = 2;
                    model.UpdatedBy = UserId;
                    model.UpdatedOn = DateTime.Now;
                }

                // Remove Approval Record from Group Head.
                //UpdateApproval(model.PersonID, model.ID, "Approved", 1);
                UpdateResignationApproval(model.PersonID, model.ID, "Approved", 1);

                string logInUserEmailID = GetPersonEmail(UserId);
                string EPMEmailID = GetPersonEmail(model.ApprovalID);
                Boolean isMgr = false;

                int CurrStatus = 2;

                if (!IsHR) //To check whether it is accepted by HR or Exit Process Manager
                {
                    isMgr = true;
                    CurrStatus = 3;
                }

                string resignPersonEmail = GetPersonEmail(model.PersonID);
                string logInUserName = GetLogInUser(UserId);
                string sub = "Separation Process request approved by " + logInUserName + " for " + model.EmployeeProfile.FirstName + " " + model.EmployeeProfile.LastName + " (" + model.PersonID + ")";

                _EmailService.SendResignationEmail(model, sub, "Resignation accepted successfully", logInUserEmailID, resignPersonEmail, IsHR, isMgr, CurrStatus, "", logInUserName);

                if (!IsHR)
                    _EmailService.SendResignationEmail(model, "Separation Guidelines for " + model.EmployeeProfile.FirstName + " " + model.EmployeeProfile.LastName + " (" + model.PersonID + ")", "ToDo instruction list", logInUserEmailID, resignPersonEmail, IsHR, isMgr, 10, "", "HR Team");
                else//As per discussed with Amit : this mail triggered to EPM when HR accept employee resignation
                {
                    if (!isHRResignation) //For #155281112: This mail get triggered if resigned employee is not from HR Team
                        _EmailService.SendResignationEmail(model, "Separation Process request approved by " + logInUserName + " for " + model.EmployeeProfile.FirstName + " " + model.EmployeeProfile.LastName + " (" + model.PersonID + " )", "Intimation for EPM to take action", logInUserEmailID, EPMEmailID, IsHR, isMgr, 27, "", logInUserName);

                    if (isHRResignation)//For #155281112: This mail get triggered if resigned employee is from HR Team
                        _EmailService.SendResignationEmail(model, "Separation Guidelines for " + model.EmployeeProfile.FirstName + " " + model.EmployeeProfile.LastName + " (" + model.PersonID + ")", "ToDo instruction list", logInUserEmailID, resignPersonEmail, IsHR, isMgr, 10, "", "HR Team");
                }
                //_seperationCardService.SendResignationProcessEmails(model, "Resignation Accepted", "Resignation accepted successfully.", "");
                result.isActionPerformed = true;
                result.message = string.Format("Separation approved successfully");
            }
            catch (Exception ex)
            {
                result.isActionPerformed = true;
                result.message = string.Format("Action Failed");
                throw new HttpException(500, ex.ToString());
            }
            return result;
        }

        //Resignation rejected by Reporting Manager
        public ActionResult Reject(SeperationViewModel model)
        {
            ActionResult result = new ActionResult();
            try
            {
                _separationCardRepository.Reject(model.ID, UserId, model.PersonID);

                model.StatusID = 5;
                model.UpdatedBy = UserId;
                model.UpdatedOn = DateTime.Now;
                // Remove Approval Record from Group Head.
                UpdateApproval(model.PersonID, model.ID, model.RejectRemark, 2); //"Resignation Rejected"

                //string groupHeadEmail = GetManager(model.PersonID).organizationemail;
                //TODO: Need to remove after testing
                string exitProcessManager = GetPersonEmail(model.ApprovalID);
                string resignPersonEmail = GetPersonEmail(model.PersonID);
                _EmailService.SendResignationEmail(model, "Resignation Rejected", "Resignation rejected successfully.", exitProcessManager, resignPersonEmail, false, true, 6, "", "");

                //_seperationCardService.SendResignationProcessEmails(model, "Resignation Accepted", "Resignation accepted successfully.", "");
                result.isActionPerformed = true;
                result.message = string.Format("Separation rejected successfully");
            }
            catch (Exception ex)
            {
                result.isActionPerformed = true;
                result.message = string.Format("Action Failed");
                throw new HttpException(500, ex.ToString());
            }
            return result;
        }

        public IEnumerable<SeperationViewModel> GetSeperationList(Boolean isHR)
        {
            var data = _separationCardRepository.GetSeperationList(UserId, isHR);
            return data;
        }
        #endregion

        #region ApprovalProcess
        //async void UpdateApproval(SeperationViewModel model, string statusComment, int statusID)
        async void UpdateApproval(int PersonID, int separationID, string statusComment, int statusID)
        {
            ApprovalService service = new ApprovalService(this._BasicService);
            var data = await Task.Run(() => service.UpdateMultiLevelApproval(PersonID, _ApprovalRequestType, separationID, statusID, statusComment, UserId));
        }

        async void UpdateResignationApproval(int PersonID, int separationID, string statusComment, int statusID)
        {
            ApprovalService service = new ApprovalService(this._BasicService);
            var data = await Task.Run(() => service.UpdateApprovalForSeparation(PersonID, _ApprovalRequestType, separationID, statusID, statusComment, UserId));
        }
        #endregion

        //TODO: Need to remove after testing
        GetGroupHeadEmail_Result GetManager(int personId)
        {
            var manager = QueryHelper.GetGroupHeadEmail( personId, 6);
            return manager;
        }

        #region Seperation Job
        public async Task<bool> InitiateSeparationProcess(int SeparationID, string discussionDt)
        {
            DateTime dateToCheck = DateTime.Today.AddDays(GetNoOfDaysToCheck());

            var data = await _separationCardRepository.GetSeperations();

            //To initiate job only particular for particular employee in case of HR Separation
            if (SeparationID != 0)
                data = data.Where(t => t.ID == SeparationID).ToList();

            //var data = _separationCardRepository.GetSeperations();

            //var pendingSeparations = data.Where(t => t.StatusID == 2 && t.ActualDate <= dateToCheck).ToList();
            if (SeparationID == 0)
            {
                var pendingSeparations = data.Where(t => t.StatusID == 2 && t.ApprovalDate <= dateToCheck).ToList();
                foreach (var separation in pendingSeparations)
                {
                    await AddCheckList(separation);
                }
            }
            else//For HR Termination
            {
                foreach (var separation in data)
                {
                    separation.ResignationWOSettlement = discussionDt;
                    separation.NoticePeriod = GetNoticePeriod(separation.PersonID);
                    await AddCheckList(separation);
                }
            }
            return true;
        }

        async Task<bool> AddCheckList(SeperationViewModel model, int isJobInitiated = 0)
        {
            PhoenixEntities context = new PhoenixEntities();
            //SeparationCardRepository _SeparationCardRepository = new SeparationCardRepository(this._BasicService, this._EmailService, null, null, this._ApprovalService);
            var str = await _separationCardRepository.AddSeperationCheckList(model, this._BasicService, this._EmailService); //Need to test on 18/01/2018
            //var str = await _separationCardRepository.AddSeperationCheckList(model);
            return true;
        }


        public List<SeperationTerminationViewModel> InitiateSeparationProcessForHR(int SeparationID, string discussionDt)
        {

            List<SeperationTerminationViewModel> lstResult = new List<SeperationTerminationViewModel>();
            DateTime dateToCheck = DateTime.Today.AddDays(GetNoOfDaysToCheck());
            SeperationViewModel separation = _separationCardRepository.GetSeperationsById(SeparationID);
            separation.ResignationWOSettlement = discussionDt;
            separation.NoticePeriod = GetNoticePeriod(separation.PersonID);
            lstResult = AddCheckListForHRTermination(separation);
            return lstResult;
        }

        public List<SeperationTerminationViewModel> AddCheckListForHRTermination(SeperationViewModel model, int isJobInitiated = 0)
        {
            List<SeperationTerminationViewModel> lstResult = new List<SeperationTerminationViewModel>();
            lstResult = _separationCardRepository.AddCheckListForHRTermination(model, this._BasicService, this._EmailService);
            return lstResult;
        }
        /// <summary>
        /// Get no of days Separation release process should starts.
        /// </summary>
        /// <returns></returns>
        int GetNoOfDaysToCheck()
        {
            return Convert.ToInt32(ConfigurationManager.AppSettings["SeparationReleaseIntDays"].ToString());
        }
        #endregion

        #region Separation Process
        public async Task<SeperationProcessDetailsViewModel> GetSeperationProcessDetails(string roles, int personID, int isHistory, int? year, int? separationMode, bool isSelfView = false)
        {
            return await _separationCardRepository.GetSeperationProcessDetails(roles, personID, isHistory, year, separationMode);
        }

        public ActionResult CompleteSeperationProcess(SeperationConfigProcessViewModel model, int userID)
        {
            var data = Newtonsoft.Json.JsonConvert.SerializeObject(model.Data);
            var result = _separationCardRepository.CompleteSeperationProcess(model, userID);
            return result;
        }

        public async Task<SeperationProcessDetailsViewModel> GetSeperationProcessDetailsForDeptAdmin(string roles, int personID, bool isSelfView = false)
        {
            return await _separationCardRepository.GetSeperationProcessDetailsForDeptAdmin(roles, personID);
        }
        #endregion

        #region Printing
        public async Task<HttpResponseMessage> GenerateDocument(int letterType, int separationId, int UserId, string fileType = "")
        {
            var result = await _separationCardRepository.GenerateDocument(letterType, separationId, UserId, fileType);
            //UpdateApproval(model.PersonID, model.ID, "Approved", 1);
            return result;
        }
        #endregion

        public string GetPersonEmail(int personID)
        {
            PhoenixEntities entites = new PhoenixEntities();
            Person personData = entites.People.Where(x => x.ID == personID).FirstOrDefault();
            string email = personData.PersonEmployment.First().OrganizationEmail;
            return email;
        }

        #region Employee Termination

        public List<SeperationTerminationViewModel> TerminateEmployee(SeperationViewModel model, int UserId)
        {
            List<SeperationTerminationViewModel> lstResult = new List<SeperationTerminationViewModel>();

            try
            {
                var db = new PhoenixEntities();
                Boolean isTerminate = false;
                model.IsTermination = 1;
                DateTime oldResignDate = DateTime.Now;
                DateTime oldExpectedDate = DateTime.Now;
                Boolean isResigned = false;

                string discussionDt = model.ResignationWOSettlement;

                if (model.EmailID != "")
                {
                    Boolean isUpdateStatus = false;
                    string personalEmail = "";

                    var personDetl = _BasicService.All<PersonPersonal>().Where(x => x.PersonID == model.PersonID).First();

                    // if (personDetl.PersonalEmail == "" || personDetl.PersonalEmail == null)
                    personalEmail = model.EmailID;
                    //else if (personDetl.PersonalEmail.ToString().ToLower() != model.EmailID.ToString().ToLower())
                    //    personalEmail = personDetl.PersonalEmail + ";" + model.EmailID;

                    var newpersonDetl1 = new PersonPersonal
                    {
                        PersonalEmail = personalEmail
                    };

                    isUpdateStatus = _BasicService.Update<PersonPersonal>(newpersonDetl1, personDetl);

                    if (isUpdateStatus)
                        _BasicService.Finalize(true);
                }
                //To fetch checklist as per Separation type i.e. 0 =Employee separation & 1 = Employee termination
                if (model.IsTermination == 1)
                    isTerminate = true;

                var roleIds = db.SeperationConfig.Where(t => t.IsActive && t.ChecklistType == isTerminate).Select(k => k.RoleID).Distinct().ToList();

                var separationExist = db.Separation.Where(x => x.PersonID == model.PersonID && (x.StatusID != 1 && x.StatusID != 5)).ToList();

                if (separationExist.Count > 0)
                {
                    int separationID = separationExist[0].ID;
                    var separationProcessExit = db.SeperationProcess.Where(t => t.SeperationID == separationID).ToList();
                    separationExist[0].StatusID = 5;
                    oldResignDate = separationExist[0].ResignDate; //To get employee Resign Date 
                    oldExpectedDate = separationExist[0].ExpectedDate;
                    isResigned = true;

                    if (separationProcessExit.Count > 0)//In case of Separation process initiated
                    {
                        foreach (var _separationProcessExit in separationProcessExit)
                        {
                            int reqID = _separationProcessExit.ID;
                            _separationProcessExit.StatusID = 99;
                            var approvalPendingReq = db.Approval.Where(t => t.RequestType == 11 && (t.Status == 0 || t.Status == 1) && t.RequestID == reqID).ToList();
                            if (approvalPendingReq.Count > 0)
                            {
                                foreach (var item in approvalPendingReq)
                                {
                                    var _approvalPendingReq = db.ApprovalDetail.Where(t => t.ApprovalID == item.ID && (t.Status == 0 || t.Status == 1)).ToList();
                                    item.Status = 99;
                                    _approvalPendingReq[0].Status = 99;
                                    _approvalPendingReq[0].StatusComment = "FREEZ";
                                    db.SaveChanges();
                                }
                            }
                        }
                    }
                    else
                    {
                        var approvalPendingReq = db.Approval.Where(t => t.RequestType == 10 && (t.Status == 0) && t.RequestID == separationID).ToList();
                        if (approvalPendingReq.Count > 0)
                        {
                            foreach (var item in approvalPendingReq)
                            {
                                //int _id = approvalPendingReq[0].ID;
                                var _approvalPendingReq = db.ApprovalDetail.Where(t => t.ApprovalID == item.ID && t.Status == 0).ToList();
                                item.Status = 99;
                                _approvalPendingReq[0].Status = 99;
                                _approvalPendingReq[0].StatusComment = "FREEZ";
                                db.SaveChanges();
                            }
                        }
                    }
                }

                if (roleIds.Count > 0)
                {
                    if (model.TerminationReason != 3 || (model.TerminationReason == 3 && !isResigned))
                    {
                        model.ResignDate = model.ExitDate;
                        model.ExpectedDate = DateTime.Now;
                    }
                    else if (isResigned) //To get actual Resign date of Employee in case of 'Resignation Without Settlement' scenario
                    {
                        model.ResignDate = oldResignDate;
                        model.ExpectedDate = oldExpectedDate;
                    }

                    model.ApprovalDate = model.ExitDate;
                    model.StatusID = 2;
                    model.ApprovalID = Convert.ToInt32(_BasicService.First<PersonEmployment>(x => x.PersonID == model.PersonID).ExitProcessManager);
                    //if (model.TerminationReason != "4")
                    //    spModel.StatusID = 2; //termination but process get initiated
                    //else
                    //    spModel.StatusID = 4; //termination without separation process

                    model = _separationCardRepository.Add(model, UserId, true);

                    //send approval for group head.
                    //SendForApproval(model);

                    //To update employee employment status based on Termination Type
                    int employmentStatus = GetEmploymentStatus(model.TerminationReason.ToString());
                    UpdatePersonEmploymentStatus(model.PersonID, employmentStatus);
                    //To initiate separation processs will get initiated if employee not on PIP 
                    //if (model.TerminationReason != "4")
                    model.ResignationWOSettlement = discussionDt;

                    lstResult = InitiateSeparationProcessForHR(model.ID, model.ResignationWOSettlement);

                }
                else
                {
                    lstResult[0].isActionPerformed = false;
                    lstResult[0].message = string.Format("Please configure separation checklist before submitting the request");
                }
            }
            catch(Exception ex)
            {
                lstResult[0].isActionPerformed = false;
                lstResult[0].message = string.Format("Action Failed");
            }
            return lstResult;
        }

        public bool SendSCNotice(List<SeperationTerminationViewModel> model)
        {
            return _separationCardRepository.SendSCNotice(model);
        }

        //To get the employment status if termination done by HR
        public int GetEmploymentStatus(string type)
        {
            //Type1 = Separation
            //Type2 = Absconding immediately after joining
            //Type3 = Resignation without settlement
            //Type4 = Absconding
            //Type5 = PIP Failure

            switch (type)
            {
                case "1":
                    return 14; //Resigned
                case "2":
                    return 6; //Imm. Absconding
                case "3":
                    return 14; //Resigned
                case "4":
                    return 6; //Absconding
                case "5":
                    return 14; //Terminated
                default:
                    return 14; //Terminated
            }
        }

        //Need to remove this code: SA on 25/10/2017
        public ActionResult EmployeeInactive(EmployeeTerminationViewModel model)
        {
            ActionResult result = new ActionResult();

            try
            {
                //var data = _separationCardRepository.EmployeeInactive(model.PersonID);

                //RaiseEmailForResignation(model, "Resignation Applied", "Resignation applied successfully.", model.EmployeeProfile.Email, exitProcessManager, false, true);

                //To update final separation process of HR role.               
                using (var db1 = new PhoenixEntities())
                {
                    var separationData = db1.Separation.Where(x => x.PersonID == model.PersonID && x.StatusID == 3).ToList();
                    int sepID = separationData[0].ID;
                    var temp = db1.SeperationProcess.Where(i => i.SeperationID == sepID && i.RoleID == 12).ToList();
                    _ApprovalRequestType = 11;
                    separationData[0].StatusID = 4;
                    db1.SaveChanges();
                    UpdateApproval(model.PersonID, temp[0].ID, "Approved", 1);
                }
                result.isActionPerformed = true;
                result.message = string.Format("Employee Inactivated Successfully");
            }
            catch
            {
                result.isActionPerformed = false;
                result.message = string.Format("Action Failed");
            }
            return result;
        }
        #endregion

        #region Withdrawal Process
        public ActionResult WithdrawalApprove(SeperationViewModel model)
        {
            ActionResult result = new ActionResult();
            try
            {
                _separationCardRepository.WithdrawalApprove(model);

                //TO update previous employment status of Employee
                //Commented on 06/02/2018 To get Old employement status from Separation table
                //var status = _BasicService.Top<PersonEmploymentChangeLog>(0, x => x.PersonID == model.PersonID).OrderByDescending(x => x.ActionDate).ToList();
                //int empStatus = Convert.ToInt32(status[0].EmploymentStatus);
                using (var db = new PhoenixEntities())
                {
                    //Added on 06/02/2018
                    int empStatus = db.Separation.FirstOrDefault(x => x.ID == model.ID).OldEmploymentStatus;
                    UpdatePersonEmploymentStatus(model.PersonID, empStatus);

                    string exitProcessManager = GetPersonEmail(UserId);
                    string resignPersonEmail = GetPersonEmail(model.PersonID);

                    string logInUserName = GetLogInUser(UserId);
                    string sub = "Withdraw Request accepted by " + logInUserName;

                    //if (!model.isApprovedByHR)

                    var resultData = db.SeperationProcess.Where(x => x.SeperationID == model.ID).ToList();
                    if (resultData.Count == 0)
                        _EmailService.SendResignationEmail(model, sub, "Resignation Withdraw request accepted.", exitProcessManager, resignPersonEmail, false, true, 5, "", logInUserName);

                    //Note: If employee withdraw resignation after HR acceptance and before Exit Process manager approval then in such case, pending separation request of Manager's will be mark as bydefault Rejected 

                    /*29/11/2017 :Commented below code: If EPM not approved the resignation req. but Approved the Withdraw req. in this case FREEZ commented req. should be remain as it is*/
                    //var approvalPendingReq = db.Approval.Where(t => t.RequestType == 10 && t.Status == 1 && t.RequestID == model.ID).ToList();

                    //if (approvalPendingReq.Count > 0)
                    //{
                    //    foreach (var item in approvalPendingReq)
                    //    {
                    //        var _approvalPendingReq = db.ApprovalDetail.Where(t => t.StatusComment == "FREEZ" && t.Status == 1 && t.ApproverID == model.ApprovalID && t.ApprovalID == item.ID).ToList();
                    //        if (_approvalPendingReq.Count > 0)
                    //        {
                    //            _approvalPendingReq[0].Status = 1;
                    //            _approvalPendingReq[0].StatusComment = "Approved";
                    //        }
                    //    }
                    //    //context.SaveChanges();
                    //}

                    db.SaveChanges();
                }

                UpdateResignationApproval(model.PersonID, model.ID, "Approved", 1);

                Boolean isCreditback = creditBackLeaves(model);

                //_seperationCardService.SendResignationProcessEmails(model, "Resignation Accepted", "Resignation accepted successfully.", "");  
                //UpdateResignationApproval(model.PersonID, model.ID, "Approved Withdraw Req.", 1);

                result.isActionPerformed = true;
                result.message = string.Format("Separation withdrawal request accepted");
            }
            catch (Exception ex)
            {
                result.isActionPerformed = true;
                result.message = string.Format("Action Failed");
                throw new HttpException(500, ex.ToString());
            }
            return result;
        }

        public ActionResult WithdrawalReject(SeperationViewModel model)
        {
            ActionResult result = new ActionResult();
            try
            {
                using (var db = new PhoenixEntities())
                {
                    var separation = db.Separation.Where(x => x.ID == model.ID).FirstOrDefault();
                    separation.UpdatedBy = UserId;
                    separation.UpdatedOn = DateTime.Now;
                    separation.WithdrawRemark = "";
                    separation.RejectRemark = separation.RejectRemark + DateTime.Now.Date.ToShortDateString() + " : <b>" + model.RejectRemark + "</b><br/>";

                    if (separation.StatusID == 0)//To auto approved Resignation Req. on behalf of EPM
                        separation.StatusID = 2;

                    //Note: If employee withdraw resignation after HR acceptance and before Exit Process manager approval then in such case, pending separation request of Manager's will be mark as bydefault Accepted.
                    var approvalPendingReq = db.Approval.Where(t => t.RequestType == 10 && t.Status == 1 && t.RequestID == model.ID).ToList();
                    if (approvalPendingReq.Count > 0)
                    {
                        foreach (var item in approvalPendingReq)
                        {
                            var _approvalPendingReq = db.ApprovalDetail.Where(t => t.StatusComment == "FREEZ" && t.Status == 1 && t.ApproverID == model.ApprovalID && t.ApprovalID == item.ID).ToList();
                            if (_approvalPendingReq.Count > 0)
                            {
                                _approvalPendingReq[0].Status = 1;
                                _approvalPendingReq[0].StatusComment = "Approved";
                            }
                        }
                        //context.SaveChanges();
                    }
                    db.SaveChanges();
                }

                model.UpdatedBy = UserId;
                model.UpdatedOn = DateTime.Now;
                // Remove Approval Record from Group Head.
                //UpdateApproval(model.PersonID, model.ID, model.RejectRemark, 2); //"Resignation Rejected"
                UpdateResignationApproval(model.PersonID, model.ID, model.RejectRemark, 2);

                //string groupHeadEmail = GetManager(model.PersonID).organizationemail;
                //TODO: Need to remove after testing
                string exitProcessManager = GetPersonEmail(model.ApprovalID);
                string resignPersonEmail = GetPersonEmail(model.PersonID);
                string logInUserName = GetLogInUser(model.ApprovalID);
                string sub = "Withdraw Request rejected by " + logInUserName;

                _EmailService.SendResignationEmail(model, sub, "Separation withdrawal request rejected", exitProcessManager, resignPersonEmail, false, true, 6, "", logInUserName);

                //_seperationCardService.SendResignationProcessEmails(model, "Resignation Accepted", "Resignation accepted successfully.", "");
                result.isActionPerformed = true;
                result.message = string.Format("Separation withdrawal request rejected");
            }
            catch (Exception ex)
            {
                result.isActionPerformed = true;
                result.message = string.Format("Action Failed");
                throw new HttpException(500, ex.ToString());
            }
            return result;
        }
        #endregion

        //This will credit Comp-off if On-Notice employee worked on weekdays and his resignation getting withdraw
        public bool creditBackLeaves(SeperationViewModel model)
        {
            PhoenixEntities entities = new PhoenixEntities();
            entities.CreditCompOffAfterWithdraw(model.ResignDate, model.PersonID);
            return true;
        }

        public async Task<HttpResponseMessage> GenerateTerminationDocument(int letterType, int separationId, int UserId)
        {
            var result = await _separationCardRepository.GenerateTerminationDocument(letterType, separationId, UserId);
            return result;
        }

        public string GetLogInUser(int personID)
        {
            PhoenixEntities entites = new PhoenixEntities();
            Person personData = entites.People.Where(x => x.ID == personID).FirstOrDefault();
            string LogInUser = personData.FirstName + " " + personData.LastName;
            return LogInUser;
        }

        public async Task<HttpResponseMessage> DownloadZip(int personID)
        {
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
            string uploadFolder = ConfigurationManager.AppSettings["SeparationFileFolder"].ToString();
            uploadFolder = uploadFolder + @"\" + personID + @"\" + personID + ".zip";
            string pdfFileName = personID + ".zip";

            string uploadFoldertest = uploadFolder + @"\" + personID;

            using (MemoryStream ms = new MemoryStream())
            {
                using (FileStream file = new FileStream(uploadFolder, FileMode.Open, FileAccess.Read))
                {
                    byte[] bytes = new byte[file.Length];
                    file.Read(bytes, 0, (int)file.Length);
                    ms.Write(bytes, 0, (int)file.Length);

                    HttpResponseMessage httpResponseMessage = new HttpResponseMessage();

                    httpResponseMessage.Content = new ByteArrayContent(bytes.ToArray());
                    httpResponseMessage.Content.Headers.Add("x-filename", uploadFolder);
                    httpResponseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                    httpResponseMessage.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
                    httpResponseMessage.Content.Headers.ContentDisposition.FileName = pdfFileName;
                    httpResponseMessage.StatusCode = HttpStatusCode.OK;
                    return httpResponseMessage;
                }
            }
        }

        //public ActionResult ExitDateUpdate(ChangeReleaseDateViewModel model, int userID, int isDateChange)
        //{
        //    ActionResult result = new ActionResult();
        //    try
        //    {
        //        var statusCode = _separationCardRepository.ExitDateUpdate(model, userID, isDateChange);
        //        result.isActionPerformed = true;
        //        result.message = string.Format("Exit date updated successfully");
        //    }
        //    catch
        //    {
        //        result.isActionPerformed = true;
        //        result.message = string.Format("Action Failed");
        //    }
        //    return result;
        //}
        public SeperationTerminationViewModel ExitDateUpdate(ChangeReleaseDateViewModel model, int userID, int isDateChange)
        {
            SeperationTerminationViewModel result = new SeperationTerminationViewModel();
            try
            {
                result = _separationCardRepository.ExitDateUpdate(model, userID, isDateChange);
                result.isActionPerformed = true;
                result.message = string.Format("Exit date updated successfully");

            }
            catch
            {
                result.isActionPerformed = true;
                result.message = string.Format("Action Failed");
            }
            return result;
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

        public async Task<IEnumerable<SeperationViewModel>> GetSeperationListHistory(int userId, Boolean isHR)
        {
            var data = await _separationCardRepository.GetSeperationListHistory(userId, isHR);
            return data;
        }

        public SeperationTerminationViewModel ShowCauseNotice2(EmailTemplateViewModel model, int type, int separationprocessid, int userId)
        {

            SeperationTerminationViewModel result = new SeperationTerminationViewModel();

            try
            {
                result = _separationCardRepository.ShowCauseNotice2(model, type, separationprocessid, userId);

                result.isActionPerformed = true;
                result.message = string.Format("Get Show cause notice-2 send Successfully");
            }
            catch
            {
                result.isActionPerformed = false;
                result.message = string.Format("Action Failed");
            }
            return result;
        }
        //public ActionResult ShowCauseNotice2(EmailTemplateViewModel model, int type, int separationprocessid, int userId)
        //{
        //    ActionResult result = new ActionResult();

        //    try
        //    {
        //        var data = _separationCardRepository.ShowCauseNotice2(model, type, separationprocessid, userId);

        //        result.isActionPerformed = true;
        //        result.message = string.Format("Show cause notice-2 send Successfully");
        //    }
        //    catch
        //    {
        //        result.isActionPerformed = false;
        //        result.message = string.Format("Action Failed");
        //    }
        //    return result;
        //}


        public async Task<object> SendSeparationReminderMail()
        {
            using (PhoenixEntities context = new PhoenixEntities())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    for (int count = 0; count < 3; count++)
                    {
                        int days = CheckDates(count);
                        DateTime toCheckDate = DateTime.Now.AddDays(days + 1);
                        //DateTime toCheckDate = DateTime.Now.AddDays(3);
                        var _toCheckDate = toCheckDate.Date;

                        var pendingSeparations = context.Separation.Where(x => x.StatusID == 3 && x.ApprovalDate == _toCheckDate && x.Person1.PersonEmployment.FirstOrDefault().OfficeLocation.Value == count).Distinct().OrderByDescending(x => x.ID).ToList();
                        foreach (var item in pendingSeparations)
                        {
                            var _SeparationProcess = context.SeperationProcess.Where(x => x.SeperationID == item.ID).ToList();
                            InitiateSeparationReminderEmails(context, _SeparationProcess);
                        }
                    }
                    transaction.Commit();
                }
            }

            return "Success";
        }

        public void InitiateSeparationReminderEmails(PhoenixEntities context, List<SeperationProcess> _SeparationProcess)
        {
            _EmailService.InitiateSeparationReminderEmails(context, _SeparationProcess);
        }

        public ActionResult AddExitForm(ExitProcessFormDetailViewModel model, int separationID)
        {
            ActionResult result = new ActionResult();

            try
            {
                var data = _separationCardRepository.AddExitForm(model, separationID, UserId);
                result.isActionPerformed = true;
                result.message = string.Format("Exit Form Submitted Successfully");
            }
            catch
            {
                result.isActionPerformed = false;
                result.message = string.Format("Action Failed");
            }
            return result;
        }

        public async Task<ExitProcessFormDetailViewModel> GetExitFormData(int separationID)
        {
            return await _separationCardRepository.GetExitFormData(separationID);
        }

        public int CheckDates(int location)
        {
            DateTime daytoCheck = DateTime.Now.AddDays(2);
            int count = 1;
            using (PhoenixEntities context = new PhoenixEntities())
            {

                var holidayList = context.HolidayList.Where(x => x.Location == location);

                for (DateTime index = DateTime.Now.Date; index <= daytoCheck; index = index.AddDays(1))
                {
                    foreach (var holiday in holidayList)
                    {
                        if (index.Date.CompareTo(holiday.Date) == 0)
                        {
                            count++;
                            daytoCheck = daytoCheck.AddDays(1);
                        }
                    }

                    if (index.DayOfWeek == DayOfWeek.Saturday || index.DayOfWeek == DayOfWeek.Sunday)
                    {
                        count++;
                        daytoCheck = daytoCheck.AddDays(1);
                    }
                }


            }

            return count;
        }

        public async Task<string> GetPersonalEmailID(int PersonId)
        {
            return await Task.Run(() =>
            {
                PhoenixEntities entites = new PhoenixEntities();
                var personData = entites.PersonPersonals.Where(x => x.PersonID == PersonId).FirstOrDefault();
                string email = personData.PersonalEmail;
                return email;
            });
        }

        //This will give Separation List for RMG team 
        public async Task<IEnumerable<SeperationViewModel>> GetSeperationListForRMG(string roles, int personID)
        {
            return await _separationCardRepository.GetSeperationListForRMG(roles, personID);
        }

        #region Employee Contract Conversion

        public ContractConversionVM GetContractConversionData(int PersonId)
        {
            ContractConversionVM objResult = new ContractConversionVM();
            try
            {
                using (PhoenixEntities dbContext = new PhoenixEntities())
                {
                    objResult = (from p in dbContext.People
                                 join pe in dbContext.PersonEmployment on p.ID equals pe.PersonID
                                 join pr in dbContext.PersonReporting on p.ID equals pr.PersonID
                                 where p.ID == PersonId
                                 select new ContractConversionVM
                                 {
                                     ISContractConverion = true,
                                     OldPersonID = p.ID,
                                     FirstName = p.FirstName,
                                     MiddleName = p.MiddleName,
                                     LastName = p.LastName,                                     
                                     DeliveryUnit = pe.DeliveryUnit,
                                     DesignationID = pe.DesignationID,
                                     EmployeeType = pe.EmployeeType,
                                     EmploymentStatus = pe.EmploymentStatus,
                                     ExitProcessManager = pe.ExitProcessManager,
                                     JoiningDate = pe.JoiningDate,
                                     OfficeLocation = pe.OfficeLocation,
                                     WorkLocation = pe.WorkLocation,
                                     OrganizationEmail = pe.OrganizationEmail.Remove(0,4),
                                     ProbationReviewDate = pe.ProbationReviewDate,
                                     RejoinedWithinYear = pe.RejoinedWithinYear,
                                     OldPersonExitDate = pe.ExitDate,
                                     ReportingTo = pr.ReportingTo                                     
                                 }).Single();
                }
            }
            catch (Exception ex)
            {

            }
            return objResult;
        }

        #endregion
    }
}
