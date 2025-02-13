using AutoMapper;
using Pheonix.Core.v1.Services;
using Pheonix.Core.v1.Services.Email;
using Pheonix.DBContext;
using Pheonix.Models.VM;
using Pheonix.Models.VM.Classes.TalentAcqRRF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;



namespace Pheonix.Core.Repository.TARRFrequest
{
    public class TARRFRepository : ITARRFRepository
    {
        private PhoenixEntities _phoenixEntity;
        private IEmailService _EmailService;
        public TARRFRepository(IBasicOperationsService opsService, IEmailService emailService)
        {
            _phoenixEntity = new PhoenixEntities();
            _phoenixEntity.Database.Connection.Open();
            _EmailService = emailService;
        }

        public Task<bool> DeleteReq(int ReqId)
        {
            bool status = false;
            return Task.Run(() =>
            {
                using (var _db = _phoenixEntity)
                {
                    TARRF rowToDelete = _db.TARRF.Where(x => x.Id == ReqId).FirstOrDefault();
                    _db.TARRF.Remove(rowToDelete);
                    _db.SaveChanges();
                    status = true;
                }
                return status;
            });
        }

        public async Task<List<TARRFViewModel>> GetAllTalentAcqRequestsAsync()
        {
            return await Task.Run(() =>
            {
                using (var _db = _phoenixEntity)
                {
                    var TARRFlst = _db.TARRF.ToList();
                    List<TARRFViewModel> talentacqreqVM = Mapper.Map<List<TARRF>, List<TARRFViewModel>>(TARRFlst);
                    return talentacqreqVM;
                    // --- need to complete code for Skills and Interviewer  
                }
            });
        }

        public async Task<List<DropdownItems>> GetDesignationDropDown()
        {
            return await Task.Run(() =>
            {
                using (var _db = _phoenixEntity)
                {
                    var designations = _db.Designations.Where(x => x.IsDeleted == false);
                    List<DropdownItems> DesignationList = new List<DropdownItems>();
                    foreach (var item in designations)
                    {
                        DropdownItems dropdownItem = new DropdownItems
                        {
                            ID = item.ID,
                            Text = item.Name
                        };
                        DesignationList.Add(dropdownItem);
                    }
                    return DesignationList;
                }
            });
        }

        public async Task<List<DropdownItems>> GetJobDescriptionDropDown()
        {
            return await Task.Run(() =>
            {
                using (var _db = _phoenixEntity)
                {
                    var jobdescriptionlst = _db.Jobdescriptions.ToList();
                    List<DropdownItems> jdlist = new List<DropdownItems>();
                    foreach (var item in jobdescriptionlst)
                    {
                        DropdownItems drpdownitem = new DropdownItems
                        {
                            ID = item.Id,
                            Text = item.Title,
                            PrefixText = item.Link
                        };
                        jdlist.Add(drpdownitem);
                    }
                    return jdlist;
                }
            });
        }

        public async Task<List<DropdownItems>> GetRRFApprover()
        {
            return await Task.Run(() =>
            {
                using (var _db = _phoenixEntity)
                {
                    int roleID = _db.Role.Where(r => r.Name == "RRFApprover").Select(r => r.ID).FirstOrDefault();
                    var rrfApprover = _db.PersonInRole.Where(x => x.RoleID == roleID && x.IsDeleted == false).ToList();

                    List<DropdownItems> ApproverList = new List<DropdownItems>();
                    foreach (var item in rrfApprover)
                    {
                        DropdownItems drpdownitem = new DropdownItems
                        {
                            ID = item.PersonID,
                            Text = item.Person.FirstName + " " + item.Person.LastName
                        };
                        ApproverList.Add(drpdownitem);
                    }
                    return ApproverList;
                }
            });
        }

