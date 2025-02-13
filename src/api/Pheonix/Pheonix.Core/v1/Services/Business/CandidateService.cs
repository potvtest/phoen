using Pheonix.Core.v1.Services.Email;
using Pheonix.Core.v1.Services.syncATS;
using Pheonix.DBContext;
using Pheonix.Models.VM;
using Pheonix.Models.VM.Classes.Candidate;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Core.v1.Services.Business
{
    public class CandidateService : ICandidateService
    {
        private IBasicOperationsService service;
        private IEmailService emailService;
        private readonly PhoenixEntities _phoenixEntity;
        private SyncAts syn;

        public CandidateService(IBasicOperationsService opsService, IEmailService opsEmailService)
        {
            service = opsService;
            emailService = opsEmailService;
            _phoenixEntity = new PhoenixEntities();
            syn = new SyncAts();
        }

        #region Manage Candidate
        public Task<int> ManageCandidate(CandidateViewModel model)
        {
            return Task.Run(() =>
            {
                int candidate = 0;
                bool isTaskCreated = false;
                bool isCandidateExist = false;
                try
                {
                    if (model.ID == 0)
                    {
                        if (!string.IsNullOrEmpty(model.CandidatePersonalVM.PersonalEmail) && !string.IsNullOrEmpty(model.CandidatePersonalVM.Mobile))
                        {
                            isCandidateExist = IsCandidateExist(model.CandidatePersonalVM.PersonalEmail, model.CandidatePersonalVM.Mobile);
                        }
                        if (!isCandidateExist)
                        {
                            isTaskCreated = CreateCandidate(model);
                            candidate = 1; // Create Candidate
                        }
                        else
                        {
                            candidate = 3; // Candidate Exist
                        }
                    }
                    else
                    {
                        isTaskCreated = UpdateCandidate(model);
                        candidate = 2; // Update Candidate
                    }
                }
                catch (Exception e)
                {
                    isTaskCreated = false;
                    candidate = -1; // Some error occurred 
                }
                return candidate;
            });
        }
        #endregion

        #region Create Candidate
        public bool CreateCandidate(CandidateViewModel model)
        {
            //return Task.Run(() =>
            //{
            bool isTaskCreated = false;
            //bool ResourceAvailability = true;

            using (PhoenixEntities dbContext = new PhoenixEntities())
            {
                try
                {
                    Candidate objCandidate = new Candidate();
                    objCandidate.CandidateStatus = model.CandidateStatus;
                    objCandidate.CurrentCTC = model.CurrentCTC;
                    objCandidate.DateOfBirth = model.DateOfBirth;
                    objCandidate.ExperienceMonths = model.ExperienceMonths;
                    objCandidate.ExperienceYears = model.ExperienceYears;
                    objCandidate.FirstName = model.FirstName;
                    objCandidate.Gender = model.Gender;
                    objCandidate.Image = model.Image;
                    objCandidate.IsDeleted = false;
                    objCandidate.LastName = model.LastName;
                    objCandidate.MiddleName = model.MiddleName;
                    objCandidate.NoticePeriod = model.NoticePeriod;
                    objCandidate.PanNumber = model.PanNumber;
                    objCandidate.PFNumber = model.PfNumber;
                    objCandidate.Reason = model.Reason;
                    objCandidate.ReleventExperienceMonths = model.ReleventExperienceMonths;
                    objCandidate.ReleventExperienceYears = model.ReleventExperienceYears;
                    objCandidate.ResumePath = model.ResumePath;
                    objCandidate.RRFNumber = model.RrfNumber;
                    objCandidate.Salutation = model.Salutation;
                    objCandidate.SourceName = model.SourceName;
                    objCandidate.SourceType = model.SourceType;

                    dbContext.Candidate.Add(objCandidate);
                    dbContext.SaveChanges();

                    if (model.CandidatePersonalVM != null)
                    {
                        InsertCandidatePersonal(model.CandidatePersonalVM, objCandidate.ID);
                    }
                    if (model.CandidateAddressVM.Count() > 0)
                    {
                        InsertCandidateAddress(model.CandidateAddressVM, objCandidate.ID);
                    }
                    if (model.CandidateCertificationVM.Count() > 0)
                    {
                        InsertCandidateCertification(model.CandidateCertificationVM, objCandidate.ID);
                    }
                    if (model.CandidateEmploymentHistoryVM.Count() > 0)
                    {
                        InsertCandidateEmploymentHistory(model.CandidateEmploymentHistoryVM, objCandidate.ID);
                    }
                    if (model.CandidatePassportVM.Count() > 0)
                    {
                        InsertCandidatePassport(model.CandidatePassportVM, objCandidate.ID);
                    }
                    if (model.CandidateQualificationMappingVM.Count() > 0)
                    {
                        InsertCandidateQualificationMapping(model.CandidateQualificationMappingVM, objCandidate.ID);
                    }
                    if (model.CandidateSkillMappingVM.Count() > 0)
                    {
                        InsertCandidateSkillMapping(model.CandidateSkillMappingVM, objCandidate.ID);
                    }
                    isTaskCreated = true;
                }
                catch
                {
                    isTaskCreated = false;
                }
            }
            return isTaskCreated;
            //  });

        }

        private void InsertCandidatePersonal(CandidatePersonalViewModel model, int CandidateId)
        {
            using (PhoenixEntities dbContext = new PhoenixEntities())
            {
                try
                {
                    CandidatePersonal objCandidatePersonal = new CandidatePersonal();
                    objCandidatePersonal.AlternateContactNo = model.AlternateContactNo;
                    objCandidatePersonal.AlternateEmail = model.AlternateEmail;
                    objCandidatePersonal.CandidateID = CandidateId;
                    objCandidatePersonal.HighestQualification = model.HighestQualification;
                    objCandidatePersonal.Hobbies = model.Hobbies;
                    objCandidatePersonal.IsDeleted = false;
                    objCandidatePersonal.IsUSVisa = model.IsUSVisa;
                    objCandidatePersonal.IsValidPassport = model.IsValidPassport;
                    objCandidatePersonal.MaritalStatus = model.MaritalStatus;
                    objCandidatePersonal.Mobile = model.Mobile;
                    objCandidatePersonal.PersonalEmail = model.PersonalEmail;
                    objCandidatePersonal.Phone = model.Phone;
                    objCandidatePersonal.ReadytoRelocate = model.ReadytoRelocate;
                    objCandidatePersonal.SpouseName = model.SpouseName;
                    objCandidatePersonal.WeddingDate = model.WeddingDate;

                    dbContext.CandidatePersonal.Add(objCandidatePersonal);
                    dbContext.SaveChanges();

                }
                catch (Exception e)
                {

                }
            };
        }

        private void InsertCandidateAddress(IEnumerable<CandidateAddressViewModel> model, int CandidateId)
        {

            using (PhoenixEntities dbContext = new PhoenixEntities())
            {
                try
                {
                    foreach (var item in model)
                    {
                        CandidateAddress objCandidateAddress = new CandidateAddress();
                        objCandidateAddress.Address = item.Address;
                        objCandidateAddress.CandidateID = CandidateId;
                        objCandidateAddress.City = item.City;
                        objCandidateAddress.Country = item.Country;
                        objCandidateAddress.IsCurrent = true;
                        objCandidateAddress.IsDeleted = false;
                        objCandidateAddress.Pin = item.Pin;
                        objCandidateAddress.State = item.State;
                        dbContext.CandidateAddress.Add(objCandidateAddress);
                        dbContext.SaveChanges();
                    }

                }
                catch (Exception e)
                {

                }

            }
        }

        private void InsertCandidateCertification(IEnumerable<CandidateCertificationViewModel> model, int CandidateId)
        {

            using (PhoenixEntities dbContext = new PhoenixEntities())
            {
                try
                {
                    foreach (var item in model)
                    {
                        CandidateCertification objCandidateCertification = new CandidateCertification();
                        objCandidateCertification.CandidateID = CandidateId;
                        objCandidateCertification.CertificationDate = item.CertificationDate;
                        objCandidateCertification.CertificationID = item.CertificationID;
                        objCandidateCertification.CertificationNumber = item.CertificationNumber;
                        objCandidateCertification.Grade = item.Grade;
                        objCandidateCertification.IsDeleted = false;
                        objCandidateCertification.StatusId = item.StatusId;
                        dbContext.CandidateCertification.Add(objCandidateCertification);
                        dbContext.SaveChanges();
                    }
                }
                catch (Exception e)
                {

                }

            }
        }

        private void InsertCandidateEmploymentHistory(IEnumerable<CandidateEmploymentHistoryViewModel> model, int CandidateId)
        {

            using (PhoenixEntities dbContext = new PhoenixEntities())
            {
                try
                {
                    foreach (var item in model)
                    {
                        CandidateEmploymentHistory objCandidateEmploymentHistory = new CandidateEmploymentHistory();
                        objCandidateEmploymentHistory.CandidateID = CandidateId;
                        objCandidateEmploymentHistory.EmploymentType = item.EmploymentType;
                        objCandidateEmploymentHistory.JoiningDate = item.JoiningDate;
                        objCandidateEmploymentHistory.LastDesignation = item.LastDesignation;
                        objCandidateEmploymentHistory.Location = item.Location;
                        objCandidateEmploymentHistory.IsDeleted = false;
                        objCandidateEmploymentHistory.OrganisationName = item.OrganisationName;
                        objCandidateEmploymentHistory.RoleDescription = item.RoleDescription;
                        objCandidateEmploymentHistory.WorkedTill = item.WorkedTill;
                        dbContext.CandidateEmploymentHistory.Add(objCandidateEmploymentHistory);
                        dbContext.SaveChanges();
                    }
                }
                catch (Exception e)
                {

                }

            }
        }

        private void InsertCandidatePassport(IEnumerable<CandidatePassportViewModel> model, int CandidateId)
        {

            using (PhoenixEntities dbContext = new PhoenixEntities())
            {
                try
                {
                    foreach (var item in model)
                    {
                        CandidatePassport objCandidatePassport = new CandidatePassport();
                        objCandidatePassport.CandidateID = CandidateId;
                        objCandidatePassport.BlankPagesLeft = item.BlankPagesLeft;
                        objCandidatePassport.DateOfExpiry = item.DateOfExpiry;
                        objCandidatePassport.DateOfIssue = item.DateOfIssue;
                        objCandidatePassport.FatherNameAsInPasssport = "NA";
                        objCandidatePassport.MotherNameAsInPassport = "NA";
                        objCandidatePassport.IsDeleted = false;
                        objCandidatePassport.NameAsInPassport = item.NameAsInPassport;
                        objCandidatePassport.PassportFileURL = item.PassportFileURL;
                        objCandidatePassport.PassportNumber = item.PassportNumber;
                        objCandidatePassport.PlaceIssued = item.PlaceIssued;
                        objCandidatePassport.PPHolderDependentID = item.PPHolderDependentID;
                        objCandidatePassport.RelationWithPPHolder = item.RelationWithPPHolder;
                        objCandidatePassport.SpouseNameAsInPassport = "NA";
                        dbContext.CandidatePassport.Add(objCandidatePassport);
                        dbContext.SaveChanges();
                    }
                }
                catch (Exception e)
                {

                }

            }
        }

        private void InsertCandidateQualificationMapping(IEnumerable<CandidateQualificationMappingViewModel> model, int CandidateId)
        {
            using (PhoenixEntities dbContext = new PhoenixEntities())
            {

                try
                {
                    foreach (var item in model)
                    {
                        CandidateQualificationMapping objCandidateQualificationMapping = new CandidateQualificationMapping();
                        objCandidateQualificationMapping.CandidateID = CandidateId;
                        objCandidateQualificationMapping.Grade_Class = item.GradeClass;
                        objCandidateQualificationMapping.Institute = item.Institute;
                        objCandidateQualificationMapping.Percentage = item.Percentage;
                        objCandidateQualificationMapping.Year = item.Year;
                        objCandidateQualificationMapping.University = item.University;
                        objCandidateQualificationMapping.IsDeleted = false;
                        objCandidateQualificationMapping.StatusId = item.StatusId;
                        objCandidateQualificationMapping.Specialization = item.Specialization;
                        objCandidateQualificationMapping.QualificationType = item.QualificationType;
                        objCandidateQualificationMapping.QualificationID = item.QualificationID;
                        dbContext.CandidateQualificationMapping.Add(objCandidateQualificationMapping);
                        dbContext.SaveChanges();
                    }
                }
                catch (Exception e)
                {

                }

            }
        }

        private void InsertCandidateSkillMapping(IEnumerable<CandidateSkillMappingViewModel> model, int CandidateId)
        {
            using (PhoenixEntities dbContext = new PhoenixEntities())
            {
                try
                {
                    foreach (var item in model)
                    {
                        CandidateSkillMapping objCandidateSkillMapping = new CandidateSkillMapping();
                        objCandidateSkillMapping.CandidateID = CandidateId;
                        objCandidateSkillMapping.ExperienceMonths = item.ExperienceMonths;
                        objCandidateSkillMapping.ExperienceYears = 0;
                        objCandidateSkillMapping.HasCoreCompetency = false;
                        objCandidateSkillMapping.SkillID = item.SkillID;
                        objCandidateSkillMapping.SkillRating = 1;
                        objCandidateSkillMapping.IsDeleted = false;
                        dbContext.CandidateSkillMapping.Add(objCandidateSkillMapping);
                        dbContext.SaveChanges();
                    }
                }
                catch (Exception e)
                {

                }

            }
        }

        #endregion

        #region Get Candidate
        public async Task<List<CandidateViewModel>> GetCandidate(string ListToDisplay)
        {
            List<CandidateViewModel> lstCandidateViewModel = new List<CandidateViewModel>();
            using (PhoenixEntities dbContext = new PhoenixEntities())
            {
                dbContext.Configuration.AutoDetectChangesEnabled = false;
                try
                {
                    lstCandidateViewModel = (from c in dbContext.Candidate
                                             where c.CandidateStatus != 10 // 10 is Joined
                                             select new CandidateViewModel
                                             {
                                                 ID = c.ID,
                                                 CandidateStatus = c.CandidateStatus,
                                                 DateOfBirth = c.DateOfBirth,
                                                 CurrentCTC = c.CurrentCTC,
                                                 ExperienceMonths = c.ExperienceMonths,
                                                 ExperienceYears = c.ExperienceYears,
                                                 ISUS = c.FirstName.Contains("US:") ? true : false,
                                                 FirstName = c.FirstName.Contains("US:") ? c.FirstName.Remove(0, 3) : c.FirstName,
                                                 Gender = c.Gender,
                                                 Image = c.Image,
                                                 IsDeleted = c.IsDeleted,
                                                 LastName = c.LastName,
                                                 MiddleName = c.MiddleName,
                                                 NoticePeriod = c.NoticePeriod,
                                                 PanNumber = c.PanNumber,
                                                 PfNumber = c.PFNumber,
                                                 Reason = c.Reason,
                                                 ReleventExperienceMonths = c.ReleventExperienceMonths,
                                                 ReleventExperienceYears = c.ReleventExperienceYears,
                                                 ResumePath = c.ResumePath,
                                                 RrfNumber = c.RRFNumber,
                                                 Salutation = c.Salutation,
                                                 SourceName = c.SourceName,
                                                 SourceType = c.SourceType,
                                                 CandidateAddressVM = (from ca in dbContext.CandidateAddress
                                                                       where ca.CandidateID == c.ID
                                                                       select new CandidateAddressViewModel
                                                                       {
                                                                           Address = ca.Address,
                                                                           CandidateID = ca.CandidateID,
                                                                           City = ca.City,
                                                                           Country = ca.Country,
                                                                           ID = ca.ID,
                                                                           IsCurrent = ca.IsCurrent,
                                                                           IsDeleted = ca.IsDeleted,
                                                                           Pin = ca.Pin,
                                                                           State = ca.State
                                                                       }).ToList(),

                                                 CandidateCertificationVM = (from cc in dbContext.CandidateCertification
                                                                             where cc.CandidateID == c.ID
                                                                             select new CandidateCertificationViewModel
                                                                             {
                                                                                 CandidateID = cc.CandidateID,
                                                                                 CertificationDate = cc.CertificationDate,
                                                                                 IsDeleted = cc.IsDeleted,
                                                                                 ID = cc.ID,
                                                                                 CertificationID = cc.CertificationID,
                                                                                 CertificationNumber = cc.CertificationNumber,
                                                                                 Grade = cc.Grade,
                                                                                 StatusId = cc.StatusId
                                                                             }).ToList(),

                                                 CandidateEmploymentHistoryVM = (from ce in dbContext.CandidateEmploymentHistory
                                                                                 where ce.CandidateID == c.ID
                                                                                 select new CandidateEmploymentHistoryViewModel
                                                                                 {
                                                                                     ID = ce.ID,
                                                                                     CandidateID = ce.CandidateID,
                                                                                     IsDeleted = ce.IsDeleted,
                                                                                     EmploymentType = ce.EmploymentType,
                                                                                     JoiningDate = ce.JoiningDate,
                                                                                     LastDesignation = ce.LastDesignation,
                                                                                     Location = ce.Location,
                                                                                     OrganisationName = ce.OrganisationName,
                                                                                     RoleDescription = ce.RoleDescription,
                                                                                     WorkedTill = ce.WorkedTill
                                                                                 }).ToList(),

                                                 CandidatePassportVM = (from cp in dbContext.CandidatePassport
                                                                        where cp.CandidateID == c.ID
                                                                        select new CandidatePassportViewModel
                                                                        {
                                                                            DateOfExpiry = cp.DateOfExpiry,
                                                                            IsDeleted = cp.IsDeleted,
                                                                            BlankPagesLeft = cp.BlankPagesLeft,
                                                                            CandidateID = cp.CandidateID,
                                                                            DateOfIssue = cp.DateOfIssue,
                                                                            FatherNameAsInPasssport = cp.FatherNameAsInPasssport,
                                                                            ID = cp.ID,
                                                                            MotherNameAsInPassport = cp.MotherNameAsInPassport,
                                                                            NameAsInPassport = cp.NameAsInPassport,
                                                                            PassportFileURL = cp.PassportFileURL,
                                                                            PassportNumber = cp.PassportNumber,
                                                                            PlaceIssued = cp.PlaceIssued,
                                                                            PPHolderDependentID = cp.PPHolderDependentID,
                                                                            RelationWithPPHolder = cp.RelationWithPPHolder,
                                                                            SpouseNameAsInPassport = cp.SpouseNameAsInPassport
                                                                        }).ToList(),


                                                 CandidatePersonalVM = (from cprsl in dbContext.CandidatePersonal
                                                                        where cprsl.CandidateID == c.ID
                                                                        select new CandidatePersonalViewModel
                                                                        {
                                                                            AlternateEmail = cprsl.AlternateEmail,
                                                                            AlternateContactNo = cprsl.AlternateContactNo,
                                                                            CandidateID = cprsl.CandidateID,
                                                                            HighestQualification = cprsl.HighestQualification,
                                                                            Hobbies = cprsl.Hobbies,
                                                                            ID = cprsl.ID,
                                                                            IsDeleted = cprsl.IsDeleted,
                                                                            IsUSVisa = cprsl.IsUSVisa,
                                                                            IsValidPassport = cprsl.IsValidPassport,
                                                                            MaritalStatus = cprsl.MaritalStatus,
                                                                            Mobile = cprsl.Mobile,
                                                                            PersonalEmail = cprsl.PersonalEmail,
                                                                            Phone = cprsl.Phone,
                                                                            ReadytoRelocate = cprsl.ReadytoRelocate,
                                                                            SpouseName = cprsl.SpouseName,
                                                                            WeddingDate = cprsl.WeddingDate
                                                                        }).FirstOrDefault(),

                                                 CandidateQualificationMappingVM = (from cq in dbContext.CandidateQualificationMapping
                                                                                    where cq.CandidateID == c.ID
                                                                                    select new CandidateQualificationMappingViewModel
                                                                                    {
                                                                                        CandidateID = cq.CandidateID,
                                                                                        GradeClass = cq.Grade_Class,
                                                                                        ID = cq.ID,
                                                                                        Institute = cq.Institute,
                                                                                        IsDeleted = cq.IsDeleted,
                                                                                        Percentage = cq.Percentage,
                                                                                        QualificationID = cq.QualificationID,
                                                                                        QualificationType = cq.QualificationType,
                                                                                        Specialization = cq.Specialization,
                                                                                        StatusId = cq.StatusId,
                                                                                        University = cq.University,
                                                                                        Year = cq.Year
                                                                                    }).ToList(),

                                                 CandidateSkillMappingVM = (from cs in dbContext.CandidateSkillMapping
                                                                            where cs.CandidateID == c.ID
                                                                            select new CandidateSkillMappingViewModel
                                                                            {
                                                                                CandidateID = cs.CandidateID,
                                                                                ExperienceMonths = cs.ExperienceMonths,
                                                                                ExperienceYears = cs.ExperienceYears,
                                                                                HasCoreCompetency = cs.HasCoreCompetency,
                                                                                ID = cs.ID,
                                                                                IsDeleted = cs.IsDeleted,
                                                                                SkillID = cs.SkillID,
                                                                                SkillRating = cs.SkillRating
                                                                            }).ToList()

                                             }).ToList();

                    foreach (var item in lstCandidateViewModel)
                    {
                        if (!String.IsNullOrEmpty(item.RrfNumber))
                        {
                            string[] strRRFArray = item.RrfNumber.Split('.');
                            int rrfNo = Convert.ToInt32(strRRFArray[0]);
                            int rrfNumbers = Convert.ToInt32(strRRFArray[1]);
                            TARRFDetail rD = (from x in dbContext.TARRFDetail
                                              where x.RRFNo == rrfNo && x.RRFNumber == rrfNumbers
                                              select x).FirstOrDefault();
                            item.RrfStatus = rD.Status;
                            if (item.RrfStatus != 0)
                            {
                                item.RrfNumber = "";
                            }

                            TARRF r = (from x in dbContext.TARRF
                                       where x.RRFNo == rrfNo
                                       select x).FirstOrDefault();
                            item.DeliveryUnit = r.DeliveryUnit;
                            item.DesignationID = r.Designation;
                            item.RrfRequestor = r.CreatedBy;
                            if (!string.IsNullOrEmpty(r.RequestorComments) && r.RequestorComments.Contains("Opening for :"))
                            {
                                string loc = r.RequestorComments.Split(':')[1];

                                item.OfficeLocation = dbContext.WorkLocation.Where(l => l.LocationName == "India - " + loc).FirstOrDefault().ID;
                            }
                            else
                                item.OfficeLocation = 0;

                            if (r.EmploymentType == 1)
                            {
                                item.EmployeeType = "Contract";
                            }
                            if (r.EmploymentType == 2)
                            {
                                item.EmployeeType = "Regular";
                            }
                        }
                    }
                    if (ListToDisplay.ToLower() == "mnh")
                        return await Task.Run(() => { return lstCandidateViewModel.Where(c => c.SourceName.ToLower() == "mnh").OrderByDescending(x => x.ID).ToList(); });
                    else
                        return await Task.Run(() => { return lstCandidateViewModel.OrderByDescending(x => x.ID).ToList(); });
                }
                catch (Exception e)
                {
                    return lstCandidateViewModel;
                }
            }
        }
        public async Task<List<CandidateViewModel>> GetCandidateHistory()
        {
            List<CandidateViewModel> lstCandidateViewModel = new List<CandidateViewModel>();
            using (PhoenixEntities dbContext = new PhoenixEntities())
            {
                try
                {
                    lstCandidateViewModel = (from c in dbContext.Candidate
                                             where c.CandidateStatus == 10 // 10 is Joined
                                             select new CandidateViewModel
                                             {
                                                 ID = c.ID,
                                                 CandidateStatus = c.CandidateStatus,
                                                 DateOfBirth = c.DateOfBirth,
                                                 CurrentCTC = c.CurrentCTC,
                                                 ExperienceMonths = c.ExperienceMonths,
                                                 ExperienceYears = c.ExperienceYears,
                                                 ISUS = c.FirstName.Contains("US:") ? true : false,
                                                 FirstName = c.FirstName.Contains("US:") ? c.FirstName.Remove(0, 3) : c.FirstName,
                                                 Gender = c.Gender,
                                                 Image = c.Image,
                                                 IsDeleted = c.IsDeleted,
                                                 LastName = c.LastName,
                                                 MiddleName = c.MiddleName,
                                                 NoticePeriod = c.NoticePeriod,
                                                 PanNumber = c.PanNumber,
                                                 PfNumber = c.PFNumber,
                                                 Reason = c.Reason,
                                                 ReleventExperienceMonths = c.ReleventExperienceMonths,
                                                 ReleventExperienceYears = c.ReleventExperienceYears,
                                                 ResumePath = c.ResumePath,
                                                 RrfNumber = c.RRFNumber,
                                                 Salutation = c.Salutation,
                                                 SourceName = c.SourceName,
                                                 SourceType = c.SourceType,
                                                 CandidateAddressVM = (from ca in dbContext.CandidateAddress
                                                                       where ca.CandidateID == c.ID
                                                                       select new CandidateAddressViewModel
                                                                       {
                                                                           Address = ca.Address,
                                                                           CandidateID = ca.CandidateID,
                                                                           City = ca.City,
                                                                           Country = ca.Country,
                                                                           ID = ca.ID,
                                                                           IsCurrent = ca.IsCurrent,
                                                                           IsDeleted = ca.IsDeleted,
                                                                           Pin = ca.Pin,
                                                                           State = ca.State
                                                                       }).ToList(),

                                                 CandidateCertificationVM = (from cc in dbContext.CandidateCertification
                                                                             where cc.CandidateID == c.ID
                                                                             select new CandidateCertificationViewModel
                                                                             {
                                                                                 CandidateID = cc.CandidateID,
                                                                                 CertificationDate = cc.CertificationDate,
                                                                                 IsDeleted = cc.IsDeleted,
                                                                                 ID = cc.ID,
                                                                                 CertificationID = cc.CertificationID,
                                                                                 CertificationNumber = cc.CertificationNumber,
                                                                                 Grade = cc.Grade,
                                                                                 StatusId = cc.StatusId
                                                                             }).ToList(),

                                                 CandidateEmploymentHistoryVM = (from ce in dbContext.CandidateEmploymentHistory
                                                                                 where ce.CandidateID == c.ID
                                                                                 select new CandidateEmploymentHistoryViewModel
                                                                                 {
                                                                                     ID = ce.ID,
                                                                                     CandidateID = ce.CandidateID,
                                                                                     IsDeleted = ce.IsDeleted,
                                                                                     EmploymentType = ce.EmploymentType,
                                                                                     JoiningDate = ce.JoiningDate,
                                                                                     LastDesignation = ce.LastDesignation,
                                                                                     Location = ce.Location,
                                                                                     OrganisationName = ce.OrganisationName,
                                                                                     RoleDescription = ce.RoleDescription,
                                                                                     WorkedTill = ce.WorkedTill
                                                                                 }).ToList(),

                                                 CandidatePassportVM = (from cp in dbContext.CandidatePassport
                                                                        where cp.CandidateID == c.ID
                                                                        select new CandidatePassportViewModel
                                                                        {
                                                                            DateOfExpiry = cp.DateOfExpiry,
                                                                            IsDeleted = cp.IsDeleted,
                                                                            BlankPagesLeft = cp.BlankPagesLeft,
                                                                            CandidateID = cp.CandidateID,
                                                                            DateOfIssue = cp.DateOfIssue,
                                                                            FatherNameAsInPasssport = cp.FatherNameAsInPasssport,
                                                                            ID = cp.ID,
                                                                            MotherNameAsInPassport = cp.MotherNameAsInPassport,
                                                                            NameAsInPassport = cp.NameAsInPassport,
                                                                            PassportFileURL = cp.PassportFileURL,
                                                                            PassportNumber = cp.PassportNumber,
                                                                            PlaceIssued = cp.PlaceIssued,
                                                                            PPHolderDependentID = cp.PPHolderDependentID,
                                                                            RelationWithPPHolder = cp.RelationWithPPHolder,
                                                                            SpouseNameAsInPassport = cp.SpouseNameAsInPassport
                                                                        }).ToList(),


                                                 CandidatePersonalVM = (from cprsl in dbContext.CandidatePersonal
                                                                        where cprsl.CandidateID == c.ID
                                                                        select new CandidatePersonalViewModel
                                                                        {
                                                                            AlternateEmail = cprsl.AlternateEmail,
                                                                            AlternateContactNo = cprsl.AlternateContactNo,
                                                                            CandidateID = cprsl.CandidateID,
                                                                            HighestQualification = cprsl.HighestQualification,
                                                                            Hobbies = cprsl.Hobbies,
                                                                            ID = cprsl.ID,
                                                                            IsDeleted = cprsl.IsDeleted,
                                                                            IsUSVisa = cprsl.IsUSVisa,
                                                                            IsValidPassport = cprsl.IsValidPassport,
                                                                            MaritalStatus = cprsl.MaritalStatus,
                                                                            Mobile = cprsl.Mobile,
                                                                            PersonalEmail = cprsl.PersonalEmail,
                                                                            Phone = cprsl.Phone,
                                                                            ReadytoRelocate = cprsl.ReadytoRelocate,
                                                                            SpouseName = cprsl.SpouseName,
                                                                            WeddingDate = cprsl.WeddingDate
                                                                        }).FirstOrDefault(),

                                                 CandidateQualificationMappingVM = (from cq in dbContext.CandidateQualificationMapping
                                                                                    where cq.CandidateID == c.ID
                                                                                    select new CandidateQualificationMappingViewModel
                                                                                    {
                                                                                        CandidateID = cq.CandidateID,
                                                                                        GradeClass = cq.Grade_Class,
                                                                                        ID = cq.ID,
                                                                                        Institute = cq.Institute,
                                                                                        IsDeleted = cq.IsDeleted,
                                                                                        Percentage = cq.Percentage,
                                                                                        QualificationID = cq.QualificationID,
                                                                                        QualificationType = cq.QualificationType,
                                                                                        Specialization = cq.Specialization,
                                                                                        StatusId = cq.StatusId,
                                                                                        University = cq.University,
                                                                                        Year = cq.Year
                                                                                    }).ToList(),

                                                 CandidateSkillMappingVM = (from cs in dbContext.CandidateSkillMapping
                                                                            where cs.CandidateID == c.ID
                                                                            select new CandidateSkillMappingViewModel
                                                                            {
                                                                                CandidateID = cs.CandidateID,
                                                                                ExperienceMonths = cs.ExperienceMonths,
                                                                                ExperienceYears = cs.ExperienceYears,
                                                                                HasCoreCompetency = cs.HasCoreCompetency,
                                                                                ID = cs.ID,
                                                                                IsDeleted = cs.IsDeleted,
                                                                                SkillID = cs.SkillID,
                                                                                SkillRating = cs.SkillRating
                                                                            }).ToList()

                                             }).ToList();

                    foreach (var item in lstCandidateViewModel)
                    {
                        if (!String.IsNullOrEmpty(item.RrfNumber))
                        {
                            string[] strRRFArray = item.RrfNumber.Split('.');
                            int rrfNo = Convert.ToInt32(strRRFArray[0]);
                            int rrfNumbers = Convert.ToInt32(strRRFArray[1]);
                            TARRFDetail r = (from x in dbContext.TARRFDetail
                                             where x.RRFNo == rrfNo && x.RRFNumber == rrfNumbers
                                             select x).FirstOrDefault();
                            item.RrfStatus = r.Status;
                        }
                    }
                    return await Task.Run(() => { return lstCandidateViewModel.OrderByDescending(x => x.ID).ToList(); });
                }
                catch (Exception e)
                {
                    return lstCandidateViewModel;
                }
            }
        }
        #endregion

        #region Update Candidate
        public bool UpdateCandidate(CandidateViewModel model)
        {
            //return Task.Run(() =>
            //{
            bool isTaskCreated = false;
            //bool ResourceAvailability = true;
            using (PhoenixEntities dbContext = new PhoenixEntities())
            {
                try
                {
                    int CandidateId = model.ID;
                    Candidate c = (from x in dbContext.Candidate
                                   where x.ID == CandidateId
                                   select x).FirstOrDefault();
                    c.CandidateStatus = model.CandidateStatus;
                    c.CurrentCTC = model.CurrentCTC;
                    c.DateOfBirth = model.DateOfBirth;
                    c.ExperienceMonths = model.ExperienceMonths;
                    c.ExperienceYears = model.ExperienceYears;
                    c.FirstName = model.FirstName;
                    c.Gender = model.Gender;
                    c.Image = model.Image;
                    c.IsDeleted = false;
                    c.LastName = model.LastName;
                    c.MiddleName = model.MiddleName;
                    c.NoticePeriod = model.NoticePeriod;
                    c.PanNumber = model.PanNumber;
                    c.PFNumber = model.PfNumber;
                    c.Reason = model.Reason;
                    c.ReleventExperienceMonths = model.ReleventExperienceMonths;
                    c.ReleventExperienceYears = model.ReleventExperienceYears;
                    c.ResumePath = model.ResumePath;
                    c.RRFNumber = model.RrfNumber;
                    c.Salutation = model.Salutation;
                    c.SourceName = model.SourceName;
                    c.SourceType = model.SourceType;
                    dbContext.Entry(c).State = EntityState.Modified;
                    dbContext.SaveChanges();

                    if (model.CandidatePersonalVM != null)
                    {
                        UpdateCandidatePersonal(model.CandidatePersonalVM, CandidateId);
                    }
                    if (model.CandidateAddressVM.Count() > 0)
                    {
                        UpdateCandidateAddress(model.CandidateAddressVM, CandidateId);
                    }
                    if (model.CandidateCertificationVM.Count() > 0)
                    {
                        UpdateCandidateCertification(model.CandidateCertificationVM, CandidateId);
                    }
                    if (model.CandidateEmploymentHistoryVM.Count() > 0)
                    {
                        UpdateCandidateEmploymentHistory(model.CandidateEmploymentHistoryVM, CandidateId);
                    }
                    if (model.CandidatePassportVM.Count() > 0)
                    {
                        UpdateCandidatePassport(model.CandidatePassportVM, CandidateId);
                    }
                    if (model.CandidateQualificationMappingVM.Count() > 0)
                    {
                        UpdateCandidateQualificationMapping(model.CandidateQualificationMappingVM, CandidateId);
                    }
                    if (model.CandidateSkillMappingVM.Count() > 0)
                    {
                        UpdateCandidateSkillMapping(model.CandidateSkillMappingVM, CandidateId);
                    }
                    isTaskCreated = true;
                }
                catch
                {
                    isTaskCreated = false;
                }
            }
            return isTaskCreated;
            //  });

        }

        public bool UpdateCandidateStatus(int CandidateId)
        {
            //return Task.Run(() =>
            //{
            bool isTaskCreated = false;
            //bool ResourceAvailability = true;
            using (PhoenixEntities dbContext = new PhoenixEntities())
            {
                try
                {
                    Candidate c = (from x in dbContext.Candidate
                                   where x.ID == CandidateId
                                   select x).FirstOrDefault();
                    if (c != null)
                    {
                        c.CandidateStatus = 1; // 1 is Active
                        dbContext.Entry(c).State = EntityState.Modified;
                        dbContext.SaveChanges();
                    }
                    isTaskCreated = true;
                }
                catch
                {
                    isTaskCreated = false;
                }
            }
            return isTaskCreated;
            //  });

        }
        private bool UpdateCandidatePersonal(CandidatePersonalViewModel model, int candidateID)
        {
            bool isSuccsess = false;
            try
            {
                using (PhoenixEntities dbContext = new PhoenixEntities())
                {
                    CandidatePersonal p = (from x in dbContext.CandidatePersonal
                                           where x.CandidateID == candidateID
                                           select x).FirstOrDefault();
                    p.AlternateContactNo = model.AlternateContactNo;
                    p.AlternateEmail = model.AlternateEmail;
                    p.CandidateID = candidateID;
                    p.HighestQualification = model.HighestQualification;
                    p.Hobbies = model.Hobbies;
                    p.IsDeleted = false;
                    p.IsUSVisa = model.IsUSVisa;
                    p.IsValidPassport = model.IsValidPassport;
                    p.MaritalStatus = model.MaritalStatus;
                    p.Mobile = model.Mobile;
                    p.PersonalEmail = model.PersonalEmail;
                    p.Phone = model.Phone;
                    p.ReadytoRelocate = model.ReadytoRelocate;
                    p.SpouseName = model.SpouseName;
                    p.WeddingDate = model.WeddingDate;
                    if (p == null)
                    {
                        dbContext.CandidatePersonal.Add(p);
                    }
                    else
                    {
                        dbContext.Entry(p).State = EntityState.Modified;
                    }
                    dbContext.SaveChanges();
                    return isSuccsess = true;
                }
            }
            catch
            {
                return isSuccsess;
            }
        }
        private bool UpdateCandidateAddress(IEnumerable<CandidateAddressViewModel> model, int candidateID)
        {
            bool isSuccsess = false;
            using (PhoenixEntities dbContext = new PhoenixEntities())
            {
                try
                {
                    foreach (var item in model)
                    {
                        int AddressID = item.ID;
                        CandidateAddress a = (from x in dbContext.CandidateAddress
                                              where x.CandidateID == candidateID && x.ID == AddressID
                                              select x).FirstOrDefault();

                        if (a == null)
                        {
                            CandidateAddress objA = new CandidateAddress();
                            objA.Address = item.Address;
                            objA.CandidateID = candidateID;
                            objA.City = item.City;
                            objA.Country = item.Country;
                            objA.IsCurrent = true;
                            objA.IsDeleted = false;
                            objA.Pin = item.Pin;
                            objA.State = item.State;
                            dbContext.CandidateAddress.Add(objA);
                        }
                        else
                        {
                            a.Address = item.Address;
                            a.CandidateID = candidateID;
                            a.City = item.City;
                            a.Country = item.Country;
                            a.IsCurrent = true;
                            a.IsDeleted = false;
                            a.Pin = item.Pin;
                            a.State = item.State;
                            dbContext.Entry(a).State = EntityState.Modified;
                        }
                        dbContext.SaveChanges();
                    }
                    return isSuccsess = true;
                }
                catch (Exception e)
                {
                    return isSuccsess = false;
                }

            }
        }
        private bool UpdateCandidateCertification(IEnumerable<CandidateCertificationViewModel> model, int candidateID)
        {
            bool isSuccsess = false;
            using (PhoenixEntities dbContext = new PhoenixEntities())
            {
                try
                {
                    foreach (var item in model)
                    {
                        int CertID = item.ID;
                        CandidateCertification c = (from x in dbContext.CandidateCertification
                                                    where x.CandidateID == candidateID && x.ID == CertID
                                                    select x).FirstOrDefault();
                        if (c == null)
                        {
                            CandidateCertification objc = new CandidateCertification();
                            objc.CandidateID = candidateID;
                            objc.CertificationDate = item.CertificationDate;
                            objc.CertificationID = item.CertificationID;
                            objc.CertificationNumber = item.CertificationNumber;
                            objc.Grade = item.Grade;
                            objc.IsDeleted = false;
                            objc.StatusId = item.StatusId;
                            dbContext.CandidateCertification.Add(objc);
                        }
                        else
                        {
                            c.CandidateID = candidateID;
                            c.CertificationDate = item.CertificationDate;
                            c.CertificationID = item.CertificationID;
                            c.CertificationNumber = item.CertificationNumber;
                            c.Grade = item.Grade;
                            c.IsDeleted = false;
                            c.StatusId = item.StatusId;
                            dbContext.Entry(c).State = EntityState.Modified;
                        }
                        dbContext.SaveChanges();
                    }
                    return isSuccsess = true;
                }
                catch (Exception e)
                {
                    return isSuccsess;
                }
            }
        }
        private bool UpdateCandidateEmploymentHistory(IEnumerable<CandidateEmploymentHistoryViewModel> model, int candidateID)
        {
            bool isSuccsess = false;
            using (PhoenixEntities dbContext = new PhoenixEntities())
            {
                try
                {
                    IEnumerable<CandidateEmploymentHistory> candidateEmploymentHistory = from x in dbContext.CandidateEmploymentHistory
                                                                                         where x.CandidateID == candidateID
                                                                                         select x;
                    // delete old records from table
                    foreach (CandidateEmploymentHistory item in candidateEmploymentHistory.ToList())
                    {
                        dbContext.Entry(item).State = EntityState.Deleted;
                        dbContext.SaveChanges();
                    }

                    // Insert new records in to table from model 
                    foreach (var item in model)
                    {
                        CandidateEmploymentHistory obje = new CandidateEmploymentHistory();
                        obje.CandidateID = candidateID;
                        obje.EmploymentType = item.EmploymentType;
                        obje.JoiningDate = item.JoiningDate;
                        obje.LastDesignation = item.LastDesignation;
                        obje.Location = item.Location;
                        obje.IsDeleted = false;
                        obje.OrganisationName = item.OrganisationName;
                        obje.RoleDescription = item.RoleDescription;
                        obje.WorkedTill = item.WorkedTill;
                        dbContext.CandidateEmploymentHistory.Add(obje);
                        dbContext.SaveChanges();
                    }
                    return isSuccsess = true;
                }
                catch (Exception e)
                {
                    return isSuccsess;
                }

            }
        }
        private bool UpdateCandidatePassport(IEnumerable<CandidatePassportViewModel> model, int candidateID)
        {
            bool isSuccsess = false;
            using (PhoenixEntities dbContext = new PhoenixEntities())
            {
                try
                {
                    foreach (var item in model)
                    {
                        int PassID = item.ID;
                        CandidatePassport p = (from x in dbContext.CandidatePassport
                                               where x.CandidateID == candidateID && x.ID == PassID
                                               select x).FirstOrDefault();

                        if (p == null)
                        {
                            CandidatePassport objP = new CandidatePassport();
                            objP.CandidateID = candidateID;
                            objP.BlankPagesLeft = item.BlankPagesLeft;
                            objP.DateOfExpiry = item.DateOfExpiry;
                            objP.DateOfIssue = item.DateOfIssue;
                            objP.FatherNameAsInPasssport = "NA";
                            objP.MotherNameAsInPassport = "NA";
                            objP.IsDeleted = false;
                            objP.NameAsInPassport = item.NameAsInPassport;
                            objP.PassportFileURL = item.PassportFileURL;
                            objP.PassportNumber = item.PassportNumber;
                            objP.PlaceIssued = item.PlaceIssued;
                            objP.PPHolderDependentID = item.PPHolderDependentID;
                            objP.RelationWithPPHolder = item.RelationWithPPHolder;
                            objP.SpouseNameAsInPassport = "NA";
                            dbContext.CandidatePassport.Add(objP);
                        }
                        else
                        {
                            p.CandidateID = candidateID;
                            p.BlankPagesLeft = item.BlankPagesLeft;
                            p.DateOfExpiry = item.DateOfExpiry;
                            p.DateOfIssue = item.DateOfIssue;
                            p.FatherNameAsInPasssport = "NA";
                            p.MotherNameAsInPassport = "NA";
                            p.IsDeleted = false;
                            p.NameAsInPassport = item.NameAsInPassport;
                            p.PassportFileURL = item.PassportFileURL;
                            p.PassportNumber = item.PassportNumber;
                            p.PlaceIssued = item.PlaceIssued;
                            p.PPHolderDependentID = item.PPHolderDependentID;
                            p.RelationWithPPHolder = item.RelationWithPPHolder;
                            p.SpouseNameAsInPassport = "NA";
                            dbContext.Entry(p).State = EntityState.Modified;
                        }
                        dbContext.SaveChanges();
                    }
                    return isSuccsess = true;
                }
                catch (Exception e)
                {
                    return isSuccsess;
                }

            }
        }
        private bool UpdateCandidateQualificationMapping(IEnumerable<CandidateQualificationMappingViewModel> model, int candidateID)
        {
            bool isSuccsess = false;
            using (PhoenixEntities dbContext = new PhoenixEntities())
            {

                try
                {
                    IEnumerable<CandidateQualificationMapping> candidateQualificationMapping = from x in dbContext.CandidateQualificationMapping
                                                                                               where x.CandidateID == candidateID
                                                                                               select x;
                    // delete old records from table
                    foreach (CandidateQualificationMapping item in candidateQualificationMapping.ToList())
                    {
                        dbContext.Entry(item).State = EntityState.Deleted;
                        dbContext.SaveChanges();
                    }

                    // Insert new records in to table from model 
                    foreach (var item in model)
                    {
                        CandidateQualificationMapping objq = new CandidateQualificationMapping();
                        objq.CandidateID = candidateID;
                        objq.Grade_Class = item.GradeClass;
                        objq.Institute = item.Institute;
                        objq.Percentage = item.Percentage;
                        objq.Year = item.Year;
                        objq.University = item.University;
                        objq.IsDeleted = false;
                        objq.StatusId = item.StatusId;
                        objq.Specialization = item.Specialization;
                        objq.QualificationType = item.QualificationType;
                        objq.QualificationID = item.QualificationID;
                        dbContext.CandidateQualificationMapping.Add(objq);
                        dbContext.SaveChanges();
                    }
                    return isSuccsess = true;
                }
                catch (Exception e)
                {
                    return isSuccsess;
                }

            }
        }
        private bool UpdateCandidateSkillMapping(IEnumerable<CandidateSkillMappingViewModel> model, int candidateID)
        {
            bool isSuccsess = false;
            using (PhoenixEntities dbContext = new PhoenixEntities())
            {
                try
                {
                    IEnumerable<CandidateSkillMapping> candidateSkillMapping = from x in dbContext.CandidateSkillMapping
                                                                               where x.CandidateID == candidateID
                                                                               select x;
                    // delete old records from table
                    foreach (CandidateSkillMapping item in candidateSkillMapping.ToList())
                    {
                        dbContext.Entry(item).State = EntityState.Deleted;
                        dbContext.SaveChanges();
                    }

                    // Insert new records in to table from model 
                    foreach (var item in model)
                    {
                        CandidateSkillMapping objS = new CandidateSkillMapping();
                        objS.CandidateID = candidateID;
                        objS.ExperienceMonths = item.ExperienceMonths;
                        objS.ExperienceYears = 0;
                        objS.HasCoreCompetency = false;
                        objS.SkillID = item.SkillID;
                        objS.SkillRating = 1;
                        objS.IsDeleted = false;
                        dbContext.CandidateSkillMapping.Add(objS);
                        dbContext.SaveChanges();
                    }
                    return isSuccsess = true;
                }
                catch (Exception e)
                {
                    return isSuccsess;
                }
            }
        }
        #endregion

        #region  Migrate Candidate To Emp
        public async Task<int> MigrateCandidate(CandidateViewModel model, string CreateURL, string clientID, string clSecret)
        {
            return await Task.Run(() =>
            {
                int nextPersonID = 0;
                int rrfNo = 0;
                int rrfNumbers = 0;
                TARRF r = new TARRF();
                TARRFDetail rd = new TARRFDetail();
                try
                {
                    using (PhoenixEntities dbContext = new PhoenixEntities())
                    {
                        if (model.ISContractConverion)
                        {
                            nextPersonID = MigrateEmployee(model);
                            return nextPersonID;
                        }
                        bool validOrgEmail = IsValidOrgEmail(model.OrganizationEmail);
                        if (!validOrgEmail)
                        {
                            nextPersonID = -1; // -1 is represent orgEmailID is already exist in the system.
                            return nextPersonID;
                        }
                        if (!string.IsNullOrEmpty(model.RrfNumber))
                        {
                            string[] strRRFArray = model.RrfNumber.Split('.');
                            rrfNo = Convert.ToInt32(strRRFArray[0]);
                            rrfNumbers = Convert.ToInt32(strRRFArray[1]);
                            bool validRRFNumber = IsValidRRFNumber(rrfNo, rrfNumbers);
                            if (!validRRFNumber)
                            {
                                nextPersonID = -3; // -3 is represent RRF is completed or Canceled.
                                return nextPersonID;
                            }
                        }
                        nextPersonID = GenerateNextPersionID(model);
                        if (dbContext.People.Any(p => p.ID == nextPersonID))
                        {
                            nextPersonID = -2; // -2 is represent personID is already exist in the system.
                            return nextPersonID;
                        }

                        bool isPersonCreated = service.Create<Person>(new Person
                        {
                            ID = nextPersonID,
                            Active = true,
                            FirstName = model.FirstName,
                            MiddleName = model.MiddleName,
                            LastName = model.LastName,
                            DateOfBirth = model.DateOfBirth,
                            Salutation = model.Salutation,
                            Gender = model.Gender,
                            IsDeleted = false,
                            Image = "no-image.png",
                        }, null);

                        if (isPersonCreated)
                        {
                            service.Finalize(true);
                        }

                        if (!string.IsNullOrEmpty(model.RrfNumber))
                        {
                            r = (from x in dbContext.TARRF
                                 where x.RRFNo == rrfNo
                                 select x).FirstOrDefault();
                            rd = (from x in dbContext.TARRFDetail
                                  where x.RRFNo == rrfNo && x.RRFNumber == rrfNumbers
                                  select x).FirstOrDefault();
                            rd.Status = 1;
                            dbContext.Entry(rd).State = EntityState.Modified;
                        }

                        Candidate c = (from x in dbContext.Candidate
                                       where x.ID == model.ID
                                       select x).FirstOrDefault();
                        c.RRFNumber = model.RrfNumber;
                        c.CandidateStatus = 10; // 10 is Joined
                        dbContext.Entry(c).State = EntityState.Modified;
                        dbContext.SaveChanges();

                        if (model.CandidatePersonalVM != null)
                        {
                            bool isPersonPersonalCreated = service.Create<PersonPersonal>(new PersonPersonal
                            {
                                PersonID = nextPersonID,
                                MaritalStatus = model.CandidatePersonalVM.MaritalStatus,
                                BloodGroup = null,
                                DateOfMarriage = model.CandidatePersonalVM.WeddingDate,
                                Hobbies = model.CandidatePersonalVM.Hobbies,
                                Mobile = model.CandidatePersonalVM.Mobile,
                                PANNo = model.PanNumber,
                                Phone = model.CandidatePersonalVM.Phone,
                                PersonalEmail = model.CandidatePersonalVM.PersonalEmail,
                                PFNo = model.PfNumber,
                                IsDeleted = false,
                                SpouseName = model.CandidatePersonalVM.SpouseName,
                            }, null);
                            if (isPersonPersonalCreated)
                            {
                                service.Finalize(true);
                            }
                        }

                        bool isPersonContactCreated = service.Create<PersonEmployment>(new PersonEmployment
                        {
                            RRFNumber = model.RrfNumber,
                            CandidateID = model.ID,
                            PersonID = nextPersonID,
                            EmployeeType = model.EmployeeType,
                            EmploymentStatus = model.EmploymentStatus,
                            IsDayShift = true,
                            JoiningDate = model.JoiningDate,
                            ProbationReviewDate = model.ProbationReviewDate,
                            RejoinedWithinYear = model.RejoinedWithinYear,
                            BusinessGroup = "1",
                            DesignationID = model.DesignationID,
                            OfficeLocation = model.OfficeLocation,
                            OrganizationEmail = model.OrganizationEmail,
                            DeliveryUnit = model.DeliveryUnit,
                            WorkLocation = model.WorkLocation,
                            ExitProcessManager = model.ExitProcessManager,
                            OrgUnit = model.OrgUnit,
                            IsDeleted = false,
                        }, null);


                        if (isPersonContactCreated)
                        {
                            service.Finalize(true);
                        }


                        foreach (var item in model.CandidateAddressVM)
                        {
                            bool isPersonAddressCreated = false;
                            if (!String.IsNullOrEmpty(item.Address))
                            {
                                isPersonAddressCreated = service.Create<PersonAddress>(new PersonAddress
                                {
                                    Address = item.Address,
                                    City = item.City,
                                    State = item.State,
                                    Country = item.Country,
                                    Pin = item.Pin,
                                    IsCurrent = true,
                                    HouseOnRent = false,
                                    PersonID = nextPersonID,
                                    IsDeleted = false,
                                }, null);
                            }
                            if (isPersonAddressCreated)
                            {
                                service.Finalize(true);
                            }

                        }

                        foreach (var item in model.CandidateEmploymentHistoryVM)
                        {
                            bool isPersonEmploymentCreated = false;
                            if (!String.IsNullOrEmpty(item.OrganisationName))
                            {
                                isPersonEmploymentCreated = service.Create<PersonEmploymentHistory>(new PersonEmploymentHistory
                                {
                                    OrganisationName = item.OrganisationName,
                                    Location = item.Location,
                                    JoiningDate = item.JoiningDate,
                                    WorkedTill = item.WorkedTill,
                                    EmploymentType = item.EmploymentType,
                                    LastDesignation = item.LastDesignation,
                                    RoleDescription = item.RoleDescription,
                                    PersonID = nextPersonID,
                                    IsDeleted = false,
                                }, null);
                            }
                            if (isPersonEmploymentCreated)
                            {
                                service.Finalize(true);
                            }
                        }

                        foreach (var item in model.CandidateQualificationMappingVM)
                        {
                            //var newEmployeeQualifications = Mapper.Map<EmployeeQualification, PersonQualificationMapping>(model.NewModel);
                            //newEmployeeQualifications.Person = service.First<Person>(x => x.ID == nextPersonID);
                            //newEmployeeQualifications.Qualification = service.First<Qualification>(x => x.ID == model.NewModel.QualificationID);
                            //isQualificationUpdated = service.Create<PersonQualificationMapping>(newEmployeeQualifications, x => x.Qualification.ID == model.NewModel.QualificationID && x.Person.ID == model.NewModel.SearchUserID && x.ID == model.NewModel.ID);

                            bool isPersonQualificationCreated = false;
                            if (!String.IsNullOrEmpty(item.GradeClass))
                            {
                                isPersonQualificationCreated = service.Create<PersonQualificationMapping>(new PersonQualificationMapping
                                {
                                    //Institute = item.Institute,
                                    Grade_Class = item.GradeClass,
                                    University = item.University,
                                    Specialization = item.Specialization,
                                    Year = item.Year,
                                    PersonID = nextPersonID,
                                    QualificationID = item.QualificationID,
                                    IsDeleted = false,
                                    Percentage = null,
                                    QualificationType = item.QualificationType,
                                    Person = service.First<Person>(x => x.ID == nextPersonID),
                                    Qualification = service.First<Qualification>(x => x.ID == item.QualificationID),
                                }, null);
                            }

                            if (isPersonQualificationCreated)
                            {
                                service.Finalize(true);
                            }
                        }


                        foreach (var item in model.CandidateSkillMappingVM)
                        {
                            bool isPersonPersonSkillMappingCreated = false;
                            if (item.SkillID > 0)
                            {
                                isPersonPersonSkillMappingCreated = service.Create<PersonSkillMapping>(new PersonSkillMapping
                                {
                                    PersonID = nextPersonID,
                                    ExperienceMonths = 0,
                                    ExperienceYears = 0,
                                    HasCoreCompetency = false,
                                    SkillID = item.SkillID,
                                    SkillRating = 1,
                                    IsDeleted = false,
                                }, null);
                            }
                            if (isPersonPersonSkillMappingCreated)
                            {
                                service.Finalize(true);
                            }
                        }

                        bool isReportingToCreated = service.Create<PersonReporting>(new PersonReporting
                        {
                            PersonID = nextPersonID,
                            ReportingTo = model.ReportingTo,
                            Active = true,
                            IsDeleted = false,
                            ModifiedBy = model.LoggedInUserID
                        }, null);


                        if (isReportingToCreated)
                        {
                            service.Finalize(true);
                            CandidateToEmployee candidateToEmployee = new CandidateToEmployee();
                            candidateToEmployee = FillCandidateToEmployee(model, nextPersonID, r);
                            emailService.SendNewEmployeeEmail(candidateToEmployee, r, rd);
                           var strMovedtoMNH = syn.sendToMNH(candidateToEmployee, CreateURL, clientID, clSecret);
                        }
                    }
                }
                catch (Exception e)
                {
                    return nextPersonID;
                }
                return nextPersonID;
            });
        }

        private int GenerateNextPersionID(CandidateViewModel model)
        {
            int nextPersonID = 0;
            using (PhoenixEntities dbContext = new PhoenixEntities())
            {
                if (model.OfficeLocation == 2 && model.EmployeeType == "Regular")
                {
                    nextPersonID = (from p in dbContext.People
                                    join pe in dbContext.PersonEmployment on p.ID equals pe.PersonID
                                    where pe.OfficeLocation == 2 && p.ID != 900012 && pe.EmployeeType == "Regular"
                                    orderby pe.ID descending
                                    select p.ID).First() + 1;
                }
                else if (model.EmployeeType == "Contract")
                {
                    //nextPersonID = (from p in dbContext.People
                    //                join pe in dbContext.PersonEmployment on p.ID equals pe.PersonID
                    //                where pe.EmployeeType == "Contract" && p.ID < 1000
                    //                orderby p.ID descending
                    //                select p.ID).First() + 1;

                    nextPersonID = (from p in dbContext.People
                                    join pe in dbContext.PersonEmployment on p.ID equals pe.PersonID
                                    where pe.EmployeeType == "Contract"
                                    orderby pe.ID descending
                                    select p.ID).First() + 1;

                    if (nextPersonID == 1000)
                        nextPersonID = 200001;
                }
                else
                {
                    nextPersonID = (from p in dbContext.People
                                    join pe in dbContext.PersonEmployment on p.ID equals pe.PersonID
                                    where pe.OfficeLocation != 2 && p.ID != 900012 && pe.EmployeeType == "Regular"
                                    orderby pe.ID descending
                                    select p.ID).First() + 1;
                }

            }
            return nextPersonID;
        }

        private CandidateToEmployee FillCandidateToEmployee(CandidateViewModel model, int nextPersonID, TARRF r = null)
        {
            CandidateToEmployee candidateToEmployee = new CandidateToEmployee();
            candidateToEmployee.RrfNumber = model.RrfNumber;
            candidateToEmployee.EmployeeCode = nextPersonID;
            candidateToEmployee.FirstName = model.FirstName;
            candidateToEmployee.LastName = model.LastName;
            candidateToEmployee.EmploymentStatus = model.EmploymentStatus;
            candidateToEmployee.EmployeeType = model.EmployeeType;
            candidateToEmployee.DeliveryUnit = model.DeliveryUnit;
            candidateToEmployee.RejoinedWithinYear = model.RejoinedWithinYear;
            candidateToEmployee.OfficeLocation = model.OfficeLocation;
            candidateToEmployee.WorkLocation = model.WorkLocation;
            candidateToEmployee.ProbationReviewDate = model.ProbationReviewDate;
            candidateToEmployee.JoiningDate = model.JoiningDate;
            candidateToEmployee.DesignationID = model.DesignationID;
            if (r != null)
            {
                candidateToEmployee.RrfRequestor = r.Requestor;
                if (model.DesignationID != r.Designation)
                {
                    candidateToEmployee.RRFRequestedDesignationID = r.Designation;
                    candidateToEmployee.IsRRFDesignationChanged = true;
                }
                else
                {
                    candidateToEmployee.IsRRFDesignationChanged = false;
                }
            }
            candidateToEmployee.ReportingTo = model.ReportingTo;
            candidateToEmployee.ExitProcessManager = model.ExitProcessManager;
            candidateToEmployee.OrganizationEmail = model.OrganizationEmail;
            candidateToEmployee.CandidateSkillMappingVM = model.CandidateSkillMappingVM;
            candidateToEmployee.CandidateCCRecipients = model.CandidateCCRecipients;
            candidateToEmployee.LoggedInUserID = model.LoggedInUserID;
            return candidateToEmployee;
        }

        private bool IsValidOrgEmail(string emailID)
        {
            bool isvalid = false;
            using (PhoenixEntities dbContext = new PhoenixEntities())
            {
                string orgEmail = (from x in dbContext.PersonEmployment
                                   where x.OrganizationEmail == emailID
                                   select x.OrganizationEmail).FirstOrDefault();

                if (string.IsNullOrEmpty(orgEmail))
                {
                    isvalid = true;
                }
            }
            return isvalid;
        }

        private bool IsValidRRFNumber(int rrfNo, int rrfNumbers)
        {
            bool isvalid = false;
            using (PhoenixEntities dbContext = new PhoenixEntities())
            {
                int rrfNumber = (from x in dbContext.TARRFDetail
                                 where x.RRFNo == rrfNo && x.RRFNumber == rrfNumbers && x.Status == 0
                                 select x.RRFNumber).FirstOrDefault();

                if (rrfNumber > 0)
                {
                    isvalid = true;
                }
            }
            return isvalid;
        }

        private bool IsCandidateExist(string emailID, string mobileNumber)
        {
            bool isCandidateExist = false;
            using (PhoenixEntities dbContext = new PhoenixEntities())
            {
                int candidateID = (from x in dbContext.CandidatePersonal
                                   where x.PersonalEmail == emailID && x.Mobile == mobileNumber
                                   select x.CandidateID).FirstOrDefault();

                if (candidateID > 0)
                {
                    isCandidateExist = true;
                }
            }
            return isCandidateExist;
        }

        public async Task<IEnumerable<string>> GetRRFNumbers()
        {
            return await Task.Run(() =>
            {
                IEnumerable<string> RRFNumber = new List<string>();
                RRFNumber = service.All<TARRFDetail>().Where(x => x.Status == 0).Select(t => t.RRFNo + "." + t.RRFNumber);
                return RRFNumber;
            });
        }
        #endregion

        #region Contract Conversion Case
        private int MigrateEmployee(CandidateViewModel model)
        {
            int nextPersonID = 0;
            int iSSuccess = 0;
            using (PhoenixEntities dbContext = new PhoenixEntities())
            {
                nextPersonID = GenerateNextPersionID(model);
                // Call SP
                iSSuccess = dbContext.EmployeeConversion(model.OldPersonID,
                                             nextPersonID,
                                             model.FirstName,
                                             model.LastName,
                                             model.MiddleName,
                                             model.JoiningDate,
                                             model.DesignationID,
                                             model.ProbationReviewDate,
                                             model.OrganizationEmail,
                                             model.RejoinedWithinYear,
                                             model.EmployeeType,
                                             model.EmploymentStatus,
                                             model.DeliveryUnit,
                                             model.OfficeLocation,
                                             model.WorkLocation,
                                             model.ExitProcessManager,
                                             model.ReportingTo);

                //Send Notification
                CandidateToEmployee candidateToEmployee = new CandidateToEmployee();
                candidateToEmployee = FillCandidateToEmployee(model, nextPersonID);
                emailService.SendNewEmployeeEmail(candidateToEmployee);
                emailService.SendContractCompletionEmail(model, nextPersonID);

            }
            return nextPersonID;
        }
        #endregion

        public async Task<int> SyncCandidate(string ListUrl, string DetailUrl, string clientID, string clSecret, int dayMargin)
        {
            bool response = await syn.InitiateEmployeeSynchronization(ListUrl, DetailUrl, clientID, clSecret, dayMargin);

            if (response)
                return 1;
            else
                return 0;
        }
    }
}
