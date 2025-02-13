using AutoMapper;
using Newtonsoft.Json;
using Pheonix.Core.Helpers;
using Pheonix.Core.v1.Services;
using Pheonix.Core.v1.Services.Approval;
using Pheonix.Core.v1.Services.Email;
using Pheonix.Core.v1.Services.Seperation;
using Pheonix.DBContext;
using Pheonix.DBContext.Repository;
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
using System.IO;
using System.IO.Compression;
using Pheonix.Core.v1.Services.Business;

namespace Pheonix.Core.Repository.SeparationCard
{

    public class SeparationCardRepository : ISeparationCardRepository
    {
        private PhoenixEntities _phoenixEntity;
        private IBasicOperationsService service;
        private IEmailService _EmailService;
        private IPrintSeparationReportInPDF _PrintReport;
        private IContextRepository _repo;
        delegate void ProgressDelegate(string sMessage);
        private IResourceAllocationService _ResourceAllocationService;
        private ICandidateService _CandidateService;
        private ITARRFService _RRFService;

        public SeparationCardRepository(IBasicOperationsService opsService, IEmailService emailService, IPrintSeparationReportInPDF printReport, IContextRepository repository, IApprovalService approvalService, IResourceAllocationService resourceAllocationService, ICandidateService candidateService, ITARRFService rrfService)
            : this()
        {
            _phoenixEntity = new PhoenixEntities();
            _phoenixEntity.Database.Connection.Open();
            service = opsService;
            _EmailService = emailService;
            _PrintReport = printReport;
            _repo = repository;
            _ResourceAllocationService = resourceAllocationService;
            _CandidateService = candidateService;
            _RRFService = rrfService;
        }

        public SeparationCardRepository()
        { }

        int _SeparationRequestType = 10;
        int _ReleaseRequestType = 11;
        int _HRRoleId = QueryHelper.GetConfigKeyValue<int>("HRRoleId");

        #region Resignation

        public SeperationViewModel Add(SeperationViewModel model, int UserId, Boolean isTerminate)
        {
            using (var db = _phoenixEntity)
            {
                Separation dbModel = Mapper.Map<SeperationViewModel, Separation>(model);
                var NoticePeriod = GetNoticePeriod(model.PersonID);
                dbModel.ActualDate = CheckDates(model.PersonID, NoticePeriod - 1);

                // 06/02/2018: To add Notice Period and Previous employment status 
                int empStatus = Convert.ToInt32(service.First<PersonEmployment>(x => x.PersonID == model.PersonID).EmploymentStatus);
                dbModel.OldEmploymentStatus = empStatus;
                dbModel.NoticePeriod = NoticePeriod;

                if (isTerminate)
                {
                    dbModel.ApprovalDate = model.ApprovalDate;
                    dbModel.CreatedBy = UserId;
                }
                else
                {
                    dbModel.ApprovalDate = null;
                    dbModel.CreatedBy = model.PersonID;
                }
                dbModel.CreatedOn = DateTime.Now;
                dbModel.UpdatedOn = null;

                db.Separation.Add(dbModel);
                db.SaveChanges();

                model = Mapper.Map<Separation, SeperationViewModel>(dbModel);

                //TO update employment status as On Notice if Separation apply by employee
                if (!isTerminate)
                    UpdatePersonEmploymentStatus(model.PersonID, 8);
            }
            return model;
        }

        public ActionResult Update(SeperationViewModel model)
        {
            ActionResult result = new ActionResult();
            try
            {
                using (var db = _phoenixEntity)
                {
                    Separation dbModel = db.Separation.Where(x => x.ID == model.ID && x.PersonID == model.PersonID).SingleOrDefault();
                    dbModel.UpdatedBy = model.PersonID;
                    dbModel.UpdatedOn = DateTime.Now;

                    if (!model.isApprovedByHR)
                        dbModel.ApprovalDate = null;

                    if (dbModel != null)
                    {
                        db.Entry(dbModel).CurrentValues.SetValues(Mapper.Map<SeperationViewModel, Separation>(model));
                        db.SaveChanges();
                    }

                    //To update SeperationProcess table for Withdraw process
                    SeperationProcess separationProcess = db.SeperationProcess.FirstOrDefault(x => x.SeperationID == model.ID);
                    //if (separationProcess != null)
                    //{
                    //    separationProcess.Comments = "Withdraw";
                    //    separationProcess.StatusID = 1;
                    //    separationProcess.UpdatedBy = model.PersonID;
                    //    separationProcess.UpdatedOn = DateTime.Now;

                    //    db.SaveChanges();
                    //}

                    //To update ApprovalDetail table for Withdraw process
                    //var _ReleaseRequestType = Convert.ToInt32(db.Approval.Where(x => x.RequestID == model.ID && x.RequestBy == model.PersonID).First().RequestType);
                    //var _ReleaseRequestType = db.Approval.Where(x => (separationProcess == null ? x.RequestID == model.ID : x.RequestID == separationProcess.ID) && x.RequestBy == model.PersonID).First().RequestType;
                    int _RequestType = 0;

                    if (separationProcess == null && !model.isApprovedByHR)
                        _RequestType = 10;//10;

                    //else if(model.isApprovedByHR)
                    //    _RequestType = _ReleaseRequestType;//11;

                    ApprovalService service1 = new ApprovalService(this.service);

                    //UpdateApproval(separationProcess, userId);
                    //if (separationProcess.Separation != null && separationProcess.Separation.SeperationProcess.FirstOrDefault(t => t.StatusID == 0) == null)
                    //{
                    //    separationProcess.Separation.StatusID = 4;
                    //    UpdatePersonEmploymentDetails(db, separationProcess);

                    //    db.SaveChanges();
                    //}

                    //To update SeperationProcess table for Withdraw process
                    if (_RequestType == 11)
                    {
                        var resultData = db.SeperationProcess.Where(x => x.SeperationID == model.ID).ToList();
                        List<int> approvalID = new List<int>();

                        if (resultData != null)
                        {
                            foreach (var item in resultData)
                            {
                                item.Comments = "Withdraw";
                                item.StatusID = 1;
                                item.UpdatedBy = model.PersonID;
                                item.UpdatedOn = DateTime.Now;
                                db.SaveChanges();
                                approvalID.Add(item.ID);
                            }
                        }

                        foreach (var item in approvalID)
                        {
                            var tempid = db.Approval.Where(a => a.RequestID == item && a.RequestBy == model.PersonID).First().ID;
                            int approverID = Convert.ToInt32(db.ApprovalDetail.Where(x => x.ApprovalID == tempid).First().ApproverID);
                            var data = service1.UpdateMultiLevelApproval(model.PersonID, _RequestType, item, 2, "Withdraw", approverID);
                        }

                        result.message = string.Format("Separation withdraw request send for approval");
                    }
                    else if (_RequestType == 10)
                    {
                        var tempid = db.Approval.Where(a => a.RequestID == model.ID && a.RequestBy == model.PersonID).ToList();
                        foreach (var item in tempid)
                        {
                            int approverID = Convert.ToInt32(db.ApprovalDetail.Where(x => x.ApprovalID == item.ID).First().ApproverID);
                            var data = service1.UpdateApprovalForSeparation(model.PersonID, _RequestType, model.ID, 2, "Withdraw", approverID);
                        }

                        result.message = string.Format("Separation Withdrawn Successfully");
                    }
                }

                //25-06-2017 -Need to test
                //_seperationConfigRepository.UpdatePersonEmploymentDetails(_phoenixEntity, model, model.PersonID, 3); //TODO: Need to check value of ON Notice 

                result.isActionPerformed = true;
                //result.message = string.Format("Separation detail Updated Successfully");
            }
            catch
            {
                result.isActionPerformed = true;
                result.message = string.Format("Action Failed");
            }
            return result;
        }

        public int GetNoticePeriod(int id)
        {
            int empStatus = Convert.ToInt32(service.First<PersonEmployment>(x => x.PersonID == id).EmploymentStatus);

            if (empStatus == 8 || empStatus == 14 || empStatus == 6)
            {
                var status = service.Top<PersonEmploymentChangeLog>(0, x => x.PersonID == id).OrderByDescending(x => x.ActionDate).ToList();
                empStatus = Convert.ToInt32(status[0].EmploymentStatus);
            }

            var noticePeriod = Convert.ToInt32(ConfigurationManager.AppSettings["NoticePeriod"].ToString());
            switch (empStatus)
            {
                case 1: //Status 1 = Confirm
                    noticePeriod = 60;
                    break;
                default:
                    noticePeriod = 30;//Convert.ToInt32(ConfigurationManager.AppSettings["NoticePeriod"].ToString());
                    break;
            }
            return noticePeriod;
        }

        //To show resignation detail of logged-in employee on Resignation Card
        public IEnumerable<SeperationViewModel> GetEmpSeperationDetl(int empId)
        {
            try
            {
                var seperationModel = new List<SeperationViewModel>();
                using (var _db = _phoenixEntity)
                {
                    var resultData = _db.Separation.Where(s => (s.StatusID != 1 && s.StatusID != 5) && s.PersonID == empId).ToList();
                    if (resultData != null && resultData.Count > 0)
                    {
                        seperationModel = Mapper.Map<List<Separation>, List<SeperationViewModel>>(resultData);
                        foreach (var item in seperationModel)
                        {
                            item.EmployeeProfile = Mapper.Map<Person, EmployeeBasicProfile>(resultData.Where(x => x.ID == item.ID).First().Person1);
                            //item.NoticePeriod = GetNoticePeriod(empId); //Commented on 06/02/2018 Now NP will get fetch from Separation table

                            //if (string.IsNullOrEmpty(Convert.ToString(item.ApprovalDate)))
                            if (item.ApprovalDate == default(DateTime))
                            {
                                item.ApprovalDate = item.ActualDate;
                                item.isApprovedByHR = false;
                            }
                            else
                                item.isApprovedByHR = true;

                            //if (!string.IsNullOrEmpty(Convert.ToString(item.ApprovalDate)) && item.WithdrawRemark != null && item.WithdrawRemark != "")
                            if (item.ApprovalDate != default(DateTime) && item.WithdrawRemark != null && item.WithdrawRemark != "")
                                item.isWithdraw = true;
                            else
                                item.isWithdraw = false;
                        }
                    }
                    else
                    {
                        var person = service.First<Person>(x => x.ID == empId && x.Active == true);
                        var data = new SeperationViewModel();
                        data.NoticePeriod = GetNoticePeriod(empId);
                        //data.ActualDate = DateTime.Now.AddDays(data.NoticePeriod);
                        //data.ExpectedDate = DateTime.Now.AddDays(data.NoticePeriod);
                        //data.ApprovalDate = DateTime.Now.AddDays(data.NoticePeriod);
                        data.ActualDate = CheckDates(empId, data.NoticePeriod - 1);
                        data.ExpectedDate = data.ActualDate;
                        data.ApprovalDate = data.ActualDate;
                        data.PersonID = empId;
                        data.Comments = "";
                        data.SeperationReason = "";
                        data.ResignDate = DateTime.Now;
                        //data.ExpectedDate = DateTime.Now.AddDays(data.NoticePeriod);
                        data.EmployeeProfile = Mapper.Map<Person, EmployeeBasicProfile>(person);
                        seperationModel.Add(data);
                    }
                    return seperationModel;
                }
            }
            catch (Exception ex)
            {
                throw new HttpException(500, ex.ToString());
            }
        }
        #endregion

        #region Manager Approval
        public ActionResult Approve(int id, DateTime approvalDate, int userID, int personID, Boolean IsHR, string ExitDateRemark)
        {
            ActionResult result = new ActionResult();
            using (var db = _phoenixEntity)
            {
                Boolean isHRResignation = false;
                string logInUserName = GetLogInUser(userID);

                var _personRoleID = db.PersonInRole.Where(t => t.PersonID == personID).ToList();// .FirstOrDefault(t => t.PersonID == model.PersonID).RoleID;
                int[] hrRoles = new int[3];
                hrRoles[0] = 12;
                hrRoles[1] = 24;
                hrRoles[2] = 38;

                if (_personRoleID.Count > 0)
                {
                    if (hrRoles.Contains(_personRoleID[0].RoleID))
                        isHRResignation = true;
                }

                var separation = db.Separation.Where(x => x.ID == id).FirstOrDefault();
                if (separation != null)
                {
                    if (IsHR)
                    {
                        separation.ApprovalDate = approvalDate;
                        separation.ExitDateRemark = separation.ExitDateRemark + "<b>" + logInUserName + "</b> :<br/>" + DateTime.Now.Date.ToShortDateString() + " : <b>" + ExitDateRemark + "</b><br/>";//ExitDateRemark;
                        if (isHRResignation)
                        {
                            separation.StatusID = 2;
                            separation.UpdatedBy = userID;
                            separation.UpdatedOn = DateTime.Now;
                        }
                    }
                    else
                    {
                        separation.StatusID = 2;
                        separation.UpdatedBy = userID;
                        separation.UpdatedOn = DateTime.Now;
                        //separation.ApprovalDate = approvalDate;
                        //separation.ActualDate = approvalDate;
                    }
                    db.SaveChanges();
                    result.isActionPerformed = true;
                    result.message = "Record Saved";
                }
            }
            return result;
        }

        //Resignation rejected by Reporting Manager
        public ActionResult Reject(int id, int userID, int personID)
        {
            ActionResult result = new ActionResult();
            using (var db = _phoenixEntity)
            {
                var seperation = db.Separation.Where(x => x.ID == id).FirstOrDefault();
                if (seperation != null)
                {
                    seperation.StatusID = 5;
                    seperation.UpdatedBy = userID;
                    seperation.UpdatedOn = DateTime.Now;
                    db.SaveChanges();
                    result.isActionPerformed = true;
                    result.message = "Record Saved";
                }
            }
            return result;
        }