        public async Task<List<TARRFViewModel>> GetMyTalentAcqRequestsAsync(int UserId, int isApproval, bool isHR)
        {
            List<TARRFViewModel> talentacqreqVM = null;
            return await Task.Run(() =>
            {
                using (var _db = _phoenixEntity)
                {
                    var tarrfdetail = _db.TARRFDetail.Where(x => x.Status == 0).ToList();
                    var approvertarrf = _db.TARRF.Where(x => x.PrimaryApprover == UserId && x.RRFStatus == 0 && !x.IsDraft).ToList();
                    var tarrf = new List<TARRF>();
                    if (isApproval == 1) // List of all requests pending for approval with logged in user
                    {
                        if (isHR)
                        {
                            tarrf = _db.TARRF.Where(x => x.HRApprover == 0 && x.RRFStatus != 2 && !x.IsDraft).ToList();
                            foreach (TARRFDetail item in tarrfdetail)
                            {
                                /// Check and add to list; if Recruiter is having any open request from RRF detailed (splitted) requests.
                                var hrTArrf = _db.TARRF.Where(x => x.RRFNo == item.RRFNo).First();
                                if (tarrf.IndexOf(hrTArrf) < 0)
                                    tarrf.Add(hrTArrf);
                            }
                            foreach (TARRF item in approvertarrf)
                            {
                                if (tarrf.IndexOf(item) < 0)
                                    tarrf.Add(item);
                            }
                        }
                        else
                            tarrf = approvertarrf;
                    }
                    else if (isApproval == 0) // List of all requests pending for approval created by logged in user
                        tarrf = _db.TARRF.Where(x => x.Requestor == UserId).ToList();
                    else if (isApproval == 2) // List of all approved or rejected requests created by or action taken by logged in user.
                    {
                        if (isHR)
                        {
                            tarrf = _db.TARRF.Where(x => (x.HRApprover == 0 && x.RRFStatus == 2) || (x.HRApprover > 0 && x.RRFStatus >= 1)).ToList();
                            foreach (TARRFDetail item in tarrfdetail)
                            {
                                var hrTArrf = _db.TARRF.Where(x => x.RRFNo == item.RRFNo).First();
                                tarrf.Remove(hrTArrf);
                            }
                        }
                        else
                            tarrf = _db.TARRF.Where(x => x.PrimaryApprover == UserId && x.RRFStatus > 0).ToList();
                    }

                    if (tarrf != null && tarrf.Count > 0)
                    {
                        talentacqreqVM = new List<TARRFViewModel>();
                        talentacqreqVM = Mapper.Map<List<TARRF>, List<TARRFViewModel>>(tarrf);

                        foreach (var item in talentacqreqVM)
                        {
                            List<string> skillNames = new List<string>();
                            var skillIDs = _db.TASkills.Where(x => x.RRFId == item.Id).Select(x => x.SkillId).ToList();
                            foreach (var skillid in skillIDs)
                            {
                                var skillName = _db.SkillMatrices.Where(x => x.ID == skillid).Select(x => x.Name).FirstOrDefault();
                                skillNames.Add(skillName);
                            }
                            item.SkillsName = skillNames;
                            item.SkillIds = skillIDs;

                            List<string> interviewerNames = new List<string>();
                            var intreviewersIDs = _db.TAInterviewer.Where(x => x.RRFId == item.Id).Select(X => X.PersonId).ToList();
                            if (intreviewersIDs != null)
                            {
                                foreach (var personId in intreviewersIDs)
                                {
                                    var fName = _db.People.Where(x => x.ID == personId).Select(X => X.FirstName).FirstOrDefault().ToString();
                                    var lName = _db.People.Where(x => x.ID == personId).Select(X => X.LastName).FirstOrDefault().ToString();

                                    var interviewerName = fName + " " + lName;
                                    interviewerNames.Add(interviewerName);
                                }
                                item.InterviewersName = interviewerNames;
                                item.InterviewerIds = intreviewersIDs;
                            }

                            item.DeliveryUnitName = _db.DeliveryUnit.Where(x => x.ID == item.DeliveryUnit).Select(x => x.Name).FirstOrDefault();
                            item.DesignationName = _db.Designations.Where(x => x.ID == item.Designation).Select(x => x.Name).FirstOrDefault();

                            if (item.PrimaryApprover > 0)
                            {
                                var approverFirstName = _db.People.Where(x => x.ID == item.PrimaryApprover).Select(x => x.FirstName).FirstOrDefault().ToString();
                                var approverLastName = _db.People.Where(x => x.ID == item.PrimaryApprover).Select(x => x.LastName).FirstOrDefault().ToString();
                                item.PrimaryApproverName = approverFirstName + " " + approverLastName;
                            }
                            else
                            {
                                item.PrimaryApproverName = "";
                            }

                            //---- get User Data from Employee 
                            item.EmployeeProfile = Mapper.Map<Person, EmployeeBasicProfile>(_db.People.Where(x => x.ID == item.Requestor).FirstOrDefault());
                        }
                    }
                    if (talentacqreqVM != null && talentacqreqVM.Count > 0)
                    {
                        talentacqreqVM = talentacqreqVM.OrderByDescending(x => x.CreatedDate).ToList();
                    }
                    return talentacqreqVM;
                }
            });
        }

        public async Task<List<TARRFViewModel>> GetReqForAppover(int personId)
        {
            return await Task.Run(() =>
            {
                using (var _db = _phoenixEntity)
                {
                    var tarrfReq = _db.TARRF.Where(x => x.PrimaryApprover == personId).ToList();
                    List<TARRFViewModel> talentacqreqVM = Mapper.Map<List<TARRF>, List<TARRFViewModel>>(tarrfReq);
                    return talentacqreqVM;
                }
            });
        }

        public async Task<List<TARRFViewModel>> GetReqForHR()
        {
            return await Task.Run(() =>
            {
                using (var _db = _phoenixEntity)
                {  // use enum for RRFStatus 
                    var TalentAcq_Req = _db.TARRF.Where(x => x.RRFStatus == 1).ToList();
                    List<TARRFViewModel> talentacqreqVM = Mapper.Map<List<TARRF>, List<TARRFViewModel>>(TalentAcq_Req);
                    return talentacqreqVM;
                }
            });
        }

        public async Task<List<TARRFDetailViewModel>> GetTalentAcqReqDetails(int ReqId)
        {
            return await Task.Run(() =>
            {
                using (var _db = _phoenixEntity)
                {
                    List<TARRFDetailViewModel> rrfDetails = new List<TARRFDetailViewModel>();
                    var taRRFDetails = _db.TARRFDetail.Where(x => x.RRFNo == ReqId).ToList();

                    foreach (var item in taRRFDetails)
                    {

                        TARRFDetailViewModel rrfDetail = Mapper.Map<TARRFDetail, TARRFDetailViewModel>(item);
                        rrfDetails.Add(rrfDetail);
                    }
                    return rrfDetails;
                }

            });
        }