        //To show list of Separation Details for group head approval 
        public IEnumerable<SeperationViewModel> GetSeperationList(int userId, bool isHR)
        {
            try
            {
                var seperationModellist = new List<SeperationViewModel>();
                using (var context = new PhoenixEntities())
                {
                    var separationListData = context.GetSeparationListData(userId).ToList();

                    if (separationListData != null && separationListData.Any())
                    {
                        seperationModellist = separationListData.Select(data => Mapper.Map<GetSeparationListData_Result, SeperationViewModel>(data))
                                                              .ToList();
                        return seperationModellist;
                    }
                    else
                    {
                        return Enumerable.Empty<SeperationViewModel>();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while processing the separation list.", ex);
            }
        }

        //To update Employment status
        public void UpdatePersonEmploymentStatus(int PersonId, int CurrStatus)
        {
            var personDetl = service.All<PersonEmployment>().Where(x => x.PersonID == PersonId).First();
            bool isUpdateStatus = false;

            if (CurrStatus == 8)
            {
                var newpersonDetl = new PersonEmployment
                {
                    EmploymentStatus = CurrStatus,
                    SeparationRequestDate = DateTime.Now
                };
                isUpdateStatus = service.Update<PersonEmployment>(newpersonDetl, personDetl);
            }
            else
            {
                var newpersonDetl = new PersonEmployment
                {
                    EmploymentStatus = CurrStatus
                };
                isUpdateStatus = service.Update<PersonEmployment>(newpersonDetl, personDetl);
            }

            //var newpersonDetl = new PersonEmployment
            //{
            //    EmploymentStatus = CurrStatus  
            //};
            //bool isUpdateStatus = service.Update<PersonEmployment>(newpersonDetl, personDetl);

            if (isUpdateStatus)
                service.Finalize(true);
        }

        List<int> GetSeperationApprovals(PhoenixEntities context, int userId, int requestType)
        {
            // var approvalList = QueryHelper.GetApprovalsForUser2(context,userId, requestType);
            var approvalList = QueryHelper.GetApprovalsForUser2(userId, requestType);
            return approvalList;
        }

        List<int> GetApprovalsForExecutiveRole(int userId, int requestType, int logedInUserId)
        {
            var approvalList = QueryHelper.GetApprovalsForExecutiveRole(userId, requestType, logedInUserId);
            return approvalList;
        }

        #endregion

        #region Approval Process
        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        async Task SendForApproval(SeperationProcess model, IBasicOperationsService basicOpsService = null)//,IBasicOperationsService basicOpsService=null //Need to test on 18/01/2018
        {
            var approvar = GetApprovalBasedOnRole(model, model.RoleID ?? 0);
            if (approvar != 0)
            {
                var strategy = ApprovalStrategyFactory.GetStrategy(ApprovalStrategy.OneLevelOnly, model.Separation.PersonID);

                strategy.opsService = basicOpsService; //Need to test on 18/01/2018
                //strategy.opsService = this.service;
                int[] approvalList = new int[1];
                approvalList[0] = approvar;

                //ApprovalService service = new ApprovalService(this.service);

                //Need to test on 18/01/2018
                ApprovalService service;
                if (basicOpsService == null)
                    service = new ApprovalService(this.service);
                else
                    service = new ApprovalService(basicOpsService);

                service.SendForApproval(model.Separation.PersonID, _ReleaseRequestType, model.ID, approvalList);
            }
        }

        int GetApprovalBasedOnRole(SeperationProcess model, int roleId)
        {
            using (var db = new PhoenixEntities())
            {
                if (roleId != 0)
                {
                    var personInRole = db.PersonInRole.FirstOrDefault(t => t.RoleID == roleId);

                    if (personInRole != null)
                        return personInRole.PersonID;
                    else
                    {
                        //If no person assign for define role its request goes to HR                         
                        var personInRole1 = db.PersonInRole.FirstOrDefault(t => t.RoleID == Convert.ToInt32(ConfigurationManager.AppSettings["HRRoleId"]));
                        return personInRole1.PersonID;
                    }
                }
                else
                {
                    if (model != null)
                    {
                        //var personInRole = db.PersonInRole.FirstOrDefault(t => t.PersonID == model.Separation.ApprovalID);
                        //return personInRole.PersonID;

                        return model.Separation.ApprovalID;
                    }
                }
                //if (personInRole != null)
                //    return personInRole.PersonID;
            }
            return 0;
        }

        void UpdateApproval(SeperationProcess model, int userId)
        {
            ApprovalService service = new ApprovalService(this.service);
            var data = service.UpdateMultiLevelApproval(model.Separation.PersonID, _ReleaseRequestType, model.ID, 1, "Approved", userId);
        }
        #endregion

        #region Separation Job
        public async Task<IEnumerable<SeperationViewModel>> GetSeperations()
        {
            try
            {
                var seperationModel = new List<SeperationViewModel>();
                //await Task.Run(() =>
                //{
                using (var _db = new PhoenixEntities())
                {
                    var resultData = _db.Separation.ToList();
                    if (resultData != null && resultData.Count > 0)
                    {
                        seperationModel = Mapper.Map<List<Separation>, List<SeperationViewModel>>(resultData);
                        foreach (var item in seperationModel)
                        {
                            item.EmployeeProfile = Mapper.Map<Person, EmployeeBasicProfile>(resultData.Where(x => x.ID == item.ID).First().Person1);
                        }
                    }
                }
                //});
                return seperationModel;
            }
            catch (Exception ex)
            {
                throw new HttpException(500, ex.ToString());
            }
        }

        public SeperationViewModel GetSeperationsById(int SeprationId)
        {
            try
            {
                var seperationModel = new SeperationViewModel();
                //await Task.Run(() =>
                //{
                using (var _db = new PhoenixEntities())
                {
                    var resultData = _db.Separation.Where(x => x.ID == SeprationId).FirstOrDefault();
                    if (resultData != null)
                    {
                        seperationModel = Mapper.Map<Separation, SeperationViewModel>(resultData);

                        seperationModel.EmployeeProfile = Mapper.Map<Person, EmployeeBasicProfile>(resultData.Person1);

                    }
                }
                //});
                return seperationModel;
            }
            catch (Exception ex)
            {
                throw new HttpException(500, ex.ToString());
            }
        }

        public async Task<ActionResult> AddSeperationCheckList(SeperationViewModel model, IBasicOperationsService basicOpsService = null, IEmailService _EmailSendingService = null)//, IBasicOperationsService basicOpsService=null //Need to test on 18/01/2018
        {
            ActionResult result = new ActionResult();
            try
            {
                using (var db = new PhoenixEntities())
                {
                    string roles = "";
                    var separation = db.Separation.FirstOrDefault(t => t.ID == model.ID);
                    separation.StatusID = 3;
                    Boolean isTerminate = false;
                    //To fetch checklist as per Separation type i.e. 0 =Employee separation & 1 = Employee termination
                    if (model.IsTermination == 1)
                        isTerminate = true;

                    var seqRoleIds = db.SeperationConfig.Where(t => t.IsActive && t.ChecklistType == isTerminate).OrderBy(x => x.Sequence).ToList();
                    var roleIds = seqRoleIds.Select(k => k.RoleID).Distinct().ToList();
                    var separationConfigs = db.SeperationConfig.Where(t => t.IsActive && t.ChecklistType == isTerminate).OrderBy(x => x.Sequence).ToList();

                    if (db.SeperationProcess.FirstOrDefault(t => t.SeperationID == model.ID) == null)
                    {
                        foreach (var roleId in roleIds)
                        {
                            if (db.SeperationProcess.FirstOrDefault(t => t.SeperationID == model.ID && t.RoleID == roleId) == null)
                            {
                                var currentRoleConfigs = separationConfigs.Where(t => t.RoleID == roleId).ToList();
                                var configs = Mapper.Map<List<SeperationConfig>, List<SeperationConfigViewModel>>(currentRoleConfigs);

                                SeperationProcess separationProcess = new SeperationProcess();
                                separationProcess.ChecklistProcessedData = JsonConvert.SerializeObject(configs);
                                separationProcess.CreatedBy = 1;
                                separationProcess.CreatedOn = DateTime.Now;
                                separationProcess.UpdatedBy = 1;
                                separationProcess.UpdatedOn = DateTime.Now;
                                separationProcess.StatusID = 0;
                                separationProcess.RoleID = roleId;
                                separationProcess.SeperationID = model.ID;
                                db.SeperationProcess.Add(separationProcess);
                                db.SaveChanges();

                                await SendForApproval(separationProcess, basicOpsService);//Need to test on 18/01/2018
                                //SendForApproval(separationProcess);
                            }
                        }
                        roles = string.Join(",", roleIds.ToArray());
                        model.StatusID = 3;
                        //TODO: Need to remove after testing

                        var str = await SendResignationProcessEmails(model, "", "", roles, 7, model.CreatedBy, _EmailSendingService);

                    }
                    result.isActionPerformed = true;
                    result.message = string.Format("Checklist Added Successfully");
                    return result;
                }
            }
            catch
            {
                result.isActionPerformed = false;
                result.message = string.Format("Action Failed");
            }

            return result;
        }

        public List<SeperationTerminationViewModel> AddCheckListForHRTermination(SeperationViewModel model, IBasicOperationsService basicOpsService = null, IEmailService _EmailSendingService = null)
        {
            List<SeperationTerminationViewModel> lstResult = new List<SeperationTerminationViewModel>();
            try
            {
                using (var db = new PhoenixEntities())
                {
                    string roles = "";
                    var separation = db.Separation.FirstOrDefault(t => t.ID == model.ID);
                    separation.StatusID = 3;
                    Boolean isTerminate = false;
                    //To fetch checklist as per Separation type i.e. 0 =Employee separation & 1 = Employee termination
                    if (model.IsTermination == 1)
                        isTerminate = true;

                    var seqRoleIds = db.SeperationConfig.Where(t => t.IsActive && t.ChecklistType == isTerminate).OrderBy(x => x.Sequence).ToList();
                    var roleIds = seqRoleIds.Select(k => k.RoleID).Distinct().ToList();
                    var separationConfigs = db.SeperationConfig.Where(t => t.IsActive && t.ChecklistType == isTerminate).OrderBy(x => x.Sequence).ToList();

                    if (db.SeperationProcess.FirstOrDefault(t => t.SeperationID == model.ID) == null)
                    {
                        foreach (var roleId in roleIds)
                        {
                            if (db.SeperationProcess.FirstOrDefault(t => t.SeperationID == model.ID && t.RoleID == roleId) == null)
                            {
                                var currentRoleConfigs = separationConfigs.Where(t => t.RoleID == roleId).ToList();
                                var configs = Mapper.Map<List<SeperationConfig>, List<SeperationConfigViewModel>>(currentRoleConfigs);

                                SeperationProcess separationProcess = new SeperationProcess();
                                separationProcess.ChecklistProcessedData = JsonConvert.SerializeObject(configs);
                                separationProcess.CreatedBy = 1;
                                separationProcess.CreatedOn = DateTime.Now;
                                separationProcess.UpdatedBy = 1;
                                separationProcess.UpdatedOn = DateTime.Now;
                                separationProcess.StatusID = 0;
                                separationProcess.RoleID = roleId;
                                separationProcess.SeperationID = model.ID;
                                db.SeperationProcess.Add(separationProcess);
                                db.SaveChanges();

                                SendForApproval(separationProcess, basicOpsService);//Need to test on 18/01/2018
                                //SendForApproval(separationProcess);
                            }
                        }
                        roles = string.Join(",", roleIds.ToArray());
                        model.StatusID = 3;
                    }

                    lstResult = GetResignationEmail(model, "", "", roles, 7, model.CreatedBy, _EmailSendingService);

                }
            }
            catch
            {
                lstResult[0].isActionPerformed = false;
                lstResult[0].message = string.Format("Action Failed");
            }

            return lstResult;
        }

        public bool SendSCNotice(List<SeperationTerminationViewModel> model)
        {
            bool IsSendSCNotice = false;
            foreach (var item in model)
            {
                IsSendSCNotice = _EmailService.SendSCNotice(item);
            }
            return IsSendSCNotice;
        }
        #endregion

        #region Separation Process
        /// <summary>
        /// Get Applicable checklist for Logged in approval
        /// </summary>
        /// <param name="roles">Logged in users Roles</param>
        /// <param name="personID">Logged in users EmployeeId</param>
        /// <returns></returns>
        public async Task<SeperationProcessDetailsViewModel> GetSeperationProcessDetails(string roles, int personID, int isHistory, int? year, int? separationMode, bool isSelfView = false)
        {
            try
            {
                int[] _HRRoleId1 = new int[4];
                _HRRoleId1[0] = 12;
                _HRRoleId1[1] = 24;
                _HRRoleId1[2] = 38;
                _HRRoleId1[3] = 11;
                Boolean _IsHR = false;
                int orgPersonID = 0;

                var rolesIds = roles.Split(',').Select(Int32.Parse).ToList();

                if (_HRRoleId1.Intersect(rolesIds).Any())
                    _IsHR = true;

                SeperationProcessDetailsViewModel objSeperationProcess = new SeperationProcessDetailsViewModel();

                bool IsStatusPending = true;
                int chkStatus = 0;
                await Task.Run(() =>
                {
                    using (var _db = _phoenixEntity)
                    {
                        var configRoleIDs = _db.SeperationConfig.Where(c => c.IsActive == true).OrderBy(x => x.Sequence).Select(x => x.RoleID).Distinct().ToList();
                        var totalStages = configRoleIDs.Count();

                        //var data = _db.SeperationProcess.Where(p => p.StatusID == 0 || p.StatusID == 1).OrderBy(x => x.Separation.ApprovalDate).ToList();
                        var data = new List<SeperationProcess>();

                        var chkSelfView = _db.Separation.Where(s => s.PersonID == personID && (s.StatusID == 3 || s.StatusID == 4)).FirstOrDefault();

                        orgPersonID = personID;

                        var separationHistory = _db.Separation.Where(x => x.StatusID == 4).ToList();

                        if (chkSelfView != null)
                            isSelfView = true;

                        /* SA: Need to check for executives to able to view checklist of their Dept. */
                        var executiveRoles = _db.HelpDeskCategories.Where(x => rolesIds.Contains(x.AssignedExecutiveRole.Value) && x.IsDeleted == false).ToList();
                        List<int> _adminRolesForExecutive = new List<int>();
                        if (executiveRoles.Count > 0)
                        {
                            executiveRoles.ForEach(item =>
                            {
                                int assRole = _db.PersonInRole.Where(a => a.RoleID == item.AssignedRole && a.IsDeleted == false).FirstOrDefault().PersonID;
                                _adminRolesForExecutive.Add(assRole);
                            });
                            personID = _adminRolesForExecutive[0];
                        }

                        if (executiveRoles.Count > 0)
                        {
                            /* Call GetApprovalsForExecutiveRole to get ApprovalIds For ExecutiveRole*/
                            var exeApprovalIds = GetApprovalsForExecutiveRole(personID, _ReleaseRequestType, orgPersonID);
                            //data = data.Where(p => exeApprovalIds.Contains(p.ID) && p.RoleID == executiveRoles[0].AssignedRole).OrderBy(c => c.Separation.ApprovalDate).ToList();
                            var assignedRole = Convert.ToInt32(executiveRoles[0].AssignedRole.Value);
                            data = _db.SeperationProcess.Where(p => (p.StatusID == 0 || p.StatusID == 1) && exeApprovalIds.Contains(p.ID) && p.RoleID == assignedRole).OrderBy(x => x.Separation.ApprovalDate).ToList();
                        }
                        else
                        {
                            if (!isSelfView)
                            {
                                // If Not self view then load data based on approval card --Dept Admin Role
                                var approvalIds = GetSeperationApprovals(_db, personID, _ReleaseRequestType);
                                //data = data.Where(p => approvalIds.Contains(Convert.ToInt32(p.ID))).OrderBy(c => c.Separation.ApprovalDate).ToList();
                                data = _db.SeperationProcess.Where(p => (p.StatusID == 0 || p.StatusID == 1) && approvalIds.Contains(p.ID)).OrderBy(x => x.Separation.ApprovalDate).ToList();
                            }
                            else
                            {
                                // If it is self view then load data based on logged in user(self)
                                int _roleID = 0;
                                if (configRoleIDs[0] == 0)
                                    _roleID = configRoleIDs[1];
                                else
                                    _roleID = configRoleIDs[0];
                                //data = data.Where(p => p.SeperationID == chkSelfView.ID).GroupBy(c => c.SeperationID, c => c).Select(g => g.FirstOrDefault()).ToList();

                                data = _db.SeperationProcess.Where(p => (p.StatusID == 0 || p.StatusID == 1) && p.SeperationID == chkSelfView.ID).OrderBy(x => x.Separation.ApprovalDate).ToList();
                                data = data.Where(p => p.SeperationID == chkSelfView.ID).GroupBy(c => c.SeperationID, c => c).Select(g => g.FirstOrDefault()).ToList();
                            }
                        }

                        if (isHistory == 1)
                        {
                            //var _separationCompletedIds = _db.Separation.Where(x => x.StatusID == 4).Select(i => i.ID).ToList();

                            var _separationCompletedIds = new List<int>();
                            if ((year == 0 || year == null) && (separationMode == null || separationMode == -1)) // Default all records
                            {
                                _separationCompletedIds = _db.Separation.Where(x => x.StatusID == 4).Select(i => i.ID).ToList();
                            }
                            else if(separationMode == null || separationMode == -1) // Only year filter
                            {
                                _separationCompletedIds = _db.Separation.Where(x => x.StatusID == 4 && x.ResignDate.Year == year).Select(i => i.ID).ToList();
                            }
                            else if ((year == 0 || year == null) ) //Only Separation Mode filter
                            {
                                _separationCompletedIds = _db.Separation.Where(x => x.StatusID == 4 && x.IsTermination == separationMode).Select(i => i.ID).ToList();
                            }
                            else //Both filters
                            {
                                _separationCompletedIds = _db.Separation.Where(x => x.StatusID == 4 && x.ResignDate.Year == year && x.IsTermination == separationMode).Select(i => i.ID).ToList();
                            }

                            var dataID = _db.SeperationProcess.Where(p => p.StatusID == 1 && _separationCompletedIds.Contains(p.SeperationID.Value)).GroupBy(c => c.SeperationID, c => c).Select(g => g.FirstOrDefault()).OrderByDescending(c => c.Separation.ApprovalDate).ToList();
                            data = dataID;
                        }

                        if (data.Count > 0)
                        {
                            objSeperationProcess.SeperationProcess = Mapper.Map<List<SeperationProcess>, List<SeperationProcessViewModel>>(data);
                            objSeperationProcess.SeperationProcess.ForEach(item =>
                            {
                                int currRoleID = item.RoleID;
                                var seperationData = data.FirstOrDefault(x => x.ID == item.ID).Separation;
                                var exitFormData = _db.PersonExitProcessForm.FirstOrDefault(x => x.SeparationID == seperationData.ID);

                                string _AttachedFile = data.FirstOrDefault(x => x.ID == item.ID).Separation.AttachedFile;
                                if (seperationData != null)
                                {
                                    item.Employee = Mapper.Map<Person, EmployeeBasicProfile>(seperationData.Person1);

                                    int editStyle = GetEditStyle(rolesIds, personID, item.Employee.ID);

                                    var reportingManager = QueryHelper.GetManger(item.Employee.ID);

                                    item.ReportingManager = reportingManager.Name;
                                    item.ReportingManagerID = reportingManager.ID;
                                    item.EmployeeComments = seperationData.Comments;
                                    item.SeperationReason = seperationData.SeperationReason;
                                    item.IsTermination = seperationData.IsTermination.Value;
                                    item.TerminationReason = seperationData.TerminationReason.Value;
                                    item.TerminationRemark = seperationData.TerminationRemark;
                                    item.ResignDate = seperationData.ResignDate;
                                    item.StatusID = seperationData.StatusID;
                                    item.ExitDateRemark = seperationData.ExitDateRemark;
                                    string att = "";

                                    if (seperationData.AttachedFile != null && seperationData.AttachedFile != "")
                                        att = DownloadFiles(item.SeperationID);

                                    item.AttachedFile = att;

                                    if (exitFormData != null)
                                        item.IsExitFormFill = true;
                                    else
                                        item.IsExitFormFill = false;

                                    Boolean allDeptProcessNotComplete = false;
                                    Boolean withdrawReqPending = false;

                                    var processData = _db.SeperationProcess.Where(x => x.SeperationID == seperationData.ID).ToList();
                                    var withdrawReqData = _db.Approval.Where(t => t.RequestType == 10 && t.Status == 0 && t.RequestID == seperationData.ID).ToList();

                                    if (processData.Any(a => a.StatusID == 0 && _HRRoleId1.Contains(a.RoleID.Value) == false))
                                        allDeptProcessNotComplete = true;

                                    if (withdrawReqData.Count > 0) //It will check employee's withdraw request is approved or not by EPM
                                        withdrawReqPending = true;
                                  
                                    //Below Property can be mapped.
                                    if (seperationData.ApprovalDate != null)
                                        item.ApprovalDate = seperationData.ApprovalDate.Value;

                                    item.Comments = data.Where(x => x.ID == item.ID).FirstOrDefault().Comments;

                                    bool readOnly = true;

                                    item.SeperationConfigData = new List<SeperationConfigProcessViewModel>();

                                    foreach (var separationProcessDetail in seperationData.SeperationProcess)
                                    {
                                        if (separationProcessDetail.Separation.ApprovalID == personID)//To show data editable mode for Reporting Manager 
                                            rolesIds.Add(0);

                                        chkStatus = Convert.ToInt32(seperationData.SeperationProcess.Where(x => x.StatusID == 0).Count());

                                        SeperationConfigProcessViewModel separationConfiProces = Mapper.Map<SeperationProcess, SeperationConfigProcessViewModel>(separationProcessDetail);
                                        separationConfiProces.Data = JsonConvert.DeserializeObject<List<SeperationConfigViewModel>>(separationProcessDetail.ChecklistProcessedData);
                                        separationConfiProces.ExitDate = seperationData.ApprovalDate.Value;

                                        separationConfiProces.isContainNotRequired = false;
                                        foreach (var _checklistData in separationConfiProces.Data)
                                        {
                                            if (_checklistData.StatusID == 3)
                                                separationConfiProces.isContainNotRequired = true;
                                        }

                                        if (_HRRoleId1.Contains(separationConfiProces.RoleID))
                                            separationConfiProces.isHRRole = true;
                                        else
                                            separationConfiProces.isHRRole = false;


                                        //If its and HR or Self then show all data 1.HR View 2. Self View
                                        if (editStyle == 2 || editStyle == 1)
                                        {
                                            separationConfiProces.IsReadOnly = readOnly;

                                            //chkStatus = Convert.ToInt32(separationConfiProces.Data.Where(x => x.StatusID == null).Count());
                                            separationConfiProces.isPrint = IsStatusPending;

                                            if (editStyle == 1 && separationConfiProces.StatusID == 0) //&& !isSelfView
                                            {
                                                //if (string.IsNullOrEmpty(separationConfiProces.Comments))
                                                separationConfiProces.IsReadOnly = !readOnly;

                                                //This condition added to make checklist ReadOnly to prevent Exicutive's to submit their own checklist 
                                                if (orgPersonID == seperationData.PersonID && isSelfView)//!isSelfView 
                                                {
                                                    separationConfiProces.IsReadOnly = readOnly;
                                                }

                                                if (allDeptProcessNotComplete && _HRRoleId1.Contains(separationConfiProces.RoleID))//To disable Submit option for HR if all dept. checklist not completed
                                                    separationConfiProces.isChkNotComplete = allDeptProcessNotComplete;
                                            }
                                            else
                                                separationConfiProces.IsReadOnly = readOnly; //Grid will be read only for Self-View

                                            if (editStyle == 1 && !isSelfView && separationConfiProces.StatusID == 1 && allDeptProcessNotComplete && _HRRoleId1.Contains(separationConfiProces.RoleID))
                                            {
                                                if (allDeptProcessNotComplete && _HRRoleId1.Contains(separationConfiProces.RoleID))//To disable Submit option for HR if all dept. checklist not completed
                                                    separationConfiProces.isChkNotComplete = allDeptProcessNotComplete;
                                            }

                                            //To disable Submit option for HR as well as all other dept's if action was not taken for withdrawal req. by EPM 
                                            if (withdrawReqPending)
                                                separationConfiProces.isChkNotComplete = true;

                                            //if (chkStatus == 0 && separationConfiProces.RoleID == _HRRoleId && !isSelfView) //Commented on 07/11/2017
                                            if (chkStatus == 0 && _HRRoleId1.Contains(separationConfiProces.RoleID) && _IsHR && orgPersonID != seperationData.PersonID) // && !isSelfView
                                            {
                                                separationConfiProces.isPrint = !IsStatusPending;
                                            }
                                            item.SeperationConfigData.Add(separationConfiProces);
                                        }
                                        //if data is belong to specific department, load only department specific(Logged in user).
                                        else if (editStyle == 3 && currRoleID == separationProcessDetail.RoleID.Value && orgPersonID != seperationData.PersonID)
                                        {
                                            separationConfiProces.IsReadOnly = readOnly;
                                            //if (string.IsNullOrEmpty(separationConfiProces.Comments))
                                            if (separationConfiProces.StatusID == 0)
                                                separationConfiProces.IsReadOnly = !readOnly;

                                            //This condition added to make checklist ReadOnly to prevent Exicutive's to submit their own checklist 
                                            if (orgPersonID == seperationData.PersonID && isSelfView)//!isSelfView 
                                            {
                                                separationConfiProces.IsReadOnly = readOnly;
                                            }

                                            separationConfiProces.isPrint = true;
                                            //To disable Submit option for HR as well as all other dept's if action was not taken for withdrawal req. by EPM 
                                            if (withdrawReqPending)
                                            {
                                                separationConfiProces.isChkNotComplete = true;
                                                //separationConfiProces.IsReadOnly = true;
                                            }

                                            item.SeperationConfigData.Add(separationConfiProces);
                                        }
                                        /// Self view for Department Clearence
                                        else if (editStyle == 3 && orgPersonID == seperationData.PersonID)
                                        {
                                            separationConfiProces.IsReadOnly = true;
                                            separationConfiProces.isPrint = true;
                                            item.SeperationConfigData.Add(separationConfiProces);
                                        }
                                    }
                                }
                            });
                        }

                        //SA: To get records which separation request accepted by HR 
                        if (isHistory == 0 && _IsHR && !isSelfView)
                        {
                            var empData = _db.Separation.Where(s => s.StatusID == 0 || s.StatusID == 2).OrderBy(x => x.ApprovalDate).ToList();

                            if (data.Count <= 0)
                            {
                                objSeperationProcess.SeperationProcess = new List<SeperationProcessViewModel>();
                            }

                            List<SeperationProcessViewModel> pendingDataList = new List<SeperationProcessViewModel>();
                            empData.ForEach(item =>
                            {
                                SeperationProcessViewModel pendingData = new SeperationProcessViewModel();
                              
                                if (item.ApprovalDate.HasValue)
                                {
                                    pendingData.ApprovalDate = item.ApprovalDate.Value;

                                    pendingData.Employee = Mapper.Map<Person, EmployeeBasicProfile>(item.Person1);

                                    var reportingManager = QueryHelper.GetManger(item.PersonID);

                                    pendingData.ReportingManager = reportingManager.Name;
                                    pendingData.ReportingManagerID = reportingManager.ID;
                                    pendingData.EmployeeComments = item.Comments;
                                    pendingData.SeperationReason = item.SeperationReason;
                                    pendingData.IsTermination = item.IsTermination.Value;
                                    pendingData.TerminationReason = item.TerminationReason.Value;
                                    pendingData.TerminationRemark = item.TerminationRemark;
                                    pendingData.ResignDate = item.ResignDate;
                                    pendingData.AttachedFile = "";
                                    pendingData.ChecklistProcessedData = null;
                                    pendingData.SeperationConfigData = null;
                                    pendingData.StatusID = item.StatusID;
                                    pendingData.SeperationID = item.ID;
                                    pendingData.ExitDateRemark = item.ExitDateRemark;
                                    objSeperationProcess.SeperationProcess.Add(pendingData);
                                }
                            });
                        }
                    }
                });

                return objSeperationProcess;
            }
            catch (Exception ex)
            {
                throw new HttpException(500, ex.ToString());
            }
        }


        public async Task<SeperationProcessDetailsViewModel> GetSeperationProcessDetailsForDeptAdmin(string roles, int personID, bool isSelfView = false)
        {
            try
            {
                int[] _HRRoleId1 = new int[4];
                _HRRoleId1[0] = 12;
                _HRRoleId1[1] = 24;
                _HRRoleId1[2] = 38;
                _HRRoleId1[3] = 11;
                Boolean _IsHR = false;

                var rolesIds = roles.Split(',').Select(Int32.Parse).ToList();

                foreach (var item in _HRRoleId1)
                {
                    if (rolesIds.Contains(item))
                    {
                        _IsHR = true;
                    }
                }

                SeperationProcessDetailsViewModel objSeperationProcess = new SeperationProcessDetailsViewModel();

                bool IsStatusPending = true;
                int chkStatus = 0;
                await Task.Run(() =>
                {
                    using (var _db = _phoenixEntity)
                    {
                        var configRoleIDs = _db.SeperationConfig.Where(c => c.IsActive == true).OrderBy(x => x.Sequence).Select(x => x.RoleID).Distinct().ToList();
                        var totalStages = configRoleIDs.Count();

                        //var data = _db.SeperationProcess.Where(p => p.StatusID == 0 || p.StatusID == 1).OrderBy(x => x.Separation.ResignDate).GroupBy(c => c.SeperationID, c => c).Select(g => g.FirstOrDefault()).ToList();
                        var data = _db.SeperationProcess.Where(p => p.StatusID == 0 || p.StatusID == 1).ToList();

                        var chkSelfView = _db.Separation.Where(s => s.PersonID == personID && (s.StatusID == 3 || s.StatusID == 4)).FirstOrDefault();

                        /* SA: Need to check for executives to able to view checklist of their Dept. */
                        var executiveRoles = _db.HelpDeskCategories.Where(x => rolesIds.Contains(x.AssignedExecutiveRole.Value) && x.IsDeleted == false).ToList();
                        List<int> _adminRolesForExecutive = new List<int>();
                        if (executiveRoles.Count > 0)
                        {
                            foreach (var item in executiveRoles)
                            {
                                int assRole = _db.PersonInRole.Where(a => a.RoleID == item.AssignedRole && a.IsDeleted == false).FirstOrDefault().PersonID;
                                _adminRolesForExecutive.Add(assRole);
                            }
                            personID = _adminRolesForExecutive[0];
                        }

                        // If Not self view then load data based on approval card
                        var approvalIds = GetSeperationApprovals(_db, personID, _ReleaseRequestType);
                        data = data.Where(p => approvalIds.Contains(p.ID)).OrderBy(c => c.Separation.ApprovalDate).ToList();

                        if (chkSelfView != null)
                        {
                            isSelfView = true;
                            foreach (var item in data)
                            {
                                if (item.SeperationID == chkSelfView.ID)
                                {
                                    data.Remove(item);
                                    break;
                                }
                            }
                        }

                        if (data.Count > 0)
                        {
                            objSeperationProcess.SeperationProcess = Mapper.Map<List<SeperationProcess>, List<SeperationProcessViewModel>>(data);
                            foreach (var item in objSeperationProcess.SeperationProcess)
                            {
                                int currRoleID = item.RoleID;
                                var seperationData = data.FirstOrDefault(x => x.ID == item.ID).Separation;
                                var exitFormData = _db.PersonExitProcessForm.FirstOrDefault(x => x.SeparationID == seperationData.ID);

                                string _AttachedFile = data.FirstOrDefault(x => x.ID == item.ID).Separation.AttachedFile;
                                if (seperationData != null)
                                {
                                    item.Employee = Mapper.Map<Person, EmployeeBasicProfile>(seperationData.Person1);

                                    int editStyle = GetEditStyle(rolesIds, personID, item.Employee.ID);

                                    var reportingManager = QueryHelper.GetManger(item.Employee.ID);

                                    item.ReportingManager = reportingManager.Name;
                                    item.ReportingManagerID = reportingManager.ID;
                                    item.EmployeeComments = seperationData.Comments;
                                    item.SeperationReason = seperationData.SeperationReason;
                                    item.IsTermination = seperationData.IsTermination.Value;
                                    item.TerminationReason = seperationData.TerminationReason.Value;
                                    item.TerminationRemark = seperationData.TerminationRemark;
                                    //item.ExitDateRemark = seperationData.ExitDateRemark;
                                    item.ResignDate = seperationData.ResignDate;
                                    item.StatusID = seperationData.StatusID;
                                    item.ExitDateRemark = seperationData.ExitDateRemark;
                                    string att = "";

                                    if (seperationData.AttachedFile != null && seperationData.AttachedFile != "")
                                        att = DownloadFiles(item.SeperationID);

                                    item.AttachedFile = att;

                                    if (exitFormData != null)
                                        item.IsExitFormFill = true;
                                    else
                                        item.IsExitFormFill = false;

                                    Boolean allDeptProcessNotComplete = false;
                                    //using (var DB = new PhoenixEntities())
                                    //{
                                    var processData = _db.SeperationProcess.Where(x => x.SeperationID == seperationData.ID).ToList();
                                    foreach (var _processData in processData)
                                    {
                                        if (_processData.StatusID == 0 && !_HRRoleId1.Contains(_processData.RoleID.Value))
                                            allDeptProcessNotComplete = true;
                                    }
                                    //  }

                                    //Below Property can be mapped.
                                    if (seperationData.ApprovalDate != null)
                                        item.ApprovalDate = seperationData.ApprovalDate.Value;

                                    item.Comments = data.Where(x => x.ID == item.ID).FirstOrDefault().Comments;

                                    bool readOnly = true;

                                    item.SeperationConfigData = new List<SeperationConfigProcessViewModel>();

                                    foreach (var separationProcessDetail in seperationData.SeperationProcess)
                                    {
                                        if (separationProcessDetail.Separation.ApprovalID == personID)//To show data editable mode for Reporting Manager 
                                            rolesIds.Add(0);

                                        chkStatus = Convert.ToInt32(seperationData.SeperationProcess.Where(x => x.StatusID == 0).Count());

                                        SeperationConfigProcessViewModel separationConfiProces = Mapper.Map<SeperationProcess, SeperationConfigProcessViewModel>(separationProcessDetail);
                                        separationConfiProces.Data = JsonConvert.DeserializeObject<List<SeperationConfigViewModel>>(separationProcessDetail.ChecklistProcessedData);
                                        separationConfiProces.ExitDate = seperationData.ApprovalDate.Value;

                                        separationConfiProces.isContainNotRequired = false;
                                        foreach (var _checklistData in separationConfiProces.Data)
                                        {
                                            if (_checklistData.StatusID == 3)
                                                separationConfiProces.isContainNotRequired = true;
                                        }

                                        if (_HRRoleId1.Contains(separationConfiProces.RoleID))
                                            separationConfiProces.isHRRole = true;
                                        else
                                            separationConfiProces.isHRRole = false;

                                        //If its and HR or Self then show all data
                                        if (editStyle == 2 || editStyle == 1)
                                        {
                                            separationConfiProces.IsReadOnly = readOnly;

                                            //chkStatus = Convert.ToInt32(separationConfiProces.Data.Where(x => x.StatusID == null).Count());
                                            separationConfiProces.isPrint = IsStatusPending;

                                            if (editStyle == 1 && !isSelfView && separationConfiProces.StatusID == 0)
                                            {
                                                //if (string.IsNullOrEmpty(separationConfiProces.Comments))
                                                separationConfiProces.IsReadOnly = !readOnly;

                                                if (allDeptProcessNotComplete && _HRRoleId1.Contains(separationConfiProces.RoleID))//To disable Submit option for HR if all dept. checklist not completed
                                                    separationConfiProces.isChkNotComplete = allDeptProcessNotComplete;
                                            }
                                            else
                                                separationConfiProces.IsReadOnly = readOnly; //Grid will be read only for Self-View

                                            if (editStyle == 1 && !isSelfView && separationConfiProces.StatusID == 1 && allDeptProcessNotComplete && _HRRoleId1.Contains(separationConfiProces.RoleID))
                                            {
                                                if (allDeptProcessNotComplete && _HRRoleId1.Contains(separationConfiProces.RoleID))//To disable Submit option for HR if all dept. checklist not completed
                                                    separationConfiProces.isChkNotComplete = allDeptProcessNotComplete;
                                            }

                                            //if (chkStatus == 0 && separationConfiProces.RoleID == _HRRoleId && !isSelfView) //Commented on 07/11/2017
                                            if (chkStatus == 0 && _HRRoleId1.Contains(separationConfiProces.RoleID) && !isSelfView)
                                            {
                                                separationConfiProces.isPrint = !IsStatusPending;
                                            }
                                            item.SeperationConfigData.Add(separationConfiProces);
                                        }
                                        //if data is belong to specific department, load only department specific(Logged in user).
                                        else if (editStyle == 3 && currRoleID == separationProcessDetail.RoleID.Value)
                                        {
                                            separationConfiProces.IsReadOnly = readOnly;
                                            //if (string.IsNullOrEmpty(separationConfiProces.Comments))
                                            if (separationConfiProces.StatusID == 0)
                                                separationConfiProces.IsReadOnly = !readOnly;

                                            separationConfiProces.isPrint = true;

                                            item.SeperationConfigData.Add(separationConfiProces);
                                        }
                                    }
                                }
                            }
                        }

                        //SA: To get records which separation request accepted by HR 
                        if (_IsHR)
                        {
                            var empData = _db.Separation.Where(s => s.StatusID == 0 || s.StatusID == 2).OrderBy(x => x.ApprovalDate).ToList();

                            List<SeperationProcessViewModel> pendingDataList = new List<SeperationProcessViewModel>();
                            foreach (var item in empData)
                            {
                                SeperationProcessViewModel pendingData = new SeperationProcessViewModel();

                                //if (!string.IsNullOrEmpty(Convert.ToString(item.ApprovalDate)) && item.ApprovalDate.ToString() != "")
                                //if (item.ApprovalDate != default(DateTime))
                                if (item.ApprovalDate.HasValue)
                                {
                                    pendingData.ApprovalDate = item.ApprovalDate.Value;

                                    pendingData.Employee = Mapper.Map<Person, EmployeeBasicProfile>(item.Person1);

                                    var reportingManager = QueryHelper.GetManger(item.PersonID);

                                    pendingData.ReportingManager = reportingManager.Name;
                                    pendingData.ReportingManagerID = reportingManager.ID;
                                    pendingData.EmployeeComments = item.Comments;
                                    pendingData.SeperationReason = item.SeperationReason;
                                    pendingData.IsTermination = item.IsTermination.Value;
                                    pendingData.TerminationReason = item.TerminationReason.Value;
                                    pendingData.TerminationRemark = item.TerminationRemark;
                                    pendingData.ResignDate = item.ResignDate;
                                    pendingData.AttachedFile = "";
                                    pendingData.ChecklistProcessedData = null;
                                    pendingData.SeperationConfigData = null;
                                    pendingData.StatusID = item.StatusID;
                                    pendingData.SeperationID = item.ID;
                                    pendingData.ExitDateRemark = item.ExitDateRemark;
                                    objSeperationProcess.SeperationProcess.Add(pendingData);
                                }
                            }
                        }
                    }
                });

                return objSeperationProcess;
            }
            catch (Exception ex)
            {
                throw new HttpException(500, ex.ToString());
            }
        }
        /// <summary>
        /// Get Edit syle based on Logged in user.
        /// </summary>
        /// <param name="rolesIds">List of Roles for Edit Style</param>
        /// <param name="userId">Logged in user id.</param>
        /// <param name="employee">Employee Id whose separation is processing.</param>
        /// <returns></returns>
        int GetEditStyle(List<int> rolesIds, int userId, int employee)
        {
            // could be a better have editStyle
            // 1:- HR View, 2:- Self View, 3:- Department Specific View
            int[] _HRRoleId1 = new int[4];
            _HRRoleId1[0] = 12;
            _HRRoleId1[1] = 24;
            _HRRoleId1[2] = 38;
            _HRRoleId1[3] = 11;

            Boolean isHRRole = false;

            foreach (var item in _HRRoleId1)
            {
                if (rolesIds.Contains(item))
                {
                    isHRRole = true;
                }
            }

            var editStyle = 3; // Department View
            if (isHRRole && userId != employee)
            {
                editStyle = 1; // HR View
            }
            else if (userId == employee)
            {
                editStyle = 2; // Self View
            }
            else
                editStyle = 3;


            return editStyle;
        }

        public ActionResult CompleteSeperationProcess(SeperationConfigProcessViewModel model, int userId)
        {
            ActionResult result = new ActionResult();
            try
            {
                using (var db = _phoenixEntity)
                {
                    int[] hrRoles = new int[4];
                    hrRoles[0] = 12;
                    hrRoles[1] = 24;
                    hrRoles[2] = 38;
                    hrRoles[3] = 11;

                    SeperationProcess separationProcess = db.SeperationProcess.FirstOrDefault(x => x.ID == model.ID);
                    Separation sep = db.Separation.FirstOrDefault(i => i.ID == separationProcess.SeperationID);
                    var separations = new SeperationViewModel();
                    Boolean isNotRequired = false;

                    if (separationProcess != null)
                    {
                        if (model.Data.Any(a => a.StatusID == 3 ))
                            isNotRequired = true;

                        if (!isNotRequired) //It will removed comment if 'Not Required' status is not selected in dept. checklist
                            model.Comments = "";

                        var data = JsonConvert.SerializeObject(model.Data);
                        separationProcess.ChecklistProcessedData = data;
                        separationProcess.Comments = model.Comments;
                        Boolean isComplete = true;

                        //If check list item is Pending then final status should not be mark as completed                       

                        if (model.Data.Any(a => a.StatusID != 2 && a.StatusID != 3))
                            isComplete = false;

                        if (isComplete)
                        {
                            separationProcess.StatusID = 1;
                            separationProcess.UpdatedBy = userId;
                            separationProcess.UpdatedOn = DateTime.Now;
                        }

                        sep.ApprovalDate = model.ExitDate;
                        sep.UpdatedBy = userId;
                        sep.UpdatedOn = DateTime.Now;

                        if (sep.IsTermination == 1)
                        {
                            sep.ResignDate = model.ExitDate;
                            //sep.StatusID = 4;
                        }

                        db.SaveChanges();

                        //Commented on 24/12/2017 --Start Here
                        //To send mail by Department for completion of Clearance process
                        //if (isComplete)
                        //{
                        //    var personSeparation = _repo.FirstOrDefault<Separation>(t => t.ID == separationProcess.Separation.ID);
                        //    separations = Mapper.Map<Separation, SeperationViewModel>(personSeparation);
                        //    string deptEmail = GetPersonEmail(userId);
                        //    string personEmail = GetPersonEmail(separationProcess.Separation.PersonID); //TODO on 11/09/2017
                        //    separations.EmployeeProfile = Mapper.Map<Person, EmployeeBasicProfile>(personSeparation.Person1);

                        //    string logInUser = GetLogInUser(userId);
                        //    //personEmail = personEmail + ";" + GetHRGroupEmailIds();

                        //    string deptName = GetDeptName(separationProcess.RoleID.Value);
                        //    string subject = "Department Clearance – Approve by " + logInUser + " for " + deptName;//GetEmailSubject(4);
                        //    //Note: IF HR complete its own dept. checklist mail should not get triggered.
                        //    if (!hrRoles.Contains(separationProcess.RoleID.Value))
                        //        _EmailService.SendResignationEmail(separations, subject, deptName.ToString() + "," + separationProcess.RoleID.Value, deptEmail, personEmail, false, false, 8, "", logInUser);

                        //}
                        //Commented on 24/12/2017 --End Here

                        //if ((_HRRoleId != separationProcess.RoleID && isComplete) || (_HRRoleId == separationProcess.RoleID && isComplete && sep.IsTermination == 1))
                        if ((!hrRoles.Contains(separationProcess.RoleID.Value) && isComplete)) //|| (hrRoles.Contains(separationProcess.RoleID.Value) && isComplete && sep.IsTermination == 1)
                        //|| (hrRoles.Contains(separationProcess.RoleID.Value) && isComplete && sep.IsTermination == 1)
                        {
                            var personSeparation = _repo.FirstOrDefault<Separation>(t => t.ID == separationProcess.Separation.ID);
                            separations = Mapper.Map<Separation, SeperationViewModel>(personSeparation);
                            string deptEmail = GetPersonEmail(userId);
                            string personEmail = GetPersonEmail(separationProcess.Separation.PersonID); //TODO on 11/09/2017
                            separations.EmployeeProfile = Mapper.Map<Person, EmployeeBasicProfile>(personSeparation.Person1);

                            string logInUser = GetLogInUser(userId);
                            string deptName = GetDeptName(separationProcess.RoleID.Value);

                            var approvalData = db.Approval.Where(x => x.RequestID == separationProcess.ID && x.RequestType == 11).ToList();
                            int reqID = approvalData[0].ID;
                            var approvalDetailData = db.ApprovalDetail.Where(i => i.ApprovalID == reqID).ToList();
                            int apprID = Convert.ToInt32(approvalDetailData[0].ApproverID);
                            int roleID = separationProcess.RoleID.Value;
                            string deptAdminName = GetLogInUser(apprID);

                            var deptEmailID = string.Empty;

                            if (roleID != 0)
                                deptEmailID = service.First<HelpDeskCategories>(x => x.AssignedRole == roleID).EmailGroup.ToString(); // db.HelpDeskCategories.Where(x => x.AssignedRole == roleID).FirstOrDefault().EmailGroup;
                            else
                                deptEmailID = GetPersonEmail(separations.ApprovalID);//This will get EPM email ID in case of RoleID=0

                            if (apprID == userId)//If dept. clearance approved by Dept. Admin itself
                            {
                                string subject = "Department Clearance – Approved by " + logInUser + " for " + deptName + " Department";//GetEmailSubject(4);
                                //Note: IF HR complete its own dept. checklist mail should not get triggered.
                                if (!hrRoles.Contains(separationProcess.RoleID.Value))
                                    _EmailService.SendResignationEmail(separations, subject, deptName.ToString() + "," + deptAdminName + "," + apprID.ToString() + "," + roleID, deptEmail, personEmail, false, false, 8, deptEmailID.ToString(), logInUser);
                            }
                            else//If dept. clearance approved by HR on behalf of Dept. Admin
                            {
                                string subject = "Department Clearance – Approved by " + logInUser + " on behalf of " + deptAdminName + " for " + deptName + " Department";//GetEmailSubject(4);
                                //Note: IF HR complete its own dept. checklist mail should not get triggered.
                                if (!hrRoles.Contains(separationProcess.RoleID.Value))
                                    _EmailService.SendResignationEmail(separations, subject, deptName.ToString() + "," + deptAdminName + "," + apprID.ToString() + "," + roleID, deptEmail, personEmail, false, false, 28, deptEmailID.ToString(), logInUser);
                            }

                            UpdateApproval(separationProcess, apprID);
                        }
                        //else if (hrRoles.Contains(separationProcess.RoleID.Value) && isComplete)
                        //{
                        //    sep.StatusID = 4;
                        //    db.SaveChanges();
                        //}
                    }

                    result.isActionPerformed = true;
                    
                    if (separationProcess.Separation.TerminationReason == 6)
                    {
                        if(separationProcess.ChecklistProcessedData.Contains("All pending process cleared"))
                        {
                            SeperationViewModel separation = new SeperationViewModel();
                            separation.ApprovalDate = separationProcess.Separation.ApprovalDate ?? DateTime.Now;
                            separation.IsTermination = separationProcess.Separation.IsTermination ?? 0;
                            separation.TerminationReason = separationProcess.Separation.TerminationReason ?? 0;
                            //var seperation = db.Separation.Where(x => x.ID == separationProcess.Separation.separationId).FirstOrDefault();
                            //seperation.StatusID = 4;
                            sep.UpdatedBy = userId;
                            sep.UpdatedOn = DateTime.Now;
                            db.SaveChanges();
                            var data = EmployeeInactive(separationProcess.Separation.PersonID, userId, separation);
                            result.message = string.Format("Contract separation process completed.");
                        }                      
                    }
                    else
                    {
                        result.message = string.Format("Separation process completed.");
                    }
                }
            }
            catch
            {
                result.isActionPerformed = true;
                result.message = string.Format("Action Failed");
            }
            return result;
        }

        #endregion

        #region Printing
        //To Generate Separation Letters 1-Experience 2.Releiving 3.Termination
        public async Task<HttpResponseMessage> GenerateDocument(int letterType, int separationId, int UserId, string fileType = "")
        {
            SeperationViewModel separations = new SeperationViewModel();
            var personSeparation = _repo.FirstOrDefault<Separation>(t => (separationId == 0 || t.ID == separationId));
            if (personSeparation != null)
            {
                separations = Mapper.Map<Separation, SeperationViewModel>(personSeparation);
                separations.EmployeeProfile = Mapper.Map<Person, EmployeeBasicProfile>(personSeparation.Person1);
            }

            if (separations != null)
            {
                //TO update employment status of Employee as Resigned
                if (separations.IsTermination == 0)
                    UpdatePersonEmploymentStatus(separations.EmployeeProfile.ID, 3);

                string reportName = string.Empty;
                switch (letterType)
                {
                    case 1:
                        reportName = "ExperienceLetter";
                        break;
                    case 2:
                        reportName = "RelievingLetter";
                        break;
                    case 3:
                        reportName = "TerminationLetter";
                        break;
                    case 4:
                        reportName = "NDALetter";
                        break;
                }

                //To update final separation process of HR role.
                //using (var db = new PhoenixEntities())
                //{
                //    SeperationProcess separationProcess = db.SeperationProcess.FirstOrDefault(x => x.RoleID == Convert.ToInt32(ConfigurationManager.AppSettings["HRRoleId"]) && x.SeperationID == separationId);
                //    UpdateApproval(separationProcess, UserId);
                //}

                using (var db1 = new PhoenixEntities())
                {
                    var seperation = db1.Separation.Where(x => x.ID == separationId).FirstOrDefault();
                    //seperation.StatusID = 4;
                    seperation.UpdatedBy = UserId;
                    seperation.UpdatedOn = DateTime.Now;
                    db1.SaveChanges();

                    var roleIds = db1.SeperationProcess.Where(t => t.SeperationID == separationId).Select(k => k.RoleID).Distinct().ToList();
                    string roles = string.Join(",", roleIds.ToArray());
                    //TODO: Need to remove after testing
                    //var str = SendResignationProcessEmails(separations, "", "", roles,9);
                }

                //To inactive employee through normal resignation flow.
                var data = EmployeeInactive(separations.EmployeeProfile.ID, UserId, separations);


                if (fileType == "PDF")
                {
                    return await _PrintReport.GetSeparationPDFPrint(_repo, separations, reportName, fileType);
                }
                else
                {
                    return await _PrintReport.GetSeparationDOCPrint(_repo, separations, reportName, fileType);
                }
                //To print letters
                //return await _PrintReport.GetSeparationPDFPrint(_repo, separations, reportName);
            }
            return null;
        }
        #endregion

        #region Emails Section
        //TODO: Need to remove after testing
        async Task<ActionResult> SendResignationProcessEmails(SeperationViewModel model, string subject, string body, string rolIds, int status, int UserId, IEmailService _EmailSendingService = null)
        {
            ActionResult resultMsg = new ActionResult();
            {
                if (_EmailSendingService != null)
                    _EmailService = _EmailSendingService;

                List<string> result = rolIds.Split(',').ToList();
                string emailToCC = "";

                //using (PhoenixEntities entites = new PhoenixEntities())
                //{
                //    foreach (var item in result)
                //    {
                //        var approvar = GetApprovalBasedOnRole(null, Convert.ToInt32(item));
                //        if (Convert.ToInt32(item) == 0)
                //            approvar = model.ApprovalID;

                //        var employee = entites.People.Where(x => x.ID == approvar).FirstOrDefault();
                //        emailToCC = emailToCC + employee.PersonEmployment.First().OrganizationEmail + ";";
                //    }
                //}

                string personEmail = GetPersonEmail(model.PersonID);
                string exitProcessMgrEmail = GetPersonEmail(model.ApprovalID);
                string logInUserEmail = GetPersonEmail(UserId);

                string ForUserName = GetLogInUser(model.PersonID);
                string sub = "Department clearance for " + ForUserName + " (" + model.PersonID + ") has been initiated";

                string logInUserName = GetLogInUser(UserId);


                // await Task.Run(() => _EmailService.SendResignationProcessEmails(model, subject, body, emailToCC));

                if (status == 7 && model.IsTermination == 0)
                    _EmailService.SendResignationEmail(model, sub, "Separation process initiated", Convert.ToString(ConfigurationManager.AppSettings["helpdeskEmailId"]), personEmail, false, false, 7, "", "Vibrant Desk");//emailToCC
                else if (status == 7 && model.IsTermination == 1)
                {
                    //Commented on 08/02/2018 --As per HR discussion this mail is not needed
                    _EmailService.SendResignationEmail(model, "Separation process initiated for " + ForUserName + " (" + model.PersonID + ") ", "Separation process initiated", logInUserEmail, exitProcessMgrEmail, false, false, 19, "", logInUserName);

                    //if (model.TerminationReason != 1 && model.TerminationReason != 5)
                    //    _EmailService.SendResignationEmail(model, sub, "Separation process initiated", logInUserEmail, exitProcessMgrEmail, false, false, 14, "", logInUserName);

                    ////_EmailService.SendResignationEmail(model, sub, "Separation process initiated", logInUserEmail, personEmail, false, false, 7, GetHRGroupEmailIds(), logInUserName);
                    //if (model.TerminationReason == 4) //Absconding                    
                    //    _EmailService.SendResignationEmail(model, sub, "Show cause notice-1", logInUserEmail, personEmail, false, false, 15, "", logInUserName);
                    //else if (model.TerminationReason == 3)//Resignation w/o settlement                    
                    //    _EmailService.SendResignationEmail(model, sub, "Separation process initiated", logInUserEmail, personEmail, false, false, 18, "", logInUserName);

                    string empPersonalEmail = GetPersonalEmail(model.PersonID);

                    switch (model.TerminationReason)
                    {
                        case 2://Absconding immediately after joining
                            _EmailService.SendResignationEmail(model, "Temporary access block for " + model.EmployeeProfile.FirstName + " " + model.EmployeeProfile.LastName + " (" + model.PersonID + ") ", "Temporary block the access", logInUserEmail, exitProcessMgrEmail, false, false, 14, "", logInUserName);
                            break;
                        case 3://Resignation without settlement
                            _EmailService.SendResignationEmail(model, "Temporary access block for " + model.EmployeeProfile.FirstName + " " + model.EmployeeProfile.LastName + " (" + model.PersonID + ") ", "Temporary block the access", logInUserEmail, exitProcessMgrEmail, false, false, 14, "", logInUserName);
                            _EmailService.SendResignationEmail(model, "Show cause notice - 1", "Resignation w/o settlement SCN1", logInUserEmail, empPersonalEmail, false, false, 18, "", logInUserName);
                            break;
                        case 4://Absconding
                            _EmailService.SendResignationEmail(model, "Temporary access block for " + model.EmployeeProfile.FirstName + " " + model.EmployeeProfile.LastName + " (" + model.PersonID + ") ", "Temporary block the access", logInUserEmail, exitProcessMgrEmail, false, false, 14, "", logInUserName);
                            _EmailService.SendResignationEmail(model, "Show cause notice - 1 ", "Show cause notice-1", logInUserEmail, empPersonalEmail, false, false, 15, "", logInUserName);
                            break;
                        case 5://On PIP
                            _EmailService.SendResignationEmail(model, "PIP update for " + model.EmployeeProfile.FirstName + " " + model.EmployeeProfile.LastName + " (" + model.PersonID + ") ", "Show cause notice-1", logInUserEmail, exitProcessMgrEmail, false, false, 29, "", logInUserName);
                            break;
                    }
                }
                else if (status == 9)
                    _EmailService.SendResignationEmail(model, "Exit Interview Feedback Form", "Separation process closed by HR", logInUserEmail, personEmail, false, false, 9, "", logInUserName); //emailToCC
                else if (status == 5)
                    _EmailService.SendResignationEmail(model, "Withdraw Request accepted by " + logInUserName + "", "Separation withdraw request accepted", exitProcessMgrEmail, personEmail, false, false, 5, emailToCC, logInUserName);
                else if (status == 16)
                {
                    sub = "Separation process is completed for " + ForUserName + " (" + model.PersonID + ")";
                    _EmailService.SendResignationEmail(model, sub, "HR Separation Completed", logInUserEmail, personEmail, false, false, 16, "", logInUserName);
                }
                resultMsg.isActionPerformed = true;
                resultMsg.message = string.Format("Email send Successfully");
            }
            return resultMsg;
        }

        public List<SeperationTerminationViewModel> GetResignationEmail(SeperationViewModel model, string subject, string body, string rolIds, int status, int UserId, IEmailService _EmailSendingService = null)
        {
            List<SeperationTerminationViewModel> lstResult = new List<SeperationTerminationViewModel>();
            SeperationTerminationViewModel result = new SeperationTerminationViewModel();
            //string personEmail = string.Empty;
            //if (!string.IsNullOrEmpty(model.EmailID))
            //{
            //    personEmail = GetPersonEmail(model.PersonID);
            //}
            string exitProcessMgrEmail = GetPersonEmail(model.ApprovalID);
            string logInUserEmail = string.Empty;
            logInUserEmail = GetPersonEmail(UserId);
            string logInUserName = GetLogInUser(UserId);
            string empPersonalEmail = string.Empty;
            //if (string.IsNullOrEmpty(model.EmailID))
            //{
            empPersonalEmail = GetPersonalEmail(model.PersonID);
            //}

            string ForUserName = GetLogInUser(model.PersonID);
            if (model.TerminationReason == 6) // Contract conversion case
            {
                _EmailService.SendResignationEmail(model, "Contract conversion process initiated for " + ForUserName + " (" + model.PersonID + ") ", "Contract conversion process initiated", logInUserEmail, exitProcessMgrEmail, false, false, 30, "", logInUserName);
            }
            else
            {
                _EmailService.SendResignationEmail(model, "Separation process initiated for " + ForUserName + " (" + model.PersonID + ") ", "Separation process initiated", logInUserEmail, exitProcessMgrEmail, false, false, 19, "", logInUserName);
            }
            switch (model.TerminationReason)
            {
                case 1://HR Separation
                    result.SeperationReason = "HR Separation";
                    result.isActionPerformed = true;
                    result.message = "Resignation Email send Successfully";
                    lstResult.Add(result);
                    break;
                case 2://Absconding immediately after joining
                    result = _EmailService.GetResignationEmail(model, "Temporary access block for " + model.EmployeeProfile.FirstName + " " + model.EmployeeProfile.LastName + " (" + model.PersonID + ") ", "Temporary block the access", logInUserEmail, exitProcessMgrEmail, false, false, 14, "", logInUserName);
                    lstResult.Add(result);
                    break;
                case 3://Resignation without settlement
                    result = _EmailService.GetResignationEmail(model, "Temporary access block for " + model.EmployeeProfile.FirstName + " " + model.EmployeeProfile.LastName + " (" + model.PersonID + ") ", "Temporary block the access", logInUserEmail, exitProcessMgrEmail, false, false, 14, "", logInUserName);
                    lstResult.Add(result);
                    result = _EmailService.GetResignationEmail(model, "Show cause notice - 1", "Resignation w/o settlement SCN1", logInUserEmail, empPersonalEmail, false, false, 18, "", logInUserName);
                    lstResult.Add(result);
                    break;
                case 4://Absconding
                    result = _EmailService.GetResignationEmail(model, "Temporary access block for " + model.EmployeeProfile.FirstName + " " + model.EmployeeProfile.LastName + " (" + model.PersonID + ") ", "Temporary block the access", logInUserEmail, exitProcessMgrEmail, false, false, 14, "", logInUserName);
                    lstResult.Add(result);
                    result = _EmailService.GetResignationEmail(model, "Show cause notice - 1 ", "Show cause notice-1", logInUserEmail, empPersonalEmail, false, false, 15, "", logInUserName);
                    lstResult.Add(result);
                    break;
                case 5://On PIP
                    result = _EmailService.GetResignationEmail(model, "PIP update for " + model.EmployeeProfile.FirstName + " " + model.EmployeeProfile.LastName + " (" + model.PersonID + ") ", "Show cause notice-1", logInUserEmail, exitProcessMgrEmail, false, false, 29, "", logInUserName);
                    lstResult.Add(result);
                    break;
                case 6://HR Separation contract conversion case
                    result.SeperationReason = "Contract Conversion";
                    result.isActionPerformed = true;
                    result.message = "Contract conversion process initiated";
                    lstResult.Add(result);
                    break;
            }
            return lstResult;
        }

        public string GetPersonEmail(int personID)
        {
            PhoenixEntities entites = new PhoenixEntities();
            Person personData = entites.People.Where(x => x.ID == personID).FirstOrDefault();
            string email = personData.PersonEmployment.First().OrganizationEmail;
            return email;
        }
        #endregion

        #region Employee Inactivation
        //To inactivate employee from system
        public ActionResult EmployeeInactive(int PersonID, int UserId, SeperationViewModel separations = null)
        {
            ActionResult result = new ActionResult();
            try
            {
                int[] hrRoles = new int[4];
                hrRoles[0] = 12;
                hrRoles[1] = 24;
                hrRoles[2] = 38;
                hrRoles[3] = 11;
                bool isUpdateStatus = false;
                //var personData = _repo.FirstOrDefault<Person>(x => x.ID == PersonID);
                if (separations.IsTermination == 1)
                {
                    var personData = service.All<Person>().Where(x => x.ID == PersonID && x.Active == true).First();
                    var newpersonDetl = new Person
                    {
                        Active = false
                    };

                    isUpdateStatus = service.Update<Person>(newpersonDetl, personData);

                    if (isUpdateStatus)
                        service.Finalize(true);

                    isUpdateStatus = false;
                }

                //PhoenixEntities dbContext = new PhoenixEntities();
                //var personDetl = dbContext.PersonEmployment.Where(t => t.PersonID == PersonID).First();               
                SeperationProcess separationProcess = new SeperationProcess();
                using (var db1 = new PhoenixEntities())
                {

                    var separationData = db1.Separation.Where(x => x.PersonID == PersonID && x.StatusID == 3).ToList();
                    if (separationData.Count > 0)
                    {
                        int sepID = separationData[0].ID;
                        separationProcess = db1.SeperationProcess.FirstOrDefault(x => x.SeperationID == sepID && hrRoles.Contains(x.RoleID.Value));
                        separationData[0].StatusID = 4;
                        db1.SaveChanges();
                        List<int> roleIds = new List<int>();
                        var _separationProcess = db1.SeperationProcess.Where(x => x.SeperationID == sepID).ToList();
                       
                        _separationProcess.ForEach(item =>
                         roleIds.Add(item.RoleID.Value));

                        string roles = string.Join(",", roleIds.ToArray());
                        //separations.StatusID = 5; //to get sub. and msg while document generation  
                        if(separations.TerminationReason != 6) // No resignation process mail in case of Contract conversion
                        {
                            if (separations.IsTermination == 0)
                            {
                                var str = SendResignationProcessEmails(separations, "", "", roles, 9, UserId);
                            }
                            else
                            {
                                var str = SendResignationProcessEmails(separations, "", "", roles, 16, UserId);
                            }
                        }                       
                    }

                    var approvalData = db1.Approval.Where(x => x.RequestID == separationProcess.ID && x.RequestType == 11).ToList();
                    int reqID = approvalData[0].ID;
                    var approvalDetailData = db1.ApprovalDetail.Where(i => i.ApprovalID == reqID).ToList();
                    int apprID = Convert.ToInt32(approvalDetailData[0].ApproverID);

                    UpdateApproval(separationProcess, apprID); //UserId --14/01/2018
                }
                //To inactivate employee while printing letters in case of HR Separation otherwise it will done on Exit Interview closed by HR 
                if (separations.IsTermination == 1)
                {
                    var personDetl = service.All<PersonEmployment>().Where(x => x.PersonID == PersonID).First();
                    var newpersonDetl1 = new PersonEmployment
                    {
                        OrganizationEmail = "old_" + personDetl.OrganizationEmail,
                        UserName = "old_" + personDetl.UserName,
                        ExitDate = separations.ApprovalDate
                    };

                    isUpdateStatus = service.Update<PersonEmployment>(newpersonDetl1, personDetl);

                    if (isUpdateStatus)
                    {
                        service.Finalize(true);
                        int candidateID = Convert.ToInt32(personDetl.CandidateID);
                        _CandidateService.UpdateCandidateStatus(candidateID);
                        _ResourceAllocationService.DeleteRAData(PersonID, separations.ApprovalDate);
                        _RRFService.SwapRRFOwner(PersonID);
                    }


                    //Changes done to remove details from ASPNetUser and ASPLogins tables
                    PhoenixEntities entities = new PhoenixEntities();
                    entities.DeleteLogInDetails(PersonID);
                }

                result.isActionPerformed = true;
                result.message = string.Format("Employee Inacticated Successfully");
            }
            catch
            {
                result.isActionPerformed = true;
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
                using (var db = _phoenixEntity)
                {
                    Separation dbModel = db.Separation.Where(x => x.ID == model.ID && x.PersonID == model.PersonID).SingleOrDefault();
                    dbModel.UpdatedBy = model.PersonID;
                    dbModel.UpdatedOn = DateTime.Now;
                    dbModel.StatusID = 1;

                    model.StatusID = 1;

                    db.SaveChanges();
                    //if (dbModel != null)
                    //{
                    //    db.Entry(dbModel).CurrentValues.SetValues(Mapper.Map<SeperationViewModel, Separation>(model));
                    //    db.SaveChanges();
                    //}

                    //To update SeperationProcess table for Withdraw process
                    SeperationProcess separationProcess = db.SeperationProcess.FirstOrDefault(x => x.SeperationID == model.ID);

                    //To update ApprovalDetail table for Withdraw process
                    //var _ReleaseRequestType = Convert.ToInt32(db.Approval.Where(x => x.RequestID == model.ID && x.RequestBy == model.PersonID).First().RequestType);
                    //var _ReleaseRequestType = db.Approval.Where(x => (separationProcess == null ? x.RequestID == model.ID : x.RequestID == separationProcess.ID) && x.RequestBy == model.PersonID).First().RequestType;
                    int _RequestType = 0;

                    if (separationProcess != null)
                        _RequestType = 11;

                    ApprovalService service1 = new ApprovalService(this.service);

                    //To update SeperationProcess table for Withdraw process
                    if (_RequestType == 11)
                    {
                        var resultData = db.SeperationProcess.Where(x => x.SeperationID == model.ID).ToList();
                        List<int> approvalID = new List<int>();
                        List<int> roleIds = new List<int>();

                        if (resultData != null)
                        {
                            foreach (var item in resultData)
                            {
                                item.Comments = "Withdraw";
                                item.StatusID = 2;
                                item.UpdatedBy = model.PersonID;
                                item.UpdatedOn = DateTime.Now;
                                db.SaveChanges();
                                approvalID.Add(item.ID);
                                roleIds.Add(item.RoleID.Value);
                            }
                        }

                        foreach (var item in approvalID)
                        {
                            var tempid = db.Approval.Where(a => a.RequestID == item && a.RequestBy == model.PersonID && a.RequestType == _RequestType).First().ID;
                            int approverID = Convert.ToInt32(db.ApprovalDetail.Where(x => x.ApprovalID == tempid).First().ApproverID);
                            var data = service1.UpdateMultiLevelApproval(model.PersonID, _RequestType, item, 2, "Withdraw", approverID);
                        }


                        string roles = string.Join(",", roleIds.ToArray());
                        var str = SendResignationProcessEmails(model, "", "", roles, 5, dbModel.ApprovalID);
                    }
                }
                result.isActionPerformed = true;
                result.message = string.Format("Separation detail Updated Successfully");
            }
            catch
            {
                result.isActionPerformed = true;
                result.message = string.Format("Action Failed");
            }
            return result;
        }

        //public ActionResult WithdrawalReject(int id, int userID, int personID)
        //{
        //    ActionResult result = new ActionResult();
        //    using (var db = _phoenixEntity)
        //    {
        //        var seperation = db.Separation.Where(x => x.ID == id).FirstOrDefault();
        //        if (seperation != null)
        //        {
        //            seperation.StatusID = 5;
        //            seperation.UpdatedBy = userID;
        //            seperation.UpdatedOn = DateTime.Now;
        //            db.SaveChanges();
        //            result.isActionPerformed = true;
        //            result.message = "Record Saved";
        //        }
        //    }
        //    return result;
        //}
        #endregion

        public async Task<HttpResponseMessage> GenerateTerminationDocument(int letterType, int separationId, int UserId)
        {
            SeperationViewModel separations = new SeperationViewModel();
            var personSeparation = _repo.FirstOrDefault<Separation>(t => (separationId == 0 || t.PersonID == separationId));
            if (personSeparation != null)
            {
                separations = Mapper.Map<Separation, SeperationViewModel>(personSeparation);
                separations.EmployeeProfile = Mapper.Map<Person, EmployeeBasicProfile>(personSeparation.Person1);
            }

            if (separations != null)
            {

                string reportName = string.Empty;
                //List<int> letters = new List<int>();
                //letters.Add(1);
                //letters.Add(2);

                //foreach (var item in letters)
                //{
                switch (letterType)
                {
                    case 1:
                        reportName = "NDALetter";
                        break;
                    case 2:
                        reportName = "ShowCauseNotice2";
                        break;
                    case 3:
                        reportName = "LegaleNotice1";
                        break;
                    case 4:
                        reportName = "LegaleNotice2";
                        break;
                }

                //To update final separation process of HR role.
                //using (var db = new PhoenixEntities())
                //{
                //    SeperationProcess separationProcess = db.SeperationProcess.FirstOrDefault(x => x.RoleID == 24 && x.SeperationID == personSeparation.ID);
                //    UpdateApproval(separationProcess, UserId);
                //}

                //using (var db1 = new PhoenixEntities())
                //{
                //    var seperation = db1.Separation.Where(x => x.ID == separationId).FirstOrDefault();
                //    //seperation.StatusID = 4;
                //    seperation.UpdatedBy = UserId;
                //    seperation.UpdatedOn = DateTime.Now;
                //    db1.SaveChanges();
                //}

                ////To inactive employee through normal resignation flow.
                //var data = EmployeeInactive(separations.EmployeeProfile.ID, UserId, separations);

                //To print letters
                return await _PrintReport.GetTerminationPDFPrint(_repo, separationId, reportName);
                //}
            }
            return null;
        }

        #region ZipFileDownload
        public string DownloadFiles(int separationId)
        {
            string _filePath = "";

            using (var db = new PhoenixEntities())
            {
                var data = db.Separation.Where(x => x.ID == separationId).ToList();

                if (data[0].AttachedFile.Contains(@"|"))
                {

                    string[] values = data[0].AttachedFile.ToString().Split("|".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    _filePath = Path.GetDirectoryName(values[0].ToString());
                }
                else
                    _filePath = Path.GetDirectoryName(data[0].AttachedFile.ToString());

                string uploadFolder = ConfigurationManager.AppSettings["SeparationFileFolder"].ToString();
                if (Directory.Exists((uploadFolder + @"\" + data[0].PersonID)))
                {
                    if (!System.IO.File.Exists((uploadFolder + @"\" + data[0].PersonID + @"\" + data[0].PersonID + ".zip")))
                    {
                        using (var zip = new Ionic.Zip.ZipFile())
                        {
                            zip.AddDirectory(uploadFolder + @"\" + data[0].PersonID);
                            zip.Save(uploadFolder + @"\" + data[0].PersonID + @"\" + data[0].PersonID + ".zip");
                        }
                    }
                    _filePath = Path.Combine(uploadFolder + @"\" + data[0].PersonID + @"\" + data[0].PersonID + ".zip");
                }
                //zipfilepath = GetRecipientDocumentZip(_filePath, data[0].PersonID);//GetRecipientDocumentZip(_filePath, data[0].PersonID);
            }

            return _filePath;//zipfilepath;
            //return true;
        }

        #endregion

        public string GetLogInUser(int personID)
        {
            PhoenixEntities entites = new PhoenixEntities();
            Person personData = entites.People.Where(x => x.ID == personID).FirstOrDefault();
            string LogInUser = personData.FirstName + " " + personData.LastName;
            return LogInUser;
        }

        string GetHRGroupEmailIds()
        {
            //string emails = string.Empty;
            //PhoenixEntities context = new PhoenixEntities();
            //var hrGroupRoleId = Convert.ToInt32(ConfigurationManager.AppSettings["HRGroupRoleId"]);
            //emails = context.HelpDeskCategories.Where(t => t.ID == hrGroupRoleId).Select(t => t.EmailGroup).FirstOrDefault();
            //return emails;
            string emails = string.Empty;
            PhoenixEntities context = new PhoenixEntities();
            string hrGroupRoleId = (ConfigurationManager.AppSettings["HRGroupForSeparation"]).ToString();
            emails = context.HelpDeskCategories.Where(t => t.Prefix == hrGroupRoleId).Select(t => t.EmailGroup).FirstOrDefault();
            return emails;
        }

        public string GetDeptName(int roleID)
        {

            if (roleID == 12 || roleID == 24 || roleID == 38)
                return "HR";
            else if (roleID == 27 || roleID == 35)
                return "RMG";
            else if (roleID == 0)
                return "Exit Process Manager";
            else if (roleID == 25 || roleID == 37)
                return "IT";
            else if (roleID == 23 || roleID == 33)
                return "Finance";
            else if (roleID == 22 || roleID == 31 || roleID == 1)
                return "Admin";
            else if (roleID == 28 || roleID == 34)
                return "Internal";
            else if (roleID == 29 || roleID == 32)
                return "CQ";
            else if (roleID == 30 || roleID == 36)
                return "VWR";
            else if (roleID == 41 || roleID == 42)
                return "V2Hub";
            else
                return "";
        }

        #region Update Exit Date
        //public ActionResult ExitDateUpdate(ChangeReleaseDateViewModel model, int userID, int isDateChange)
        //{
        //    ActionResult result = new ActionResult();
        //    try
        //    {
        //        using (var db = new PhoenixEntities())
        //        {
        //            string logInUserName = GetLogInUser(userID);

        //            var separationData = db.Separation.Where(x => x.ID == model.SeparationID).ToList();
        //            if (isDateChange == 1)
        //            {
        //                separationData[0].ApprovalDate = model.ExitDate;
        //                separationData[0].ExitDateRemark = separationData[0].ExitDateRemark + "<b>" + logInUserName + "</b> :<br/>" + DateTime.Now.Date.ToShortDateString() + " : <b>" + model.ExitDateRemark + "</b><br/>";
        //                separationData[0].UpdatedBy = userID;
        //                separationData[0].UpdatedOn = DateTime.Now;

        //                db.SaveChanges();
        //            }

        //            string logInUserEmail = GetPersonEmail(userID);
        //            string personEmail = GetPersonEmail(separationData[0].PersonID);
        //            string ForUserName = GetLogInUser(separationData[0].PersonID);
        //            string sub = "Agreed release date for " + ForUserName + " (" + separationData[0].PersonID + ") " + " has been changed";

        //            SeperationViewModel separations = new SeperationViewModel();
        //            var personSeparation = _repo.FirstOrDefault<Separation>(t => t.ID == model.SeparationID);
        //            separations = Mapper.Map<Separation, SeperationViewModel>(personSeparation);
        //            separations.EmployeeProfile = Mapper.Map<Person, EmployeeBasicProfile>(separationData.Where(x => x.ID == model.SeparationID).First().Person1);
        //            separations.LWPDate = model.LWPDate;

        //            if (isDateChange == 1 && model.Type == 1)
        //                _EmailService.SendResignationEmail(separations, "Notice period extended due to LWP", "Notice period extended due to LWP", logInUserEmail, personEmail, false, false, 11, "", logInUserName);
        //            else if (isDateChange == 1 && model.Type == 2)
        //                _EmailService.SendResignationEmail(separations, sub, "Agreed release date has been changed", logInUserEmail, personEmail, false, false, 13, "", logInUserName);
        //            else if (isDateChange == 0)
        //                _EmailService.SendResignationEmail(separations, "Release date updated for " + ForUserName + " (" + separationData[0].PersonID + ")", "Notice period not extended after taking LWP", logInUserEmail, personEmail, false, false, 12, "", logInUserName);

        //        }
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
            //ActionResult result = new ActionResult();
            SeperationTerminationViewModel result = new SeperationTerminationViewModel();
            try
            {
                using (var db = new PhoenixEntities())
                {
                    string logInUserName = GetLogInUser(userID);

                    var separationData = db.Separation.Where(x => x.ID == model.SeparationID).ToList();
                    if (isDateChange == 1)
                    {
                        separationData[0].ApprovalDate = model.ExitDate;
                        separationData[0].ExitDateRemark = separationData[0].ExitDateRemark + "<b>" + logInUserName + "</b> :<br/>" + DateTime.Now.Date.ToShortDateString() + " : <b>" + model.ExitDateRemark + "</b><br/>";
                        separationData[0].UpdatedBy = userID;
                        separationData[0].UpdatedOn = DateTime.Now;

                        db.SaveChanges();
                    }

                    string logInUserEmail = GetPersonEmail(userID);
                    string personEmail = GetPersonEmail(separationData[0].PersonID);
                    string ForUserName = GetLogInUser(separationData[0].PersonID);
                    string sub = "Agreed release date for " + ForUserName + " (" + separationData[0].PersonID + ") " + " has been changed";

                    SeperationViewModel separations = new SeperationViewModel();
                    var personSeparation = _repo.FirstOrDefault<Separation>(t => t.ID == model.SeparationID);
                    separations = Mapper.Map<Separation, SeperationViewModel>(personSeparation);
                    separations.EmployeeProfile = Mapper.Map<Person, EmployeeBasicProfile>(separationData.Where(x => x.ID == model.SeparationID).First().Person1);
                    separations.LWPDate = model.LWPDate;

                    if (isDateChange == 1 && model.Type == 1)
                    {
                        result = _EmailService.GetResignationEmail(separations, "Notice period extended due to LWP", "Notice period extended due to LWP", logInUserEmail, personEmail, false, false, 11, "", logInUserName);

                    }
                    else if (isDateChange == 1 && model.Type == 2)
                    {
                        result = _EmailService.GetResignationEmail(separations, sub, "Agreed release date has been changed", logInUserEmail, personEmail, false, false, 13, "", logInUserName);

                    }
                    else if (isDateChange == 0)
                    {
                        result = _EmailService.GetResignationEmail(separations, "Release date updated for " + ForUserName + " (" + separationData[0].PersonID + ")", "Notice period not extended after taking LWP", logInUserEmail, personEmail, false, false, 12, "", logInUserName);

                    }
                }
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
        #endregion
        public async Task<IEnumerable<SeperationViewModel>> GetSeperationListHistory(int userId, Boolean isHR)
        {
            try
            {
                var seperationModel = new List<SeperationViewModel>();
                await Task.Run(() =>
                {
                    using (var _db = _phoenixEntity)
                    {
                        if (!isHR)
                        {
                            var approvalIds = GetMySeperationApprovals(userId, 10);

                            //var resultData = _db.Seperation.Where(s => s.StatusID == 0 && approvalIds.Contains(s.ID)).ToList();
                            var resultData = _db.Separation.Where(s => (s.StatusID != 1 && s.StatusID != 5 && s.StatusID >= 2) && approvalIds.Contains(s.ID) && s.IsTermination == 0).ToList();

                            if (resultData != null && resultData.Count > 0)
                            {
                                seperationModel = Mapper.Map<List<Separation>, List<SeperationViewModel>>(resultData);
                                foreach (var item in seperationModel)
                                {
                                    //if (!isHR && string.IsNullOrEmpty(Convert.ToString(item.ApprovalDate)))
                                    if (!isHR && item.ApprovalDate == default(DateTime))
                                    {
                                        item.isApprovedByHR = false;
                                    }
                                    //else if (isHR && string.IsNullOrEmpty(Convert.ToString(item.ApprovalDate)))
                                    else if (isHR && item.ApprovalDate == default(DateTime))
                                        item.isApprovedByHR = false;
                                    else
                                        item.isApprovedByHR = true;

                                    //if (!string.IsNullOrEmpty(Convert.ToString(item.ApprovalDate)) && item.WithdrawRemark != null && item.WithdrawRemark != "")
                                    if (item.ApprovalDate != default(DateTime) && item.WithdrawRemark != null && item.WithdrawRemark != "")
                                        item.isWithdraw = true;
                                    else
                                        item.isWithdraw = false;

                                    item.EmployeeProfile = Mapper.Map<Person, EmployeeBasicProfile>(resultData.Where(x => x.ID == item.ID).First().Person1);
                                    //item.NoticePeriod = GetNoticePeriod(item.PersonID); //Commented on 06/02/2018 Now NP will get fetch from Separation table

                                    //var dt= item.ActualDate.ToString("dddd dd MMMM",System.Globalization.CultureInfo.CreateSpecificCulture("en-US"));
                                    //item.ActualDate =Convert.ToDateTime(dt);

                                    //if (string.IsNullOrEmpty(Convert.ToString(item.ApprovalDate)))
                                    if (item.ApprovalDate == default(DateTime))
                                        item.ApprovalDate = item.ActualDate;

                                    //**********************Leave Detail********************///
                                    var empLeavesTaken = service.Top<PersonLeaveLedger>(0, a => (item.PersonID == -1 || a.Person.ID == item.PersonID) && a.Year == DateTime.Now.Year).ToList();
                                    AvailableLeaves availLeaves = new AvailableLeaves();
                                    //using (PhoenixEntities context = new PhoenixEntities())
                                    //{
                                    var leaveConsumed = _db.GetLeaveData(item.PersonID, DateTime.Now.Year).ToList();
                                    if (empLeavesTaken.Count() != 0)
                                    {
                                        //availLeaves = new AvailableLeaves()
                                        //{
                                        item.TotalLeaves = empLeavesTaken[0].OpeningBalance + (leaveConsumed.First().CreditLeaves ?? 0);
                                        item.CompOff = empLeavesTaken[0].CompOffs;
                                        item.LeavesTaken = leaveConsumed.First().LeavesConsumed ?? 0;
                                        item.LeavesApplied = leaveConsumed.First().LeavesApplied ?? 0;
                                        item.CompOffAvailable = leaveConsumed.First().CompOffAvailable ?? 0;
                                        item.LWP = leaveConsumed.First().LWPApplied ?? 0;
                                        item.CompOffConsumed = leaveConsumed.First().CompOffConsumed ?? 0;
                                        //};
                                        item.LeavesAvailable = item.TotalLeaves - item.LeavesTaken - item.LeavesApplied;
                                    }
                                    //}
                                    //return availLeaves;
                                }
                            }
                        }
                    }
                });

                return seperationModel;
            }
            catch (Exception ex)
            {
                throw new HttpException(500, ex.ToString());
            }
        }

        List<int> GetMySeperationApprovals(int userId, int requestType)
        {
            var approvalList = QueryHelper.GetMyApprovedSeparationDetl(userId, requestType);
            return approvalList;
        }

        public string GetPersonalEmail(int personID)
        {
            using (PhoenixEntities context = new PhoenixEntities())
            {
                var personData = context.PersonPersonals.Where(x => x.PersonID == personID).FirstOrDefault();
                string email = personData.PersonalEmail;
                return email;
            }
        }

        #region Send Show Cause Notice-2
        public SeperationTerminationViewModel ShowCauseNotice2(EmailTemplateViewModel model, int type, int separationprocessid, int userId)
        {
            SeperationTerminationViewModel result = new SeperationTerminationViewModel();

            try
            {
                var sepProcessData = service.Top<SeperationProcess>(0, x => x.ID == separationprocessid).ToList();
                int _separationID = sepProcessData[0].SeperationID.Value;
                var separationData = service.First<Separation>(t => t.ID == _separationID);

                SeperationViewModel separation = Mapper.Map<Separation, SeperationViewModel>(separationData);

                separation.EmployeeProfile = Mapper.Map<Person, EmployeeBasicProfile>(separationData.Person1);

                //separation.NoticePeriod = GetNoticePeriod(separationData.PersonID); //Commented on 06/02/2018 Now NP will get fetch from Separation table

                string personEmail = GetPersonEmail(separationData.PersonID);
                string empPersonalEmail = GetPersonalEmail(separationData.PersonID);
                string logInUser = GetLogInUser(userId);
                string logInUserEmail = GetPersonEmail(userId);

                string subject = "Show Cause Notice – 2"; //+ " for " + separation.EmployeeProfile.FirstName + " " + separation.EmployeeProfile.LastName;//GetEmailSubject(4);

                int _currStatus = 0;

                switch (type)
                {
                    case 1:
                        _currStatus = 20;
                        break;
                    case 2:
                        _currStatus = 21;
                        break;
                    case 3:
                        _currStatus = 22;
                        break;
                    case 4:
                        _currStatus = 23;
                        break;
                    case 5:
                        _currStatus = 24;
                        break;
                }
                result = _EmailService.GetResignationEmail(separation, subject, model.ShowCauseNoticeSendOn.ToString() + "," + model.EmailReceivedOn.ToString() + "," + model.Reason, logInUserEmail, empPersonalEmail, false, false, _currStatus, "", logInUser);
                result.isActionPerformed = true;
                result.message = string.Format("Show cause notice-2 send Successfully");
            }
            catch
            {
                result.isActionPerformed = false;
                result.message = string.Format("Action Failed");
            }
            return result;
        }
        #endregion

        #region Exit Process Form
        public ActionResult AddExitForm(ExitProcessFormDetailViewModel model, int separationID, int UserID)
        {
            ActionResult result = new ActionResult();

            try
            {
                FeedbackForLeavingOrg _FeedbackForLeavingOrg = new FeedbackForLeavingOrg();
                RatingForReportingLead _RatingForReportingLead = new RatingForReportingLead();
                RatingForOrganization _RatingForOrganization = new RatingForOrganization();
                OrganizationDevelopmentSuggestion _OrganizationDevelopmentSuggestion = new OrganizationDevelopmentSuggestion();
                ExitFormEmployeeDeclaration _ExitFormEmployeeDeclaration = new ExitFormEmployeeDeclaration();

                string personEmail = "";
                string logInUser = GetLogInUser(UserID);
                string logInUserEmail = GetPersonEmail(UserID);

                using (var db = _phoenixEntity)
                {
                    if (model.ID == 0)
                    {
                        if (model.LeavingOrgData != null)
                        {
                            if (model.LeavingOrgData.ID == 0)
                            {
                                _FeedbackForLeavingOrg = Mapper.Map<FeedbackForLeavingOrgViewModel, FeedbackForLeavingOrg>(model.LeavingOrgData);
                                _FeedbackForLeavingOrg.CreatedBy = UserID;
                                _FeedbackForLeavingOrg.CreatedOn = DateTime.Now;
                                _FeedbackForLeavingOrg.SeparationID = separationID;
                                db.FeedbackForLeavingOrg.Add(_FeedbackForLeavingOrg);
                            }
                            else
                            {
                                FeedbackForLeavingOrg dbModel = db.FeedbackForLeavingOrg.Where(x => x.ID == model.LeavingOrgData.ID && x.SeparationID == model.LeavingOrgData.SeparationID).SingleOrDefault();
                                if (dbModel != null)
                                {
                                    db.Entry(dbModel).CurrentValues.SetValues(Mapper.Map<FeedbackForLeavingOrgViewModel, FeedbackForLeavingOrg>(model.LeavingOrgData));
                                    db.SaveChanges();
                                }
                            }
                        }

                        if (model.ReportingLeadData != null)
                        {
                            if (model.ReportingLeadData.ID == 0)
                            {
                                _RatingForReportingLead = Mapper.Map<RatingForReportingLeadViewModel, RatingForReportingLead>(model.ReportingLeadData);
                                _RatingForReportingLead.CreatedBy = UserID;
                                _RatingForReportingLead.CreatedOn = DateTime.Now;
                                _RatingForReportingLead.SeparationID = separationID;
                                db.RatingForReportingLead.Add(_RatingForReportingLead);
                            }
                            else
                            {
                                RatingForReportingLead dbModel = db.RatingForReportingLead.Where(x => x.ID == model.ReportingLeadData.ID && x.SeparationID == model.ReportingLeadData.SeparationID).SingleOrDefault();
                                if (dbModel != null)
                                {
                                    db.Entry(dbModel).CurrentValues.SetValues(Mapper.Map<RatingForReportingLeadViewModel, RatingForReportingLead>(model.ReportingLeadData));
                                    db.SaveChanges();
                                }
                            }
                        }

                        if (model.OrgRatingData != null)
                        {
                            if (model.OrgRatingData.ID == 0)
                            {
                                _RatingForOrganization = Mapper.Map<RatingForOrganizationViewModel, RatingForOrganization>(model.OrgRatingData);
                                _RatingForOrganization.CreatedBy = UserID;
                                _RatingForOrganization.CreatedOn = DateTime.Now;
                                _RatingForOrganization.SeparationID = separationID;
                                db.RatingForOrganization.Add(_RatingForOrganization);
                            }
                            else
                            {
                                RatingForOrganization dbModel = db.RatingForOrganization.Where(x => x.ID == model.OrgRatingData.ID && x.SeparationID == model.OrgRatingData.SeparationID).SingleOrDefault();
                                if (dbModel != null)
                                {
                                    db.Entry(dbModel).CurrentValues.SetValues(Mapper.Map<RatingForOrganizationViewModel, RatingForOrganization>(model.OrgRatingData));
                                    db.SaveChanges();
                                }
                            }
                        }

                        if (model.SuggestionData != null)
                        {
                            if (model.SuggestionData.ID == 0)
                            {
                                _OrganizationDevelopmentSuggestion = Mapper.Map<OrgDevelopmentSuggestionViewModel, OrganizationDevelopmentSuggestion>(model.SuggestionData);
                                _OrganizationDevelopmentSuggestion.CreatedBy = UserID;
                                _OrganizationDevelopmentSuggestion.CreatedOn = DateTime.Now;
                                _OrganizationDevelopmentSuggestion.SeparationID = separationID;
                                db.OrganizationDevelopmentSuggestion.Add(_OrganizationDevelopmentSuggestion);
                            }
                            else
                            {
                                OrganizationDevelopmentSuggestion dbModel = db.OrganizationDevelopmentSuggestion.Where(x => x.ID == model.SuggestionData.ID && x.SeparationID == model.SuggestionData.SeparationID).SingleOrDefault();
                                if (dbModel != null)
                                {
                                    db.Entry(dbModel).CurrentValues.SetValues(Mapper.Map<OrgDevelopmentSuggestionViewModel, OrganizationDevelopmentSuggestion>(model.SuggestionData));
                                    db.SaveChanges();
                                }
                            }
                        }

                        if (model.EmployeeDeclarationData != null)
                        {
                            if (model.EmployeeDeclarationData.ID == 0)
                            {
                                _ExitFormEmployeeDeclaration = Mapper.Map<ExitFormEmployeeDeclarationViewModel, ExitFormEmployeeDeclaration>(model.EmployeeDeclarationData);
                                _ExitFormEmployeeDeclaration.CreatedBy = UserID;
                                _ExitFormEmployeeDeclaration.CreatedOn = DateTime.Now;
                                _ExitFormEmployeeDeclaration.SeparationID = separationID;
                                db.ExitFormEmployeeDeclaration.Add(_ExitFormEmployeeDeclaration);
                            }
                            else
                            {
                                ExitFormEmployeeDeclaration dbModel = db.ExitFormEmployeeDeclaration.Where(x => x.ID == model.EmployeeDeclarationData.ID && x.SeparationID == model.EmployeeDeclarationData.SeparationID).SingleOrDefault();
                                if (dbModel != null)
                                {
                                    db.Entry(dbModel).CurrentValues.SetValues(Mapper.Map<ExitFormEmployeeDeclarationViewModel, ExitFormEmployeeDeclaration>(model.EmployeeDeclarationData));
                                    db.SaveChanges();
                                }
                            }
                        }

                        db.SaveChanges();

                        if (model.LeavingOrgData.ID != 0 && model.ReportingLeadData.ID != 0 && model.OrgRatingData.ID != 0 && model.SuggestionData.ID != 0 && _ExitFormEmployeeDeclaration.ID != 0)
                        {
                            ExitProcessFormViewModel _ExitProcessFormViewModel = new ExitProcessFormViewModel();
                            _ExitProcessFormViewModel.SeparationID = separationID;
                            _ExitProcessFormViewModel.FeedbackForLeavingOrg = model.LeavingOrgData.ID;
                            _ExitProcessFormViewModel.RatingForReportingLead = model.ReportingLeadData.ID;
                            _ExitProcessFormViewModel.RatingForOrganization = model.OrgRatingData.ID;
                            _ExitProcessFormViewModel.OrganizationDevelopmentSuggestion = model.SuggestionData.ID;
                            _ExitProcessFormViewModel.EmployeeDeclaration = _ExitFormEmployeeDeclaration.ID;
                            _ExitProcessFormViewModel.CreatedBy = UserID;
                            _ExitProcessFormViewModel.CreatedOn = DateTime.Now;
                            _ExitProcessFormViewModel.UpdatedBy = UserID;
                            _ExitProcessFormViewModel.UpdatedOn = DateTime.Now;
                            _ExitProcessFormViewModel.IsHRReviewDone = false;

                            PersonExitProcessForm _PersonExitProcessForm = Mapper.Map<ExitProcessFormViewModel, PersonExitProcessForm>(_ExitProcessFormViewModel);
                            db.PersonExitProcessForm.Add(_PersonExitProcessForm);

                            db.SaveChanges();

                            int _personID = db.Separation.FirstOrDefault(x => x.ID == separationID).PersonID;
                            personEmail = GetPersonEmail(_personID);
                            int leaveID = 0;
                            _EmailService.SendResignationEmail(null, "Exit Interview Feedback Form Submitted", _personID.ToString() + "," + leaveID.ToString(), logInUserEmail, GetHRGroupEmailIds(), false, false, 25, "", logInUser);
                        }
                    }
                    else //Exit interview form completed by HR
                    {
                        ExitProcessFormViewModel _ExitProcessFormViewModel = Mapper.Map<ExitProcessFormViewModel, ExitProcessFormViewModel>(model.ExitProcessFormLinking);
                        _ExitProcessFormViewModel.UpdatedBy = UserID;
                        _ExitProcessFormViewModel.UpdatedOn = DateTime.Now;

                        PersonExitProcessForm dbModel = db.PersonExitProcessForm.Where(x => x.ID == model.ID && x.SeparationID == model.SeperationID).SingleOrDefault();
                        dbModel.UpdatedBy = UserID;
                        dbModel.UpdatedOn = DateTime.Now;


                        if (dbModel != null)
                        {
                            db.Entry(dbModel).CurrentValues.SetValues(Mapper.Map<ExitProcessFormViewModel, PersonExitProcessForm>(_ExitProcessFormViewModel));
                            db.SaveChanges();
                        }

                        db.SaveChanges();

                        if (_ExitProcessFormViewModel.IsHRReviewDone)
                        {
                            int _personID = db.Separation.FirstOrDefault(x => x.ID == separationID).PersonID;
                            int leaveID = 0;
                            personEmail = GetPersonEmail(_personID);
                            _EmailService.SendResignationEmail(null, "Exit Stage Completed", _personID.ToString() + "," + leaveID.ToString(), logInUserEmail, personEmail, false, false, 26, "", logInUser);

                            //To inactivate employee while printing letters in case of HR Separation otherwise it will done on Exit Interview closed by HR 
                            Boolean isUpdateStatus = false;

                            var personDetl = service.All<PersonEmployment>().Where(x => x.PersonID == _personID).First();
                            var newpersonDetl1 = new PersonEmployment
                            {
                                OrganizationEmail = "old_" + personDetl.OrganizationEmail,
                                UserName = "old_" + personDetl.UserName,
                                ExitDate = db.Separation.FirstOrDefault(x => x.ID == separationID).ApprovalDate
                            };

                            isUpdateStatus = service.Update<PersonEmployment>(newpersonDetl1, personDetl);

                            if (isUpdateStatus)
                            {
                                service.Finalize(true);
                                int candidateID = Convert.ToInt32(personDetl.CandidateID);
                                _CandidateService.UpdateCandidateStatus(candidateID);
                                _ResourceAllocationService.DeleteRAData(_personID, newpersonDetl1.ExitDate ?? DateTime.MaxValue);
                                _RRFService.SwapRRFOwner(_personID);
                            }

                            //To inactivate employee in Person table
                            var personData = service.All<Person>().Where(x => x.ID == _personID && x.Active == true).First();
                            var newpersonDetl = new Person
                            {
                                Active = false
                            };

                            isUpdateStatus = service.Update<Person>(newpersonDetl, personData);

                            if (isUpdateStatus)
                                service.Finalize(true);

                            isUpdateStatus = false;

                            //Changes done to remove details from ASPNetUser and ASPLogins tables
                            PhoenixEntities entities = new PhoenixEntities();
                            entities.DeleteLogInDetails(_personID);
                        }
                    }
                }
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
            try
            {
                return await Task.Run(() =>
                {
                    ExitProcessFormDetailViewModel objExitProcessForm = new ExitProcessFormDetailViewModel();
                    using (var db = _phoenixEntity)
                    {
                        FeedbackForLeavingOrg _FeedbackForLeavingOrgData = db.FeedbackForLeavingOrg.FirstOrDefault(x => x.SeparationID == separationID);
                        RatingForReportingLead _RatingForReportingLeadData = db.RatingForReportingLead.FirstOrDefault(x => x.SeparationID == separationID);
                        RatingForOrganization _RatingForOrganizationData = db.RatingForOrganization.FirstOrDefault(x => x.SeparationID == separationID);
                        OrganizationDevelopmentSuggestion _OrganizationSuggestionData = db.OrganizationDevelopmentSuggestion.FirstOrDefault(x => x.SeparationID == separationID);
                        ExitFormEmployeeDeclaration _EmployeeDeclarationData = db.ExitFormEmployeeDeclaration.FirstOrDefault(x => x.SeparationID == separationID);
                        PersonExitProcessForm _formData = db.PersonExitProcessForm.FirstOrDefault(x => x.SeparationID == separationID);

                        if (_FeedbackForLeavingOrgData != null)
                            objExitProcessForm.LeavingOrgData = Mapper.Map<FeedbackForLeavingOrg, FeedbackForLeavingOrgViewModel>(_FeedbackForLeavingOrgData);

                        if (_RatingForReportingLeadData != null)
                            objExitProcessForm.ReportingLeadData = Mapper.Map<RatingForReportingLead, RatingForReportingLeadViewModel>(_RatingForReportingLeadData);

                        if (_RatingForOrganizationData != null)
                            objExitProcessForm.OrgRatingData = Mapper.Map<RatingForOrganization, RatingForOrganizationViewModel>(_RatingForOrganizationData);

                        if (_OrganizationSuggestionData != null)
                            objExitProcessForm.SuggestionData = Mapper.Map<OrganizationDevelopmentSuggestion, OrgDevelopmentSuggestionViewModel>(_OrganizationSuggestionData);

                        if (_EmployeeDeclarationData != null)
                            objExitProcessForm.EmployeeDeclarationData = Mapper.Map<ExitFormEmployeeDeclaration, ExitFormEmployeeDeclarationViewModel>(_EmployeeDeclarationData);

                        if (_formData != null)
                        {
                            objExitProcessForm.ExitProcessFormLinking = Mapper.Map<PersonExitProcessForm, ExitProcessFormViewModel>(_formData);
                            objExitProcessForm.ID = _formData.ID;
                        }

                        objExitProcessForm.SeperationID = separationID;// _formData.SeparationID.Value;
                    }

                    return objExitProcessForm;
                });
            }
            catch (Exception ex)
            {
                throw new HttpException(500, ex.ToString());
            }
        }
        #endregion

        public DateTime CheckDates(int personID, int days)
        {
            DateTime daytoCheck = DateTime.Now.AddDays(days);
            int location = 0;

            using (PhoenixEntities context = new PhoenixEntities())
            {
                location = context.PersonEmployment.Where(x => x.PersonID == personID).FirstOrDefault().OfficeLocation.Value;
                var holidayList = context.HolidayList.Where(x => x.Location == location);

                for (DateTime index = daytoCheck; index >= daytoCheck; index = index.AddDays(-1))
                {
                    foreach (var holiday in holidayList)
                    {
                        if (daytoCheck.Date.CompareTo(holiday.Date) == 0)
                        {
                            daytoCheck = daytoCheck.AddDays(-1);
                        }
                    }


                    if (daytoCheck.DayOfWeek == DayOfWeek.Saturday || daytoCheck.DayOfWeek == DayOfWeek.Sunday)
                    {
                        daytoCheck = daytoCheck.AddDays(-1);

                        if (daytoCheck.DayOfWeek == DayOfWeek.Saturday || daytoCheck.DayOfWeek == DayOfWeek.Sunday)
                        {
                            daytoCheck = daytoCheck.AddDays(-1);
                        }
                    }
                }
            }

            return daytoCheck;
        }

        public async Task<IEnumerable<SeperationViewModel>> GetSeperationListForRMG(string roles, int personID)
        {
            try
            {
                var seperationModel = new List<SeperationViewModel>();
                await Task.Run(() =>
                {
                    using (var _db = _phoenixEntity)
                    {
                        var resultData = _db.Separation.Where(s => s.StatusID != 1 && s.StatusID != 5 && s.StatusID != 4).OrderBy(c => c.ApprovalDate).ToList();

                        if (resultData != null && resultData.Count > 0)
                        {
                            seperationModel = Mapper.Map<List<Separation>, List<SeperationViewModel>>(resultData);
                            foreach (var item in seperationModel)
                            {

                                item.EmployeeProfile = Mapper.Map<Person, EmployeeBasicProfile>(resultData.Where(x => x.ID == item.ID).First().Person1);

                                if (item.ApprovalDate == default(DateTime))
                                    item.ApprovalDate = item.ActualDate;

                                //**********************Leave Detail********************///
                                //var empLeavesTaken = service.Top<PersonLeaveLedger>(0, a => (item.PersonID == -1 || a.Person.ID == item.PersonID) && a.Year == DateTime.Now.Year).ToList();
                                //AvailableLeaves availLeaves = new AvailableLeaves();
                                //using (PhoenixEntities context = new PhoenixEntities())
                                //{
                                //    var leaveConsumed = context.GetLeaveData(item.PersonID, DateTime.Now.Year).ToList();
                                //    if (empLeavesTaken.Count() != 0)
                                //    {
                                //        item.TotalLeaves = empLeavesTaken[0].OpeningBalance + (leaveConsumed.First().CreditLeaves ?? 0);
                                //        item.CompOff = empLeavesTaken[0].CompOffs;
                                //        item.LeavesTaken = leaveConsumed.First().LeavesConsumed ?? 0;
                                //        item.LeavesApplied = leaveConsumed.First().LeavesApplied ?? 0;
                                //        item.CompOffAvailable = leaveConsumed.First().CompOffAvailable ?? 0;
                                //        item.LWP = leaveConsumed.First().LWPApplied ?? 0;
                                //        item.CompOffConsumed = leaveConsumed.First().CompOffConsumed ?? 0;
                                //        item.LeavesAvailable = item.TotalLeaves - item.LeavesTaken - item.LeavesApplied;
                                //    }
                                //}
                            }
                        }
                    }
                });

                return seperationModel.OrderBy(x => x.ApprovalDate);
            }
            catch (Exception ex)
            {
                throw new HttpException(500, ex.ToString());
            }
        }

        
    }
}