        public async Task<TARRFViewModel> GetTalentAcqReqById(int ReqId)
        {
            return await Task.Run(() =>
            {
                using (var _db = _phoenixEntity)
                {
                    var TalentAcq_Req = _db.TARRF.Where(x => x.Id == ReqId).FirstOrDefault();
                    TARRFViewModel reqVM = Mapper.Map<TARRF, TARRFViewModel>(TalentAcq_Req);

                    reqVM.DeliveryUnitName = _db.DeliveryUnit.Where(x => x.ID == reqVM.DeliveryUnit).Select(x => x.Name).FirstOrDefault();
                    reqVM.DesignationName = _db.Designations.Where(x => x.ID == reqVM.Designation).Select(x => x.Name).FirstOrDefault();

                    List<string> skillNames = new List<string>();
                    var skillIDs = _db.TASkills.Where(x => x.RRFId == reqVM.Id).Select(x => x.SkillId).ToList();
                    foreach (var skillid in skillIDs)
                    {
                        var skillName = _db.SkillMatrices.Where(x => x.ID == skillid).Select(x => x.Name).FirstOrDefault();
                        skillNames.Add(skillName);
                    }
                    reqVM.SkillsName = skillNames;
                    reqVM.SkillIds = skillIDs;
                    if (reqVM.PrimaryApprover > 0)
                    {
                        var firstName = _db.People.Where(x => x.ID == reqVM.PrimaryApprover).Select(x => x.FirstName).FirstOrDefault().ToString();
                        var lastName = _db.People.Where(x => x.ID == reqVM.PrimaryApprover).Select(x => x.LastName).FirstOrDefault().ToString();
                        reqVM.PrimaryApproverName = firstName + " " + lastName;
                    }
                    else
                    {
                        reqVM.PrimaryApproverName = "";
                    }
                    List<string> interviewerNames = new List<string>();
                    var intreviewersIDs = _db.TAInterviewer.Where(x => x.RRFId == reqVM.Id).Select(X => X.PersonId).ToList();
                    foreach (var personId in intreviewersIDs)
                    {
                        var fName = _db.People.Where(x => x.ID == personId).Select(X => X.FirstName).FirstOrDefault().ToString();
                        var lName = _db.People.Where(x => x.ID == personId).Select(X => X.LastName).FirstOrDefault().ToString();

                        var interviewerName = fName + " " + lName;
                        interviewerNames.Add(interviewerName);
                    }
                    reqVM.InterviewersName = interviewerNames;
                    reqVM.InterviewerIds = intreviewersIDs;

                    if (reqVM.JD > 0)
                    {
                        reqVM.JDLink = _db.Jobdescriptions.Where(x => x.Id == reqVM.JD).Select(x => x.Link).FirstOrDefault();
                        reqVM.JDTitle = _db.Jobdescriptions.Where(x => x.Id == reqVM.JD).Select(x => x.Title).FirstOrDefault();
                    }
                    /// Change DB and EDMX pending for this
                    /// reqVM.EmpTypeName = _db.TalentAcq_EmploymentType.Where(x => x.ID == reqVM.EmploymentType).Select(x => x.EmploymentType).FirstOrDefault();
                    /// 
                    reqVM.EmpTypeName = reqVM.EmploymentType == 1 ? "Contract" : reqVM.EmploymentType == 2 ? "Regular" : reqVM.EmploymentType == 3 ? "US Employee" : "Not Selected"; // "Pending due to EDMX change";
                    //reqVM.EmpTypeName =  NEED TO IMPLEMENT THIS 
                    //_db.TalentAcq_EmploymentType.Where(x => x.ID == reqVM.EmploymentType).Select(x => x.EmploymentType).FirstOrDefault();

                    // --- for SLA 
                    if (reqVM.SLA != 0)
                    {
                        List<TARRFDetail> reqDetails = _db.TARRFDetail.Where(x => x.RRFNo.ToString() == reqVM.RRFNo).ToList();
                        IEnumerable<TARRFDetailViewModel> reqDetailsVM = Mapper.Map<IList<TARRFDetail>, IList<TARRFDetailViewModel>>(reqDetails);
                        reqVM.ReqDetails = reqDetailsVM;
                    }
                    return reqVM;
                }
            });
        }

        public async Task<bool> ReqApprovedByHR(int SLA, string Comments, int ReqId)
        {
            return await Task.Run(() =>
            {
                bool status = false;
                using (var _db = _phoenixEntity)
                {
                    TARRF rowToUpdate = new TARRF();
                    rowToUpdate.SLA = SLA;
                    rowToUpdate.ExpectedClosureDate = rowToUpdate.ModifiedDate.Value.AddDays(SLA);
                    rowToUpdate.HRApproverComments = Comments;

                    _db.Entry(rowToUpdate);
                    int rowUpdated = _db.SaveChanges();

                    if (rowUpdated == 1)  //--- Insert multiple rows into TalentAcq_ReqDetails table.
                    {
                        for (var i = 1; i <= rowToUpdate.Position.Value; i++)
                        {
                            var talentAcq_reqdetails = new TARRFDetail();
                            talentAcq_reqdetails.RRFNo = Convert.ToInt32(rowToUpdate.RRFNo);
                            talentAcq_reqdetails.Status = 1;  //---In pROGRESS   use enum 
                            talentAcq_reqdetails.comments = "";
                            talentAcq_reqdetails.RRFNumber = i;
                            _db.TARRFDetail.Add(talentAcq_reqdetails);
                            // _db.SaveChanges();
                        }
                    }
                    var Rows = _db.SaveChanges();
                    if (Rows > 0)
                    {
                        status = true;
                    }
                    return status;
                }
            });
        }

        public async Task<bool> SaveUpdateTalentAcqRequests(TARRFViewModel model)
        {
            bool status = false;
            int CurrentYear = DateTime.Now.Year;
            //model.RequestDate = DateTime.Now;
            int rowUpdated = 0;
            return await Task.Run(() =>
            {
                using (var _db = _phoenixEntity)
                {
                    var mappedTalentAcq_Req = Mapper.Map<TARRFViewModel, TARRF>(model);
                    if (mappedTalentAcq_Req.Id == 0)
                    {
                        mappedTalentAcq_Req.CreatedDate = DateTime.Now;
                        _db.TARRF.Add(mappedTalentAcq_Req);
                        _db.SaveChanges();
                        int NewReqId = mappedTalentAcq_Req.Id;

                        TARRF rowToUpdate = _db.TARRF.Where(x => x.Id == NewReqId).FirstOrDefault();
                        int RRFno = int.Parse(string.Concat(CurrentYear.ToString(), NewReqId.ToString()));
                        //--- generate  RRFno  after insert into main table get PK ID of table to create RRFNO '2019 + 1','2019 + 2', ....
                        if (!model.IsDraft)
                        {
                            if (rowToUpdate != null)
                            {
                                rowToUpdate.RRFNo = RRFno;
                                _db.Entry(rowToUpdate);
                                rowUpdated = _db.SaveChanges();
                                model.RRFNo = Convert.ToString(RRFno);
                            }
                        }

                        if (NewReqId > 0)// (rowUpdated == 1)
                        {
                            //----  TASkills table insert
                            var skillIds = model.SkillIds;
                            foreach (var item in skillIds)
                            {
                                TASkills taskills = new TASkills();
                                taskills.RRFId = NewReqId;
                                taskills.SkillId = item;
                                _db.TASkills.Add(taskills);
                                _db.SaveChanges();
                            }

                            //---  TAInterviewer table insert
                            var interviewerIds = model.InterviewerIds;
                            foreach (var item in interviewerIds)
                            {
                                TAInterviewer tainterviewer = new TAInterviewer();
                                tainterviewer.RRFId = NewReqId;
                                tainterviewer.PersonId = item;
                                _db.TAInterviewer.Add(tainterviewer);
                                _db.SaveChanges();
                            }
                            /// If Recruiter raises request, add subsequent rrfs to detail table immediately after saving rrf request
                            /// 

                            if (model.RRFStatus == 1 && model.HRApprover > 0)
                            {
                                // Change expected closure date logic = approval date + sla days 
                                rowToUpdate.ExpectedClosureDate = model.HRApprover == 0 ? rowToUpdate.ExpectedClosureDate : rowToUpdate.ModifiedDate.Value.AddDays(GetDays(model.SLA));
                                _db.Entry(rowToUpdate).State = System.Data.Entity.EntityState.Modified;
                                _db.SaveChanges();

                                for (int i = 1; i <= model.Position; i++)
                                {
                                    TARRFDetail rrfDetail = new TARRFDetail();
                                    rrfDetail.RRFNo = RRFno;
                                    rrfDetail.RRFNumber = i;
                                    rrfDetail.Status = 0;
                                    rrfDetail.comments = string.Empty;

                                    _db.Entry(rrfDetail).State = System.Data.Entity.EntityState.Added;
                                    _db.SaveChanges();
                                }
                                // Send RRF New Request Email
                                _EmailService.SendNewRRFRequestEmail(model);
                                ///Send email if Recruiter approve RRF///
                                model.ExpectedClosureDate = rowToUpdate.ExpectedClosureDate;
                                _EmailService.SendApprovedRRFHREmail(model);
                            }
                            else
                            {
                                if (!model.IsDraft)
                                {
                                    // Send RRF New Request Email
                                    _EmailService.SendNewRRFRequestEmail(model);
                                }

                            }


                            status = true;
                        }

                    }
                    else  //--- Update TalentAcq_Req table.
                    {

                        TARRF rowToUpdate = _db.TARRF.Where(x => x.Id == model.Id).FirstOrDefault();
                        //model.CreatedBy = Convert.ToInt32(rowToUpdate.CreatedBy);
                        bool rrfSubmit = false;
                        int? RRFno = null;
                        if (rowToUpdate.RRFNo == null || rowToUpdate.RRFNo <= 0)
                        {
                            rrfSubmit = true;
                            RRFno = int.Parse(string.Concat(CurrentYear.ToString(), model.Id.ToString()));
                            model.RRFNo = Convert.ToString(RRFno);
                        }

                        if (rowToUpdate != null)
                        {
                            if (rowToUpdate.ModifiedDate == null)
                            {
                                rowToUpdate.ModifiedDate = DateTime.Now;
                            }
                            rowToUpdate.RRFNo = int.Parse(model.RRFNo);
                            rowToUpdate.DeliveryUnit = model.DeliveryUnit;
                            rowToUpdate.Designation = model.Designation;
                            rowToUpdate.EmploymentType = model.EmploymentType;
                            rowToUpdate.ExpectedClosureDate = model.HRApprover == 0 ? rowToUpdate.ExpectedClosureDate : rowToUpdate.ModifiedDate.Value.AddDays(GetDays(model.SLA)); // TODO -- Change expected closure date logic = approval date + sla days 
                            rowToUpdate.HRApprover = model.HRApprover;
                            rowToUpdate.RequestorComments = model.RequestorComments;
                            rowToUpdate.HRApproverComments = model.HRApproverComments;
                            rowToUpdate.IsDraft = model.IsDraft;
                            rowToUpdate.JD = model.JD;
                            rowToUpdate.MaxYrs = model.MaxYrs;
                            rowToUpdate.MinYrs = model.MinYrs;
                            rowToUpdate.Position = model.Position;
                            rowToUpdate.PrimaryApprover = model.PrimaryApprover;
                            rowToUpdate.PrimaryApproverComments = model.PrimaryApproverComments;
                            rowToUpdate.RequestDate = model.RequestDate;
                            rowToUpdate.SLA = model.SLA;
                            rowToUpdate.RRFStatus = model.RRFStatus;
                            rowToUpdate.ModifiedBy = model.ModifiedBy;
                            rowToUpdate.ModifiedDate = DateTime.Now;

                            _db.Entry(rowToUpdate).State = System.Data.Entity.EntityState.Modified;
                            _db.SaveChanges();

                            //---- Update Skills table if any changes made
                            var skillids = model.SkillIds;
                            var taSkillRemove = _db.TASkills.Where(s => s.RRFId == model.Id).ToList();

                            foreach (var remItem in taSkillRemove)
                            {
                                _db.Entry(remItem).State = System.Data.Entity.EntityState.Deleted;
                                _db.SaveChanges();
                            }

                            foreach (var item in skillids)
                            {
                                TASkills taskills = _db.TASkills.Where(s => s.RRFId == model.Id && s.SkillId == item).FirstOrDefault();   // new TASkills();
                                if (taskills == null)
                                {
                                    taskills = new TASkills();
                                    taskills.RRFId = model.Id;
                                    taskills.SkillId = item;
                                    _db.Entry(taskills).State = System.Data.Entity.EntityState.Added;
                                    _db.SaveChanges();
                                }
                            }

                            //---- Update Interviewer table if any changes made 
                            var interviewerIds = model.InterviewerIds;
                            var taPanelremove = _db.TAInterviewer.Where(s => s.RRFId == model.Id).ToList();

                            foreach (var remItem in taPanelremove)
                            {
                                _db.Entry(remItem).State = System.Data.Entity.EntityState.Deleted;
                                _db.SaveChanges();
                            }

                            foreach (var item in interviewerIds)
                            {
                                TAInterviewer tainterviewer = _db.TAInterviewer.Where(s => s.RRFId == model.Id && s.PersonId == item).FirstOrDefault();   // new TAInterviewer();
                                if (tainterviewer == null)
                                {
                                    tainterviewer = new TAInterviewer();
                                    tainterviewer.RRFId = model.Id;
                                    tainterviewer.PersonId = item;
                                    _db.Entry(tainterviewer).State = System.Data.Entity.EntityState.Added;
                                    _db.SaveChanges();
                                }
                            }

                            if (rrfSubmit && !model.IsDraft)
                            {
                                // Send RRF New Request Email
                                _EmailService.SendNewRRFRequestEmail(model);
                            }

                            if (model.RRFStatus == 1 && model.HRApprover > 0)
                            {
                                for (int i = 1; i <= model.Position; i++)
                                {
                                    TARRFDetail rrfDetail = new TARRFDetail();
                                    rrfDetail.RRFNo = int.Parse(model.RRFNo);
                                    rrfDetail.RRFNumber = i;
                                    rrfDetail.Status = 0;
                                    rrfDetail.comments = string.Empty;

                                    _db.Entry(rrfDetail).State = System.Data.Entity.EntityState.Added;
                                    _db.SaveChanges();
                                }
                                ///Send email if Recruiter approve RRF///
                                model.ExpectedClosureDate = rowToUpdate.ExpectedClosureDate;
                                model.CreatedBy = Convert.ToInt32(rowToUpdate.CreatedBy);
                                _EmailService.SendApprovedRRFHREmail(model);
                            }
                            if (model.RRFStatus == 1 && model.HRApprover == 0)
                            {
                                ///Send email if primary approver approve RRF///
                                model.CreatedBy = Convert.ToInt32(rowToUpdate.CreatedBy);
                                _EmailService.SendApproveRRFEmail(model);
                            }
                            if (model.RRFStatus == 2)
                            {
                                ///Send email if Recruiter Reject RRF///
                                model.CreatedBy = Convert.ToInt32(rowToUpdate.CreatedBy);
                                _EmailService.SendRejectRRFEmail(model);
                            }
                            status = true;
                        }
                    }
                }
                return status;
            });
        }

        public async Task<bool> SaveUpdateRequestDetailsAsync(TARRFDetailViewModel model)
        {
            bool rowUpdated = false;
            return await Task.Run(() =>
            {
                using (var _db = _phoenixEntity)
                {
                    TARRFDetail rowToUpdate = _db.TARRFDetail.Where(x => x.Id == model.Id).FirstOrDefault();
                    if (rowToUpdate != null)
                    {
                        TARRFDetail rrfDetail = new TARRFDetail();
                        rowToUpdate.RRFNo = model.RRFNo;
                        rowToUpdate.Status = model.Status;
                        rowToUpdate.comments = model.comments;

                        _db.Entry(rowToUpdate).State = System.Data.Entity.EntityState.Modified;
                        _db.SaveChanges();
                        if (model.Status == 2)
                        {
                            // Send cancel rrf request email
                            TARRF taRRF = _db.TARRF.Where(x => x.RRFNo == model.RRFNo).FirstOrDefault();
                            _EmailService.SendCanceledRRFHREmail(model, taRRF);
                        }
                        rowUpdated = true;
                    }
                }
                return rowUpdated;
            });
        }

        private int GetDays(int sla)
        {
            switch (sla)
            {
                case 1:
                    return 30;
                case 2:
                    return 75;
                case 3:
                    return 90;
                case 4:
                    return 120;
                default:
                    return 1;
            }
        }

        public bool SwapRRFOwner(int userId)
        {
            bool isSuccess = false;
            string swappedRRFs = string.Empty;

            using (var _db = _phoenixEntity)
            {
                List<TARRF> rrfList = new List<TARRF>();
                rrfList = _db.TARRF.Where(x => (x.CreatedBy == userId || x.Requestor == userId) && x.RRFStatus <= 1).ToList();
                int rrfAdmin = _db.PersonInRole.Where(x => x.RoleID == 46).Select(a => a.PersonID).FirstOrDefault();
                rrfList.ForEach(item =>
                {
                    List<TARRFDetail> rrfCheck = new List<TARRFDetail>();
                    rrfCheck = _db.TARRFDetail.Where(x => x.RRFNo == item.RRFNo && x.Status == 0).ToList();
                    if (rrfCheck.Count() > 0)
                    {
                        swappedRRFs = swappedRRFs + item.RRFNo + ", ";
                        item.CreatedBy = rrfAdmin;
                        item.Requestor = rrfAdmin;

                        _db.Entry(item).State = System.Data.Entity.EntityState.Modified;
                        _db.SaveChanges();
                    }
                });
                //swappedRRFs = swappedRRFs.TrimEnd(',');
                //_EmailService.RrfSwapNotification(userId, rrfAdmin, swappedRRFs);
                //isSuccess = true;
                if (!string.IsNullOrEmpty(swappedRRFs))
                {
                    _EmailService.RrfSwapNotification(userId, rrfAdmin, swappedRRFs);
                }
                isSuccess = true;
            }
            return isSuccess;
        }
    }
}
