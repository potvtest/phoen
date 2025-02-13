using AutoMapper;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Pheonix.Core.Helpers;
using Pheonix.Core.v1.Services.Approval;
using Pheonix.Core.v1.Services.Email;
using Pheonix.DBContext;
using Pheonix.DBContext.Repository;
using Pheonix.Helpers;
using Pheonix.Models;
using Pheonix.Models.VM;
using Pheonix.Models.VM.Classes.Employee;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Web;

namespace Pheonix.Core.v1.Services.Business
{
    public class ApprovalUser
    {
        public int empId { get; set; }
        public string empName { get; set; }
        public string empEmail { get; set; }
        public string empImage { get; set; }
        public string currentUserEmail { get; set; }
        public string cardName { get; set; }
    }

    public abstract class UpdateApprovalStrategy<T> where T : new()
    {
        private ApprovalUser _user;
        private IBasicOperationsService _service;
        private IEmailService _emailService;
        private ISaveToStageService _saveService;
        private T _model;
        private int _status;
        private Func<int, ChangeSet<T>, bool, ChangeSet<T>> _updateWith;

        public UpdateApprovalStrategy(IBasicOperationsService service, IEmailService emailService, ISaveToStageService saveService, ApprovalUser user, T model, int status, Func<int, ChangeSet<T>, bool, ChangeSet<T>> updateWith)
        {
            this._user = user;
            this._service = service;
            this._saveService = saveService;
            this._emailService = emailService;
            this._model = model;
            this._status = status;
            this._updateWith = updateWith;
        }

        public abstract ChangeSet<T> GetRecords<T>(string moduleCode, int moduleId, string previousEntry, string newEntry);

        protected async Task<int> UpdateHookedApproval(int userId, int componentID, int recordID, int statusID, string statusComment)
        {
            var strategy = ApprovalStrategyFactory.GetStrategy(ApprovalStrategy.HrOnly, userId);
            strategy.opsService = _service;
            ApprovalService approvalService = new ApprovalService(_service);
            int requestID = (recordID != 0) ? recordID : 2;
            return await approvalService.UpdateApproval(userId, 2, requestID, statusID, statusComment, componentID);
        }

        public async Task<int> ProcessSingle(Stage oldStageModel, string comments, bool isMultiple = false)
        {
            var details = GetRecords<T>(oldStageModel.ModuleCode, oldStageModel.ModuleID.Value, oldStageModel.PreviousEntry, oldStageModel.NewEntry);
            if (_status == 1)
            {
                var changeSet = _updateWith(_user.empId, details, isMultiple);
                if (changeSet == null)
                    throw new ArgumentNullException("Change set can not be null after update");
            }
            var newStageModel = new Stage
            {
                ApprovalStatus = _status
            };
            bool isUpdated = _service.Update<Stage>(newStageModel, oldStageModel);
            _service.Finalize(isUpdated);

            await UpdateHookedApproval(_user.empId, oldStageModel.ModuleID.Value, oldStageModel.RecordID.Value, _status, comments);

            _emailService.SendUserProfileStatus(_user.empId, _user.empName, _user.cardName, _user.currentUserEmail, _user.empEmail, _status, _user.empImage);

            return 0;
        }

        public async Task<int> ProcessMultiple(MultiRecordStage oldStageModel, string comments, bool isMultiple = false)
        {
            var details = GetRecords<T>(oldStageModel.ModuleCode, oldStageModel.ModuleID.Value, oldStageModel.PreviousEntry, oldStageModel.NewEntry);
            if (_status == 1)
            {
                if (oldStageModel.StatusID == 3) isMultiple = true;
                var changeSet = _updateWith(_user.empId, details, isMultiple);
                if (changeSet == null)
                    throw new ArgumentNullException("Change set can not be null after update");
            }
            var newStageModel = new MultiRecordStage
            {
                ApprovalStatus = _status
            };
            bool isUpdated = _service.Update<MultiRecordStage>(newStageModel, oldStageModel);
            _service.Finalize(isUpdated);

            await UpdateHookedApproval(_user.empId, oldStageModel.ModuleID.Value, oldStageModel.RecordID.Value, _status, comments);

            _emailService.SendUserProfileStatus(_user.empId, _user.empName, _user.cardName, _user.currentUserEmail, _user.empEmail, _status, _user.empImage);

            return 0;
        }
    }

    public class SingleCardApprovalStrategy<T> : UpdateApprovalStrategy<T> where T : new()
    {
        public SingleCardApprovalStrategy(IBasicOperationsService service, IEmailService emailService, ISaveToStageService saveService, ApprovalUser user, T model, int statusId, Func<int, ChangeSet<T>, bool, ChangeSet<T>> updateWith)
            : base(service, emailService, saveService, user, model, statusId, updateWith) { }

        public override ChangeSet<T> GetRecords<T>(string moduleCode, int moduleId, string previousEntry, string newEntry)
        {
            ChangeSet<T> empPersonalDetails = new ChangeSet<T>
            {
                ModuleCode = moduleCode,
                ModuleId = moduleId,
                NewModel = JsonConvert.DeserializeObject<T>(newEntry, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() }),
                OldModel = JsonConvert.DeserializeObject<T>(previousEntry, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() }),
                SendForApproval = false
            };

            return empPersonalDetails;
        }
    }

    public class EmployeeService : IEmployeeService
    {
        #region Class level variables

        private IBasicOperationsService service;
        private ISaveToStageService _stageService;
        private IEmailService emailService;

        #endregion Class level variables

        #region Constructor

        public EmployeeService(IContextRepository repository, IBasicOperationsService opsService, ISaveToStageService stageService, IEmailService opsEmailService)
        {
            service = opsService;
            _stageService = stageService;
            emailService = opsEmailService;
        }

        #endregion Constructor

        #region Public Contracts

        public async Task<IEnumerable<PersonViewModel>> List(string filters = null)
        {
            return await Task.Run(() =>
            {
                var result = service.All<Person>();
                if (result.Any())
                    return Mapper.Map<IEnumerable<Person>, IEnumerable<PersonViewModel>>(result);
                return null;
            });
        }

        public Task<IEnumerable<PersonViewModel>> Single(string filters = null)
        {
            throw new NotImplementedException();
        }

        public int Add(PersonViewModel model)
        {
            var obj = Mapper.Map<PersonViewModel, Person>(model);

            bool isCreated = service.Create(obj,
                m => m.FirstName == obj.FirstName
                    && m.LastName == obj.LastName);

            return service.Finalize(isCreated);
        }

        public int Update(PersonViewModel model)
        {
            var obj = Mapper.Map<PersonViewModel, Person>(model);
            bool isUpdated = service.Update(obj, obj);
            return service.Finalize(isUpdated);
        }

        public int Delete(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<EmployeeProfileViewModel> GetProfile(int id, bool isMyRecord)
        {
            return await Task.Run(() =>
            {
                bool isRRFRole = false;
                var employee = service.First<Person>(x => x.ID == id);
                if (employee != null)
                {
                    var viewModel = Mapper.Map<Person, EmployeeProfileViewModel>(employee);
                    viewModel.RejoinedWithinYearDdl = viewModel.RejoinedWithinYear ? "Yes" : "No";
                    viewModel.ActiveDdl = viewModel.Active ? 0 : 1;
                    viewModel.Qualifications = Mapper.Map<List<PersonQualificationMapping>, List<EmployeeQualification>>(employee.PersonQualificationMappings.Where(x => x.IsDeleted == false).OrderByDescending(x => x.Year).ToList());
                    viewModel.Visas = Mapper.Map<List<PersonVisa>, List<EmployeeVisa>>(employee.PersonVisa.Where(x => x.IsDeleted == false).ToList());
                    viewModel.Addresses = Mapper.Map<List<PersonAddress>, List<EmployeeAddress>>(employee.PersonAddresses.Where(x => x.IsDeleted == false).ToList());
                    if (viewModel.Addresses.Count() == 1)
                    {
                        (viewModel.Addresses as List<EmployeeAddress>).Add(new EmployeeAddress()
                        {
                            AddressLabel = "Permanent Address",
                            CurrentAddress = viewModel.Addresses.Where(x => x.IsCurrent == true).First().CurrentAddress,
                            CurrentAddressCountry = viewModel.Addresses.Where(x => x.IsCurrent == true).First().CurrentAddressCountry,
                            IsCurrent = false,
                            ID = 0,
                            City = viewModel.Addresses.Where(x => x.IsCurrent == true).First().City,
                            State = viewModel.Addresses.Where(x => x.IsCurrent == true).First().State,
                            Pin = viewModel.Addresses.Where(x => x.IsCurrent == true).First().Pin,
                        });
                    }
                    PhoenixEntities entities = new PhoenixEntities();
                    var grade = (from pe in entities.PersonEmployment
                                join d in entities.Designations on pe.DesignationID equals d.ID
                                where pe.PersonID == id
                                select new { result = d.Grade }).FirstOrDefault();
                    viewModel.grade = (int)grade.result;
                    #region Get Reporting Manager & Exit Process Manager

                    var empOrganizationDetails = service.First<PersonReporting>(x => x.PersonID == id);
                    var empEmployment = employee.PersonEmployment.First();
                    viewModel.EmployeeOrganizationdetails = Mapper.Map<PersonEmployment, EmployeeOrganizaionDetails>(empEmployment);
                    if (empOrganizationDetails != null)
                    {
                        viewModel.EmployeeOrganizationdetails.ReportingTo = empOrganizationDetails.ReportingTo;
                        var reportingManager = service.First<Person>(x => x.ID == empOrganizationDetails.ReportingTo);
                        viewModel.EmployeeOrganizationdetails.ReportingManager = reportingManager.FirstName + " " + reportingManager.LastName;
                    }
                    else
                    {
                        viewModel.EmployeeOrganizationdetails.ReportingManager = "";
                        viewModel.EmployeeOrganizationdetails.ReportingTo = 0;
                    }
                    if (empEmployment.ExitProcessManager != null)
                    {
                        int exitProcessManager = employee.PersonEmployment.First().ExitProcessManager.Value;
                        var exitProcessManagerName = service.First<Person>(x => x.ID == exitProcessManager);
                        viewModel.EmployeeOrganizationdetails.ExitProcessManager = exitProcessManagerName.ID;
                    }

                    using (PhoenixEntities context = new PhoenixEntities())
                    {
                        int currentUserId = id;
                        GetReportingManager_Result data = QueryHelper.GetManger(currentUserId);
                        if (data != null)
                        {
                            viewModel.R1 = new EmployeeManagerViewModel
                            {
                                Name = data.Name,
                                id = data.ID
                            };
                            currentUserId = data.ID;
                            data = QueryHelper.GetManger(currentUserId);
                            if (data != null)
                            {
                                viewModel.R2 = new EmployeeManagerViewModel
                                {
                                    Name = data.Name,
                                    id = data.ID
                                };
                            }
                        }
                        else
                        {
                            viewModel.R1 = new EmployeeManagerViewModel();
                            viewModel.R2 = new EmployeeManagerViewModel();
                        }
                        int officeLocationId = viewModel.OL;
                        viewModel.OLText = context.WorkLocation.Where(x => x.ID == officeLocationId).Select(y => y.LocationName).FirstOrDefault();
                        int workLocationId = viewModel.EmployeeOrganizationdetails.WorkLocation ?? default(int);
                        viewModel.EmployeeOrganizationdetails.WLText = context.WorkLocation.Where(x => x.ID == workLocationId).Select(y => y.LocationName).FirstOrDefault();
                        List<EmploymentHelpDeskCategories> lstHelpDeskCategories = new List<EmploymentHelpDeskCategories>();
                        if (employee.PersonInRole != null)
                        {
                            viewModel.Role = Mapper.Map<List<PersonInRole>, List<EmployeeRole>>(employee.PersonInRole.Where(x => x.IsDeleted == false).ToList());
                            if (viewModel.Role != null)
                            {
                                foreach (var item in viewModel.Role.ToList())
                                {
                                    EmploymentHelpDeskCategories helpDeskCategories = new EmploymentHelpDeskCategories();
                                    helpDeskCategories = context.HelpDeskCategories.Where(x => x.AssignedRole == item.roleId || x.AssignedExecutiveRole == item.roleId).Select(s => new EmploymentHelpDeskCategories { Id = s.Prefix, Text = s.Name }).FirstOrDefault();
                                    if (helpDeskCategories != null)
                                    {
                                        lstHelpDeskCategories.Add(helpDeskCategories);
                                    }
                                }
                            }
                        }
                        viewModel.HelpDeskCategories = lstHelpDeskCategories;

                        foreach (var item in viewModel.Role.ToList())
                        {
                            isRRFRole = (item.role == "CEO" || item.role == "Director" || item.role == "Accounting VP" || item.role == "Development VP" || item.role == "Marketing VP" || item.role == "Human Resources VP" || item.role == "Sales VP" || item.role == "Accounting Manager" || item.role == "HR Manager" || item.role == "Delivery Manager" || item.role == "Finance VP" || item.role == "Finance Admin" || item.role == "HR Admin" || item.role == "IT Admin" || item.role == "RMG Admin" || item.role == "PMO Admin" || item.role == "Vibrant Web Admin" || item.role == "Normalizer");

                            if (isRRFRole) break;
                        }
                    }
                    #endregion

                    viewModel.EmployeePassport = new List<EmployeePassport>();
                    viewModel.EmployeePassport = Mapper.Map<List<PersonPassport>, List<EmployeePassport>>(employee.PersonPassport.Where(x => x.RelationWithPPHolder == 1 && x.IsDeleted == false).OrderByDescending(x => x.DateOfExpiry).ToList());
                    viewModel.EmergencyContacts = Mapper.Map<List<PersonContact>, List<EmployeeEmergencyContact>>(employee.PersonContacts.Where(x => x.IsDeleted == false).ToList());
                    viewModel.Dependents = Mapper.Map<List<PersonDependent>, List<EmployeeDependent>>(employee.PersonDependents.Where(x => x.IsDeleted == false).ToList());
                    viewModel.Declarations = Mapper.Map<List<PersonDeclaration>, List<EmployeeDeclaration>>(employee.PersonDeclarations.Where(x => x.IsDeleted == false).ToList());
                    viewModel.Skills = Mapper.Map<List<PersonSkillMapping>, List<EmployeeSkill>>(employee.PersonSkillMappings.Where(x => x.IsDeleted == false).ToList());

                    var allSkillMappings = employee.PersonSkillMappings.Where(x => x.IsDeleted == false).ToList(); // Retrieve all skill mappings associated with the employee

                    var primarySkillMappings = allSkillMappings.Where(x => x.IsPrimary).ToList(); // Filter primary skill mappings
                    viewModel.PrimarySkills = Mapper.Map<List<PersonSkillMapping>, List<EmployeeSkill>>(primarySkillMappings); // Map primary skill mappings to EmployeeSkill objects for the ViewModel

                    var secondarySkillMappings = allSkillMappings.Where(x => !x.IsPrimary).ToList(); // Filter secondary skill mappings
                    viewModel.SecondarySkills = Mapper.Map<List<PersonSkillMapping>, List<EmployeeSkill>>(secondarySkillMappings); // Map secondary skill mappings to EmployeeSkill objects for the ViewModel

                    viewModel.Certifications = Mapper.Map<List<PersonCertification>, List<EmployeeCertification>>(employee.PersonCertification.Where(x => x.IsDeleted == false).OrderByDescending(c => c.CertificationDate.Value).ToList());
                    viewModel.EmployeeHistories = Mapper.Map<List<PersonEmploymentHistory>, List<EmployeeEmploymentHistory>>(employee.PersonEmploymentHistories.Where(x => x.IsDeleted == false).OrderByDescending(x => x.WorkedTill).ToList());
                    viewModel.EmployeeMedicalHistories = Mapper.Map<List<PersonMedicalHistory>, List<EmployeeMedicalHistory>>(employee.PersonMedicalHistories.Where(x => x.IsDeleted == false).OrderByDescending(x => x.ID).ToList());
                    viewModel.extension = service.Top<PersonEmployment>(0, x => (x.PersonID == id)).FirstOrDefault().OfficeExtension;
                    viewModel.location = service.Top<PersonEmployment>(0, x => (x.PersonID == id)).FirstOrDefault().SeatingLocation;
                    viewModel.CompetencyID = service.Top<PersonEmployment>(0, x => x.PersonID == id).FirstOrDefault().CompetencyID;
                    viewModel.SkillDescription = service.Top<PersonEmployment>(0, x => (x.PersonID == id)).FirstOrDefault().SkillDescription;
                    viewModel.PersonBGMapping = service.All<PersonBGMapping>(x => x.PersonID == id).ToList();
                    viewModel.PersonBGMapping = viewModel.PersonBGMapping
                                                            .GroupBy(item => item.BGParameterID)
                                                            .Select(g => g.OrderBy(y => y.BGParameterID).First()).ToList();
                    //viewModel.PersonBGMapping = viewModel.PersonBGMapping
                    //                            .GroupBy(item => item.BGParameterID)
                    //                            .SelectMany(g => g.Count() > 1 ? g.Where(x => x.ProjectID != null) : g).ToList();
                    // RS: It would be nice if we move this thing to builder pattern, let's take up in clean up
                    var multiRecordItems = service.Top<MultiRecordStage>(0, x => x.By == id && (x.ApprovalStatus == 0 || x.ApprovalStatus == 3));
                    // ND : This code needs to be refactored.
                    // This code it to check whether the card is having any data in MultiRecordStage and if yes then it will have StatusID as 1 else 0.

                    #region CheckMultiRecordStage

                    foreach (var item in viewModel.Addresses.ToList())
                    {
                        item.StageStatusID = multiRecordItems.Where(x => x.RecordID == item.ID && x.ModuleID == 2).Count() > 0 ? 1 : 0;
                    }
                    foreach (var item in multiRecordItems.Where(x => x.ModuleID == 2 && x.RecordID == 0).ToList())
                    {
                        var newAddressModel = JsonConvert.DeserializeObject<EmployeeAddress>(item.NewEntry, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
                        newAddressModel.StageStatusID = 1;
                        var empAddresses = viewModel.Addresses.ToList();
                        empAddresses.Add(newAddressModel);
                        viewModel.Addresses = empAddresses;
                    }
                    foreach (var item in viewModel.EmergencyContacts.ToList())
                    {
                        item.StageStatusID = multiRecordItems.Where(x => x.RecordID == item.ID && x.ModuleID == 5).Count() > 0 ? 1 : 0;
                    }
                    foreach (var item in multiRecordItems.Where(x => x.ModuleID == 5 && x.RecordID == 0).ToList())
                    {
                        var newEmergencyModel = JsonConvert.DeserializeObject<EmployeeEmergencyContact>(item.NewEntry, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
                        newEmergencyModel.StageStatusID = 1;
                        var emergencyContacts = viewModel.EmergencyContacts.ToList();
                        emergencyContacts.Add(newEmergencyModel);
                        viewModel.EmergencyContacts = emergencyContacts;
                    }
                    foreach (var item in viewModel.EmployeePassport.ToList())
                    {
                        item.StageStatusID = multiRecordItems.Where(x => x.RecordID == item.ID && x.ModuleID == 4).Count() > 0 ? 1 : 0;
                    }
                    foreach (var item in multiRecordItems.Where(x => x.ModuleID == 4 && x.RecordID == 0).ToList())
                    {
                        var newPassportModel = JsonConvert.DeserializeObject<EmployeePassport>(item.NewEntry, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
                        newPassportModel.StageStatusID = 1;
                        var employeePassport = viewModel.EmployeePassport.ToList();
                        employeePassport.Add(newPassportModel);
                        viewModel.EmployeePassport = employeePassport;
                    }
                    foreach (var item in viewModel.Visas.ToList())
                    {
                        item.StageStatusID = multiRecordItems.Where(x => x.RecordID == item.ID && x.ModuleID == 8).Count() > 0 ? 1 : 0;
                    }
                    foreach (var item in multiRecordItems.Where(x => x.ModuleID == 8 && x.RecordID == 0).ToList())
                    {
                        var newVisaModel = JsonConvert.DeserializeObject<EmployeeVisa>(item.NewEntry, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
                        newVisaModel.StageStatusID = 1;
                        var employeeVisa = viewModel.Visas.ToList();
                        employeeVisa.Add(newVisaModel);
                        viewModel.Visas = employeeVisa;
                    }
                    foreach (var item in viewModel.Qualifications.ToList())
                    {
                        item.StageStatusID = multiRecordItems.Where(x => x.RecordID == item.ID && x.ModuleID == 3).Count() > 0 ? 1 : 0;
                    }
                    foreach (var item in multiRecordItems.Where(x => x.ModuleID == 3 && x.RecordID == 0).ToList())
                    {
                        var newQualificationModel = JsonConvert.DeserializeObject<EmployeeQualification>(item.NewEntry, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
                        newQualificationModel.StageStatusID = 1;
                        var employeeVisa = viewModel.Qualifications.ToList();
                        employeeVisa.Add(newQualificationModel);
                        viewModel.Qualifications = employeeVisa;
                    }
                    foreach (var item in viewModel.Addresses.ToList())
                    {
                        item.StageStatusID = multiRecordItems.Where(x => x.RecordID == item.ID && x.ModuleID == 2).Count() > 0 ? 1 : 0;
                    }
                    foreach (var item in multiRecordItems.Where(x => x.ModuleID == 2 && x.RecordID == 0).ToList())
                    {
                        var newAddressModel = JsonConvert.DeserializeObject<EmployeeAddress>(item.NewEntry, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
                        newAddressModel.StageStatusID = 1;
                        var employeeVisa = viewModel.Addresses.ToList();
                        employeeVisa.Add(newAddressModel);
                        viewModel.Addresses = employeeVisa;
                    }
                    foreach (var item in viewModel.Dependents.ToList())
                    {
                        item.StageStatusID = multiRecordItems.Where(x => x.RecordID == item.ID && x.ModuleID == 17).Count() > 0 ? 1 : 0;
                    }
                    foreach (var item in multiRecordItems.Where(x => x.ModuleID == 17 && x.RecordID == 0).ToList())
                    {
                        var newDependentModel = JsonConvert.DeserializeObject<EmployeeDependent>(item.NewEntry, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
                        newDependentModel.StageStatusID = 1;
                        var employeeVisa = viewModel.Dependents.ToList();
                        employeeVisa.Add(newDependentModel);
                        viewModel.Dependents = employeeVisa;
                    }
                    foreach (var item in viewModel.Declarations.ToList())
                    {
                        item.StageStatusID = multiRecordItems.Where(x => x.RecordID == item.ID && x.ModuleID == 18).Count() > 0 ? 1 : 0;
                    }
                    foreach (var item in multiRecordItems.Where(x => x.ModuleID == 18 && x.RecordID == 0).ToList())
                    {
                        var newDeclarationModel = JsonConvert.DeserializeObject<EmployeeDeclaration>(item.NewEntry, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
                        newDeclarationModel.StageStatusID = 1;
                        var employeeVisa = viewModel.Declarations.ToList();
                        employeeVisa.Add(newDeclarationModel);
                        viewModel.Declarations = employeeVisa;
                    }
                    //foreach (var item in viewModel.Skills.ToList())
                    //{
                    //    item.StageStatusID = multiRecordItems.Where(x => x.RecordID == item.ID && x.ModuleID == 16).Count() > 0 ? 1 : 0;
                    //}
                    //foreach (var item in multiRecordItems.Where(x => x.ModuleID == 16 && x.RecordID == 0).ToList())
                    //{
                    //    var newSkillModel = JsonConvert.DeserializeObject<EmployeeSkill>(item.NewEntry, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
                    //    newSkillModel.StageStatusID = 1;
                    //    var employeeVisa = viewModel.Skills.ToList();
                    //    employeeVisa.Add(newSkillModel);
                    //    viewModel.Skills = employeeVisa;
                    //}

                    foreach (var item in viewModel.PrimarySkills.ToList())
                    {
                        item.StageStatusID = multiRecordItems.Where(x => x.RecordID == item.ID && x.ModuleID == 16).Count() > 0 ? 1 : 0;
                    }

                    foreach (var item in multiRecordItems.Where(x => x.ModuleID == 16 && x.RecordID == 0).ToList())
                    {
                        var newSkillModel = JsonConvert.DeserializeObject<EmployeeSkill>(item.NewEntry, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
                        newSkillModel.StageStatusID = 1;
                        var primarySkills = viewModel.PrimarySkills.ToList();
                        primarySkills.Add(newSkillModel);
                        viewModel.Skills = primarySkills.Concat(viewModel.SecondarySkills).ToList();
                    }

                    foreach (var item in viewModel.Certifications.ToList())
                    {
                        item.StageStatusID = multiRecordItems.Where(x => x.RecordID == item.ID && x.ModuleID == 6).Count() > 0 ? 1 : 0;
                    }
                    foreach (var item in multiRecordItems.Where(x => x.ModuleID == 6 && x.RecordID == 0).ToList())
                    {
                        var newCertificationModel = JsonConvert.DeserializeObject<EmployeeCertification>(item.NewEntry, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
                        newCertificationModel.StageStatusID = 1;
                        var employeeVisa = viewModel.Certifications.ToList();
                        employeeVisa.Add(newCertificationModel);
                        viewModel.Certifications = employeeVisa;
                    }
                    foreach (var item in viewModel.EmployeeHistories.ToList())
                    {
                        item.StageStatusID = multiRecordItems.Where(x => x.RecordID == item.ID && x.ModuleID == 7).Count() > 0 ? 1 : 0;
                    }
                    foreach (var item in multiRecordItems.Where(x => x.ModuleID == 7 && x.RecordID == 0).ToList())
                    {
                        var newEmpHistoryModel = JsonConvert.DeserializeObject<EmployeeEmploymentHistory>(item.NewEntry, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
                        newEmpHistoryModel.StageStatusID = 1;
                        var employeeVisa = viewModel.EmployeeHistories.ToList();
                        employeeVisa.Add(newEmpHistoryModel);
                        viewModel.EmployeeHistories = employeeVisa;
                    }
                    foreach (var item in viewModel.EmployeeMedicalHistories.ToList())
                    {
                        item.StageStatusID = multiRecordItems.Where(x => x.RecordID == item.ID && x.ModuleID == 10).Count() > 0 ? 1 : 0;
                    }
                    foreach (var item in multiRecordItems.Where(x => x.ModuleID == 10 && x.RecordID == 0).ToList())
                    {
                        var newMedHistoryModel = JsonConvert.DeserializeObject<EmployeeMedicalHistory>(item.NewEntry, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
                        newMedHistoryModel.StageStatusID = 1;
                        var employeeVisa = viewModel.EmployeeMedicalHistories.ToList();
                        employeeVisa.Add(newMedHistoryModel);
                        viewModel.EmployeeMedicalHistories = employeeVisa;
                    }


                    #region Partial Commit for Code Refactoring
                    //var addressList = Mapper.Map<List<PersonAddress>, List<EmployeeAddress>>(employee.PersonAddresses.Where(x => x.IsDeleted == false).ToList());
                    //viewModel.Addresses = addressList;
                    //if (viewModel.Addresses.Count() == 1)
                    //{
                    //    (viewModel.Addresses as List<EmployeeAddress>).Add(new EmployeeAddress()
                    //    {
                    //        AddressLabel = "Permanent Address",
                    //        CurrentAddress = viewModel.Addresses.Where(x => x.IsCurrent == true).First().CurrentAddress,
                    //        CurrentAddressCountry = viewModel.Addresses.Where(x => x.IsCurrent == true).First().CurrentAddressCountry,
                    //        IsCurrent = false,
                    //        ID = 0
                    //    });
                    //}
                    //foreach (var multiRecordItem in multiRecordItems.GroupBy(x => x.ModuleID).ToList())
                    //{
                    //    int moduleId = multiRecordItem.First().ModuleID.Value;
                    //    switch (moduleId)
                    //    {
                    //        default:
                    //            viewModel.Addresses = CheckMulti<EmployeeAddress>(viewModel, multiRecordItems, multiRecordItem, addressList);
                    //            break;
                    //    }
                    //}
                    #endregion





                    #endregion CheckMultiRecordStage

                    if (isMyRecord)
                    {
                        string myCardIdsList = service.First<PhoenixConfig>(x => x.ConfigKey == "MyCards").ConfigValue;
                        string approvalCardIdsList = service.First<PhoenixConfig>(x => x.ConfigKey == "CardsForApproval").ConfigValue;

                        viewModel.Cards = JsonConvert.DeserializeObject<int[]>(myCardIdsList);
                        viewModel.Approvals = JsonConvert.DeserializeObject<int[]>(approvalCardIdsList);
                    }
                    else
                    {
                        string searchCardIdsList = service.First<Navigation>(x => x.Navigation1 == "Search").Components;
                        viewModel.Cards = JsonConvert.DeserializeObject<int[]>(searchCardIdsList);
                        viewModel.Approvals = new int[0];
                    }
                    var employmentstatusdetails = service.First<EmploymentStatus>(x => x.Id == empEmployment.EmploymentStatus);

                    viewModel.employmentStatus = employmentstatusdetails.Description;


                    viewModel.IsConfirmationHistoryPresent = GetConfirmationHistrory(id, viewModel.Role);
                    viewModel.employmentStatusID = empEmployment.EmploymentStatus.Value;

                    var isDM = employee.PersonEmployment.Where(x => x.Designation.Name == "Director-Strategic Data Solutions" || x.Designation.Name == "Sr.Programme Director" || x.Designation.Name == "RMG Head" || x.Designation.Name == "Delivery Manager" || x.Designation.Name == "Business HR Head" || x.Designation.Name == "Recruitment Head" || x.Designation.Name == "Sr.Director - Software Delivery" || x.Designation.Name == "Director - Software Delivery" || x.Designation.Name == "Director" || x.Designation.Name == "Consultant - Account Owner" || x.Designation.Name == "Consultant - Sr.Delivery Manager" || x.Designation.Name == "Director Of Engineering" || x.Designation.Name == "VP – Creative Director" || x.Designation.Name == "SVP - Business Development" || x.Designation.Name == "Director - Digital marketing" || x.Designation.Name == "Vice President - Marketing" || x.Designation.Name == "Consultant - Vice President" || x.Designation.Name == "Chief Operating Officer" || x.Designation.Name == "Chief Finance Officer" || x.Designation.Name == "Chief Executive Officer" || x.Designation.Name == "VP - Business Development" || x.Designation.Name == "Sr.VP - M & E" || x.Designation.Name == "Vice President" || x.Designation.Name == "Sr. Manager - Finance" || x.Designation.Name == "Sr.Project Manager - Digital Services" || x.Designation.Name == "Technology Manager" || x.Designation.Name == "Manager - PMO").Select(a => a.DesignationID).FirstOrDefault();

                    viewModel.IsRRF = (isDM != null && isDM > 0) || isRRFRole;

                    return viewModel;
                }
                return null;
            });
        }

        private bool GetConfirmationHistrory(int id, IEnumerable<IEmployeeRole> Role)
        {
            if (Role.Any(x => x.roleId == 24))
            {
                return true;
            }
            else
            {
                var result = service.All<PersonConfirmation>(x => x.ReportingManagerId == id).ToList();
                if (result.Count > 0) { return true; }
                else { return false; }
            }
        }

        #region Partial Commit for Code Refactoring
        //private static IEnumerable<T> CheckMulti<T>(EmployeeProfileViewModel viewModel, IEnumerable<MultiRecordStage> multiRecordItems, IGrouping<int?, MultiRecordStage> multiRecordItem, List<T> viewList) where T : Pheonix.Models.VM.IBaseModel
        //{
        //    foreach (var item in viewList.ToList())
        //    {
        //        item.StageStatusID = multiRecordItems.Where(x => x.RecordID == item.ID && x.ModuleID == multiRecordItem.First().ModuleID).Count() > 0 ? 1 : 0;
        //    }
        //    foreach (var item in multiRecordItems.Where(x => x.ModuleID == multiRecordItem.First().ModuleID && x.RecordID == 0).ToList())
        //    {
        //        var newAddressModel = JsonConvert.DeserializeObject<T>(item.NewEntry, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
        //        newAddressModel.StageStatusID = 1;
        //        var empAddresses = viewList.ToList();
        //        empAddresses.Add(newAddressModel);
        //        viewList = empAddresses;
        //    }
        //    return viewList;
        //}
        #endregion

        //private int GetItemStatus(int empID, int moduleID, IBaseModel model)
        //{
        //    int statusID = 0;

        //    var multiRecordItems = service.Top<MultiRecordStage>(0, x => x.By == empID && x.ModuleID == moduleID && x.ApprovalStatus != 1);
        //    if (multiRecordItems.Count() > 0)
        //        statusID = multiRecordItems.Where(x => x.RecordID == model.ID && x.ModuleID == 5).Count() > 0 ? 1 : 0;

        //    return statusID;
        //}

        public async Task<IEnumerable<int>> GetCardStatus(int id, bool isMyRecord)
        {
            return await Task.Run(() =>
            {
                List<int> cardSendForApproval = new List<int>();
                if (isMyRecord)
                {
                    var stageApprovalsList = service.Top<Stage>(0, x => x.IsDeleted == false && (x.ApprovalStatus == 0 || x.ApprovalStatus == 3) && x.By == id).ToList();
                    var multiRecordApprovalList = service.Top<MultiRecordStage>(0, x => x.IsDeleted == false && (x.ApprovalStatus == 0 || x.ApprovalStatus == 3) && x.By == id).ToList();
                    foreach (var item in stageApprovalsList.GroupBy(x => x.ModuleID))
                    {
                        cardSendForApproval.Add((int)item.Key);
                    }
                    foreach (var item in multiRecordApprovalList.GroupBy(x => x.ModuleID))
                    {
                        cardSendForApproval.Add((int)item.Key);
                    }
                }
                return cardSendForApproval;
            });
        }

        #region Redundant Method To Remove.

        public Task<IEnumerable<T>> List<T>(string filters = null)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<T>> Single<T>(string filters = null)
        {
            throw new NotImplementedException();
        }

        public int Add<T>(T model)
        {
            throw new NotImplementedException();
        }

        public int Update<T>(T model)
        {
            //throw new NotImplementedException();
            var employee = Mapper.Map<Person>(model);
            var oldEmployee = service.First<Person>(x => x.ID == employee.ID);
            bool IsUpdated = service.Update<Person>(employee, oldEmployee);
            return service.Finalize(IsUpdated);
        }

        public int Delete<T>(int id)
        {
            throw new NotImplementedException();
        }

        #endregion Redundant Method To Remove.

        public async Task<IEnumerable<PersonViewModel>> GetAllEmployess(string EmpListFor)
        {
            return await Task.Run(() =>
            {
                using (PhoenixEntities dbContext = new PhoenixEntities())
                {
                    List<PersonViewModel> lstEmpList = new List<PersonViewModel>();
                    try
                    {
                        if (EmpListFor == "allactive")
                        {
                            lstEmpList = (from p in dbContext.People
                                          where p.Active == true
                                          select new PersonViewModel
                                          {
                                              Active = p.Active,
                                              DateOfBirth = p.DateOfBirth.Value,
                                              FirstName = p.FirstName,
                                              Gender = p.Gender.Value,
                                              ID = p.ID,
                                              LastName = p.LastName,
                                              MiddleName = p.MiddleName,
                                              EmploymentStatus = p.PersonEmployment.Where(x => x.PersonID == p.ID).Select(s => s.EmploymentStatus).FirstOrDefault(),
                                              PersonEmployment = (from pe in dbContext.PersonEmployment
                                                                  where pe.PersonID == p.ID
                                                                  select new PersonEmploymentViewModel
                                                                  {
                                                                      BusinessGroup = pe.BusinessGroup,
                                                                      Commitment = pe.Commitment,
                                                                      ConfirmationDate = pe.ConfirmationDate,
                                                                      CurrentDU = pe.CurrentDU,
                                                                      DeliveryTeam = pe.DeliveryTeam,
                                                                      DesignationID = pe.DesignationID,
                                                                      EmployeeType = pe.EmployeeType,
                                                                      EmploymentStatus = pe.EmploymentStatus,
                                                                      ExitDate = pe.ExitDate,
                                                                      ID = pe.ID,
                                                                      JoiningDate = pe.JoiningDate,
                                                                      OfficeExtension = pe.OfficeExtension,
                                                                      OrganizationEmail = pe.OrganizationEmail,
                                                                      OrgUnit = pe.OrgUnit,
                                                                      PersonID = pe.PersonID,
                                                                      ProbationMonths = pe.ProbationMonths,
                                                                      ProbationReviewDate = pe.ProbationReviewDate,
                                                                      RejoinedWithinYear = pe.RejoinedWithinYear,
                                                                      SeatingLocation = pe.SeatingLocation,
                                                                      SeparationRequestDate = pe.SeparationRequestDate,
                                                                      UserName = pe.UserName
                                                                  }).FirstOrDefault()

                                          }).ToList();
                        }
                        else
                        {
                            lstEmpList = (from p in dbContext.People
                                          select new PersonViewModel
                                          {
                                              Active = p.Active,
                                              DateOfBirth = p.DateOfBirth.Value,
                                              FirstName = p.FirstName,
                                              Gender = p.Gender.Value,
                                              ID = p.ID,
                                              LastName = p.LastName,
                                              MiddleName = p.MiddleName,
                                              EmploymentStatus = p.PersonEmployment.Where(x => x.PersonID == p.ID).Select(s => s.EmploymentStatus).FirstOrDefault(),
                                              PersonEmployment = (from pe in dbContext.PersonEmployment
                                                                  where pe.PersonID == p.ID
                                                                  select new PersonEmploymentViewModel
                                                                  {
                                                                      BusinessGroup = pe.BusinessGroup,
                                                                      Commitment = pe.Commitment,
                                                                      ConfirmationDate = pe.ConfirmationDate,
                                                                      CurrentDU = pe.CurrentDU,
                                                                      DeliveryTeam = pe.DeliveryTeam,
                                                                      DesignationID = pe.DesignationID,
                                                                      EmployeeType = pe.EmployeeType,
                                                                      EmploymentStatus = pe.EmploymentStatus,
                                                                      ExitDate = pe.ExitDate,
                                                                      ID = pe.ID,
                                                                      JoiningDate = pe.JoiningDate,
                                                                      OfficeExtension = pe.OfficeExtension,
                                                                      OrganizationEmail = pe.OrganizationEmail,
                                                                      OrgUnit = pe.OrgUnit,
                                                                      PersonID = pe.PersonID,
                                                                      ProbationMonths = pe.ProbationMonths,
                                                                      ProbationReviewDate = pe.ProbationReviewDate,
                                                                      RejoinedWithinYear = pe.RejoinedWithinYear,
                                                                      SeatingLocation = pe.SeatingLocation,
                                                                      SeparationRequestDate = pe.SeparationRequestDate,
                                                                      UserName = pe.UserName
                                                                  }).FirstOrDefault()
                                          }).ToList();
                        }
                        return lstEmpList;
                    }
                    catch (Exception ex)
                    {
                        return lstEmpList;
                    }
                }

            });
        }

        public async Task<IEnumerable<EmployeeSearchFilters>> GetEmployeeFilterList(int filterListFor)
        {
            return await Task.Run(() =>
            {
                PhoenixEntities dbContext = new PhoenixEntities();
                IEnumerable<EmployeeSearchFilters> employeeSearchFilters = new List<EmployeeSearchFilters>();
                // 1 for Skills 
                if (filterListFor == 1)
                {
                    employeeSearchFilters = (from sm in dbContext.SkillMatrices
                                             where sm.IsDeleted == false
                                             select new EmployeeSearchFilters
                                             {
                                                 ID = sm.ID,
                                                 Text = sm.Name
                                             });
                }
                // 2 for Resource Pool
                if (filterListFor == 2)
                {
                    employeeSearchFilters = (from rp in dbContext.ResourcePool
                                             where rp.IsDeleted == false
                                             select new EmployeeSearchFilters
                                             {
                                                 ID = rp.ID,
                                                 Text = rp.Name
                                             });
                }
                // 3 for Delivery Unit
                if (filterListFor == 3)
                {
                    employeeSearchFilters = (from du in dbContext.DeliveryUnit
                                             where du.Active == true
                                             select new EmployeeSearchFilters
                                             {
                                                 ID = du.ID,
                                                 Text = du.Name
                                             });
                }
                // 4 for Delivery Team
                if (filterListFor == 4)
                {
                    employeeSearchFilters = (from dt in dbContext.DeliveryTeam
                                             where dt.IsActive == true
                                             select new EmployeeSearchFilters
                                             {
                                                 ID = dt.ID,
                                                 Text = dt.Name
                                             });
                }
                return employeeSearchFilters;
            });
        }

        public async Task<IEnumerable<EmployeeBasicProfile>> SearchEmployessBasedOnFilter(EmployeeSearchCriteria objEmployeeSearchCriteria)
        {
            return await Task.Run(() =>
            {
                PhoenixEntities dbContext = new PhoenixEntities();
                List<EmployeeBasicProfile> lstEmployeeBasicProfile = new List<EmployeeBasicProfile>();
                IEnumerable<EmployeeBasicProfile> EmployeeSearchList = new List<EmployeeBasicProfile>();
                bool showInActive = objEmployeeSearchCriteria.ShowInActive;
                bool active;
                if (showInActive == false)
                {
                    active = true;
                }
                else
                {
                    active = false;
                }
                string query = objEmployeeSearchCriteria.SearchQuery.Trim();
                // 1 for Skills 
                if (objEmployeeSearchCriteria.EmpListFor == 1)
                {
                    EmployeeSearchList = (from p in dbContext.People
                                          join pe in dbContext.PersonEmployment on p.ID equals pe.PersonID
                                          join d in dbContext.Designations on pe.DesignationID equals d.ID
                                          join pp in dbContext.PersonPersonals on p.ID equals pp.PersonID
                                          join psm in dbContext.PersonSkillMappings on p.ID equals psm.PersonID
                                          join sm in dbContext.SkillMatrices on psm.SkillID equals sm.ID
                                          where sm.Name == query && psm.SkillRating >= objEmployeeSearchCriteria.MinRating && psm.SkillRating <= objEmployeeSearchCriteria.MaxRating && p.Active == active && sm.IsDeleted == false
                                          select new EmployeeBasicProfile
                                          {
                                              ID = p.ID,
                                              FirstName = p.FirstName,
                                              LastName = p.LastName,
                                              MiddleName = p.MiddleName,
                                              Active = p.Active,
                                              CurrentDesignation = d.Name,
                                              CurrentDesignationID = d.ID,
                                              DateOfBirth = p.DateOfBirth,
                                              Email = pe.OrganizationEmail,
                                              Extension = pe.OfficeExtension,
                                              ImagePath = p.Image,
                                              joiningDate = pe.JoiningDate ?? DateTime.MinValue,
                                              Mobile = pp.Mobile,
                                              ResidenceNumber = pp.Phone,
                                              PANNo = pp.PANNo,
                                              PFNo = pp.PFNo,
                                              SeatingLocation = pe.SeatingLocation,
                                              OfficeLocation = pe.OfficeLocation,
                                              SkillName = sm.Name,
                                              SkillRating = psm.SkillRating
                                              //Enum.GetName(typeof(EnumHelpers.Location), pe.OfficeLocation)//EnumExtensions.GetEnumDescription((EnumHelpers.Location)pe.OfficeLocation) //EnumExtensions.GetEnumDescription((EnumHelpers.Location)pe.OfficeLocation)                                             
                                          });
                }

                // 2 for Resource Pool
                if (objEmployeeSearchCriteria.EmpListFor == 2)
                {
                    EmployeeSearchList = (from p in dbContext.People
                                          join pe in dbContext.PersonEmployment on p.ID equals pe.PersonID
                                          join d in dbContext.Designations on pe.DesignationID equals d.ID
                                          join pp in dbContext.PersonPersonals on p.ID equals pp.PersonID
                                          join rp in dbContext.ResourcePool on pe.ResourcePool equals rp.ID
                                          where rp.Name == query && p.Active == active && rp.IsDeleted == false
                                          select new EmployeeBasicProfile
                                          {
                                              ID = p.ID,
                                              FirstName = p.FirstName,
                                              LastName = p.LastName,
                                              MiddleName = p.MiddleName,
                                              Active = p.Active,
                                              CurrentDesignation = d.Name,
                                              CurrentDesignationID = d.ID,
                                              DateOfBirth = p.DateOfBirth,
                                              Email = pe.OrganizationEmail,
                                              Extension = pe.OfficeExtension,
                                              ImagePath = p.Image,
                                              joiningDate = pe.JoiningDate ?? DateTime.MinValue,
                                              Mobile = pp.Mobile,
                                              ResidenceNumber = pp.Phone,
                                              PANNo = pp.PANNo,
                                              PFNo = pp.PFNo,
                                              SeatingLocation = pe.SeatingLocation,
                                              OfficeLocation = pe.OfficeLocation, //EnumExtensions.GetEnumDescription((EnumHelpers.Location)pe.OfficeLocation)    
                                              ResourcePoolName = rp.Name
                                          });
                }
                // 3 for Delivery Unit
                if (objEmployeeSearchCriteria.EmpListFor == 3)
                {
                    EmployeeSearchList = (from p in dbContext.People
                                          join pe in dbContext.PersonEmployment on p.ID equals pe.PersonID
                                          join d in dbContext.Designations on pe.DesignationID equals d.ID
                                          join pp in dbContext.PersonPersonals on p.ID equals pp.PersonID
                                          join du in dbContext.DeliveryUnit on pe.DeliveryUnit equals du.ID
                                          where du.Name == query && p.Active == active && du.IsDeleted == false
                                          select new EmployeeBasicProfile
                                          {
                                              ID = p.ID,
                                              FirstName = p.FirstName,
                                              LastName = p.LastName,
                                              MiddleName = p.MiddleName,
                                              Active = p.Active,
                                              CurrentDesignation = d.Name,
                                              CurrentDesignationID = d.ID,
                                              DateOfBirth = p.DateOfBirth,
                                              Email = pe.OrganizationEmail,
                                              Extension = pe.OfficeExtension,
                                              ImagePath = p.Image,
                                              joiningDate = pe.JoiningDate ?? DateTime.MinValue,
                                              Mobile = pp.Mobile,
                                              ResidenceNumber = pp.Phone,
                                              PANNo = pp.PANNo,
                                              PFNo = pp.PFNo,
                                              SeatingLocation = pe.SeatingLocation,
                                              OfficeLocation = pe.OfficeLocation, //EnumExtensions.GetEnumDescription((EnumHelpers.Location)pe.OfficeLocation)
                                              DeliveryUnitName = du.Name
                                          });
                }
                // 4 for Delivery Team
                if (objEmployeeSearchCriteria.EmpListFor == 4)
                {
                    EmployeeSearchList = (from p in dbContext.People
                                          join pe in dbContext.PersonEmployment on p.ID equals pe.PersonID
                                          join d in dbContext.Designations on pe.DesignationID equals d.ID
                                          join pp in dbContext.PersonPersonals on p.ID equals pp.PersonID
                                          join dt in dbContext.DeliveryTeam on pe.DeliveryTeam equals dt.ID
                                          where dt.Name == query && p.Active == active && dt.IsDeleted == false
                                          select new EmployeeBasicProfile
                                          {
                                              ID = p.ID,
                                              FirstName = p.FirstName,
                                              LastName = p.LastName,
                                              MiddleName = p.MiddleName,
                                              Active = p.Active,
                                              CurrentDesignation = d.Name,
                                              CurrentDesignationID = d.ID,
                                              DateOfBirth = p.DateOfBirth,
                                              Email = pe.OrganizationEmail,
                                              Extension = pe.OfficeExtension,
                                              ImagePath = p.Image,
                                              joiningDate = pe.JoiningDate ?? DateTime.MinValue,
                                              Mobile = pp.Mobile,
                                              ResidenceNumber = pp.Phone,
                                              PANNo = pp.PANNo,
                                              PFNo = pp.PFNo,
                                              SeatingLocation = pe.SeatingLocation,
                                              OfficeLocation = pe.OfficeLocation,//EnumExtensions.GetEnumDescription((EnumHelpers.Location)pe.OfficeLocation) 
                                              DeliveryTeamName = dt.Name
                                          });
                }

                foreach (EmployeeBasicProfile employeeSearchList in EmployeeSearchList)
                {
                    employeeSearchList.OLText = dbContext.WorkLocation.Where(x => x.ID == employeeSearchList.OfficeLocation).Select(y => y.LocationName).FirstOrDefault();
                    //employeeSearchList.OLText = EnumExtensions.GetEnumDescription((EnumHelpers.Location)employeeSearchList.OfficeLocation);
                    lstEmployeeBasicProfile.Add(employeeSearchList);
                }
                //var key = valueFieldValue.ToString();

                return lstEmployeeBasicProfile;
            });
        }

        public async Task<IEnumerable<EmployeeBasicProfile>> SearchEmployee(string query, bool showInActive)
        {
            var queryType = SearchParameterIdentifier.IdentifyEmployeeSearchOnPersonQuery(query, DesignationList().ToList());

            return await Task.Run(() =>
            {
                if (queryType.Key == "p")
                {
                    var result = ExecuteSearch<Person, Person, EmployeeBasicProfile>(query, SearchParameterIdentifier.GetPredicate<Person>, null);
                    return !showInActive ? result.Where(x => x.Active == true) : result;
                }
                else if (queryType.Key == "pe")
                    return ExecuteSearch<PersonEmployment, Person, EmployeeBasicProfile>(query, SearchParameterIdentifier.GetPredicate<PersonEmployment>, (result) =>
                    {
                        return !showInActive ? result.Select(t => t.Person).Where(x => x.Active == true).ToList() : result.Select(t => t.Person).ToList();
                    });
                else if (queryType.Key == "d")
                    return ExecuteSearch<Designation, Person, EmployeeBasicProfile>(query, SearchParameterIdentifier.GetPredicate<Designation>, (result) =>
                    {
                        return !showInActive ? result.SelectMany(t => t.PersonEmployment.Select(tt => tt.Person)).Where(x => x.Active == true) : result.SelectMany(t => t.PersonEmployment.Select(tt => tt.Person));
                    });
                else
                {
                    var result = ExecuteSearch<Person, Person, EmployeeBasicProfile>(query, SearchParameterIdentifier.GetDefaultPredicate<Person>, null);
                    return !showInActive ? result.Where(x => x.Active == true) : result;
                }
                //return null;
            });
        }

        public async Task<EmployeeBasicProfile> SearchEmployeeForAdminTasks(string query, bool showInActive)
        {
            var employee = (await SearchEmployee(query, showInActive)).FirstOrDefault();

            var year = DateTime.Now.Year;

            var empLeavesTaken = service.Top<PersonLeaveLedger>(0, a => (employee.ID == -1 || a.Person.ID == employee.ID) && a.Year == year).ToList();
            AvailableLeaves availLeaves = new AvailableLeaves();

            using (PhoenixEntities context = new PhoenixEntities())
            {
                var leaveConsumed = context.GetLeaveData(employee.ID, year).ToList();
                if (empLeavesTaken.Count() != 0)
                {
                    availLeaves.TotalLeaves = empLeavesTaken[0].OpeningBalance + (leaveConsumed?.First()?.CreditLeaves ?? 0);
                    availLeaves.CompOff = empLeavesTaken[0].CompOffs;
                }
                else
                {
                    availLeaves.TotalLeaves = (leaveConsumed?.First()?.CreditLeaves ?? 0);
                    availLeaves.CompOff = 0;
                }

                availLeaves.LeavesTaken = leaveConsumed?.First()?.LeavesConsumed ?? 0;
                availLeaves.LeavesApplied = leaveConsumed?.First()?.LeavesApplied ?? 0;
                availLeaves.CompOffAvailable = leaveConsumed?.First()?.CompOffAvailable ?? 0;
                availLeaves.LWP = leaveConsumed?.First()?.LWPApplied ?? 0;
                availLeaves.CompOffConsumed = leaveConsumed?.First()?.CompOffConsumed ?? 0;
                availLeaves.CLCredited = leaveConsumed?.First()?.CLCredited ?? 0;
                availLeaves.CLDebited = leaveConsumed?.First()?.CLDebited ?? 0;
                availLeaves.CLUtilized = leaveConsumed?.First()?.CLUtilized ?? 0;
                availLeaves.CLApplied = leaveConsumed?.First()?.CLApplied ?? 0;

                availLeaves.SLCredited = leaveConsumed?.First()?.SLCredited ?? 0;
                availLeaves.SLDebited = leaveConsumed?.First()?.SLDebited ?? 0;
                availLeaves.SLUtilized = leaveConsumed?.First()?.SLUtilized ?? 0;
                availLeaves.SLApplied = leaveConsumed?.First()?.SLApplied ?? 0;


                availLeaves.LeavesAvailable = availLeaves.TotalLeaves - availLeaves.LeavesTaken - availLeaves.LeavesApplied;
                employee.LeavesRemaining = Convert.ToInt32(availLeaves.LeavesAvailable);
                employee.CasualLeavesRemaining = (availLeaves.CLCredited - Math.Abs(availLeaves.CLDebited ?? 0) - availLeaves.CLUtilized - availLeaves.CLApplied) ?? 0;
                employee.SickLeavesRemaining = (availLeaves.SLCredited - Math.Abs(availLeaves.SLDebited ?? 0) - availLeaves.SLUtilized - availLeaves.SLApplied) ?? 0;

            }

            return employee;
        }

        public async Task<EmployeeAdminHistory> GetEmployeeAdminHistory(string query, int location, bool showInActive)
        {
            return await GetEmployeeAdminData(query, location, showInActive);
        }

        public async Task<EmployeeAdminHistory> GetEmployeeAdminData(string query, int location, bool showInActive)
        {
            EmployeeAdminHistory employeeAdminHistory = new EmployeeAdminHistory();
            int iD = Convert.ToInt32(query);
            var year = DateTime.Now.Year;

            DateTime today = DateTime.Now;
            DateTime twevleMonthsBack = today.AddMonths(-12);
            //12/21/2017
            // sixMonthsBack=12/21/2016
            try
            {
                await Task<EmployeeAdminHistory>.Run(() =>
                {
                    List<EmployeeAdminHistoryData> datas = new List<EmployeeAdminHistoryData>();
                    var filterDate = DateTime.Now.AddMonths(-3);

                    var signinSignOut = service.Top<SignInSignOut>(0, t => t.LastModified >= filterDate && t.UserID == iD && t.DayNotation == "P")
                                               .Select(t => new EmployeeAdminHistoryData()
                                               {
                                                   ID = Convert.ToInt32(t.SignInSignOutID),
                                                   FromDate = t.SignInTime,
                                                   ToDate = t.SignOutTime,
                                                   ActionType = "SISO",
                                                   Quantity = 1,
                                                   Narration = t.SignInComment,
                                                   ActionTypeID = 2,
                                                   CreatedDate = t.LastModified,
                                                   LeaveType = null,
                                                   AppliedByHR = (t.IsBulk == true)? true: false
                                               })
                                               .ToList().OrderByDescending(w => w.FromDate);
                    datas.AddRange(signinSignOut);

                    var compofftemp = service.Top<CompOff>(0, t => t.OnDate >= filterDate && t.PersonID == iD && t.ExpiresOn >= DateTime.Today && t.ByUser != null)
                        .Select(t => new { t.ByUser, t.ExpiresOn, t.ForDate, t.ID, t.IsApplied, t.LeaveRequestID, t.Narration, t.OnDate, t.Person, t.PersonID, t.Status, t.Year })
                        .ToList();

                    var personleavecredit = service.Top<PersonLeaveCredit>(1000, x => x.PersonID == iD && x.DateEffective >= filterDate).Select(t => new { t.CreditBalance, t.CreditedBy, t.DateEffective, t.ID, t.Narration, t.Year }).ToList();

                    var personleave = service.Top<PersonLeave>(1000, x => x.Person.ID == iD && x.RequestDate >= filterDate && x.IsDeleted == false /*&& (x.Narration.Contains("Admin Leave Application :") || x.Narration.Contains("Admin Casual Leave Application :") || x.Narration.Contains("Admin Sick Leave Application :")) */ ).Select(t => new { t.FromDate, t.ToDate, t.ID, t.IsDeleted, t.Leaves, t.Narration, t.LeaveType, t.RequestDate }).ToList();

                    var personCasualLeaveCredit = service.Top<PersonCLCredit>(1000, x => x.PersonID == iD && x.DateEffective >= filterDate).Select(t => new { t.CreditBalance, t.CreditedBy, t.DateEffective, t.ID, t.Narration, t.Year }).ToList();

                    var personSickLeaveCredit = service.Top<PersonSLCredit>(1000, x => x.PersonID == iD && x.DateEffective >= filterDate).Select(t => new { t.CreditBalance, t.CreditedBy, t.DateEffective, t.ID, t.Narration, t.Year }).ToList();


                    if (compofftemp != null)
                    {
                        foreach (var comp in compofftemp)
                        {
                            EmployeeAdminHistoryData data = new EmployeeAdminHistoryData();

                            data.ID = comp.ID;
                            data.FromDate = comp.ForDate;
                            data.ToDate = comp.ForDate;

                            if (comp.Narration == null || comp.Narration != "Admin Leave Deduction")
                            {
                                data.ActionType = "Credited Comp-Off";
                                data.Quantity = 1;
                            }
                            else if (comp.Narration.Contains("Admin Leave Deduction"))
                            {
                                data.ActionType = "Debited Comp-Off";
                                data.Quantity = -1;
                            }

                            data.Narration = comp.Narration;
                            data.ActionTypeID = 1;
                            data.CreatedDate = comp.OnDate;
                            data.LeaveType = 0;
                            data.AppliedByHR = true;
                            datas.Add(data);
                        }
                    }

                    if (personleavecredit != null)
                    {
                        foreach (var leavecredit in personleavecredit)
                        {
                            EmployeeAdminHistoryData data = new EmployeeAdminHistoryData();

                            data.ID = leavecredit.ID;
                            data.FromDate = leavecredit.DateEffective;
                            data.ToDate = leavecredit.DateEffective;
                            if (leavecredit.CreditBalance > 0)
                                data.ActionType = "Credited Privilege Leave";
                            else
                                data.ActionType = "Debited Privilege Leave";
                            data.Quantity = leavecredit.CreditBalance;
                            data.Narration = leavecredit.Narration;
                            data.ActionTypeID = 1;
                            data.CreatedDate = leavecredit.DateEffective;
                            data.LeaveType = 1;
                            data.AppliedByHR = true;
                            datas.Add(data);
                        }
                    }

                    if (personCasualLeaveCredit != null)
                    {
                        foreach (var leavecredit in personCasualLeaveCredit)
                        {
                            EmployeeAdminHistoryData data = new EmployeeAdminHistoryData();

                            data.ID = leavecredit.ID;
                            data.FromDate = leavecredit.DateEffective;
                            data.ToDate = leavecredit.DateEffective;
                            if (leavecredit.CreditBalance > 0)
                                data.ActionType = "Credited Casual Leave";
                            else
                                data.ActionType = "Debited Casual Leave";
                            data.Quantity = leavecredit.CreditBalance;
                            data.Narration = leavecredit.Narration;
                            data.ActionTypeID = 1;
                            data.CreatedDate = leavecredit.DateEffective;
                            data.LeaveType = 9;
                            data.AppliedByHR = true;
                            datas.Add(data);
                        }
                    }

                    if (personSickLeaveCredit != null)
                    {
                        foreach (var leavecredit in personSickLeaveCredit)
                        {
                            EmployeeAdminHistoryData data = new EmployeeAdminHistoryData();

                            data.ID = leavecredit.ID;
                            data.FromDate = leavecredit.DateEffective;
                            data.ToDate = leavecredit.DateEffective;
                            if (leavecredit.CreditBalance > 0)
                                data.ActionType = "Credited Sick Leave";
                            else
                                data.ActionType = "Debited Sick Leave";
                            data.Quantity = leavecredit.CreditBalance;
                            data.Narration = leavecredit.Narration;
                            data.ActionTypeID = 1;
                            data.CreatedDate = leavecredit.DateEffective;
                            data.LeaveType = 11;
                            data.AppliedByHR = true;
                            datas.Add(data);
                        }
                    }

                    if (personleave != null)
                    {
                        foreach (var leave in personleave)
                        {
                            EmployeeAdminHistoryData data = new EmployeeAdminHistoryData();

                            data.ID = leave.ID;
                            data.FromDate = leave.FromDate;
                            data.ToDate = leave.ToDate;
                            if (leave.LeaveType == 1)
                                data.ActionType = "Privilege Leave applied";
                            else if (leave.LeaveType == 0)
                                data.ActionType = "Compensatory Off applied";
                            else if (leave.LeaveType == 2)
                                data.ActionType = "Leave Without Pay applied";
                            else if (leave.LeaveType == 5)
                                data.ActionType = "Long leave applied";
                            else if (leave.LeaveType == 3)
                                data.ActionType = "Maternity Leave applied";
                            else if (leave.LeaveType == 4)
                                data.ActionType = "Paternity Leave applied";
                            else if (leave.LeaveType == 8)
                                data.ActionType = "Spl.Floating Holiday";
                            else if (leave.LeaveType == 9)
                                data.ActionType = "Casual Leave applied";
                            else if (leave.LeaveType == 10)
                                data.ActionType = "MTP Leave applied";
                            else if (leave.LeaveType == 11)
                                data.ActionType = "Sick Leave applied";
                            else if (leave.LeaveType == 12)
                                data.ActionType = "Election Holiday Leave applied";

                            data.Quantity = leave.Leaves;
                            data.Narration = leave.Narration;
                            data.ActionTypeID = 0;
                            data.CreatedDate = leave.RequestDate;
                            data.LeaveType = leave.LeaveType;
                            data.AppliedByHR = (data.Narration.Contains("Admin Leave Application :") || data.Narration.Contains("Admin Sick Leave Application :") || data.Narration.Contains("Admin Casual Leave Application :")) ? true : false;
                            datas.Add(data);
                        }
                    }

                    employeeAdminHistory.data = datas.OrderByDescending(x => x.CreatedDate).ToList();

                    //This gets the available compoff value to display


                    var empLeavesTaken = service.Top<PersonLeaveLedger>(0, a => (iD == -1 || a.Person.ID == iD) && a.Year == year).ToList();

                    if (empLeavesTaken.Count > 0)
                    {
                        employeeAdminHistory.CompOffs = service.All<CompOff>(x => x.IsApplied == false && x.Status == 1 && x.ExpiresOn >= DateTime.Today && x.PersonID == iD).Count(); //Convert.ToInt32(empLeavesTaken[0].CompOffs) - Convert.ToInt32(empLeavesTaken[0].CompOffUtilized); //Change done for #149073539                            
                    }
                    else
                    {
                        employeeAdminHistory.CompOffs = 0;
                    }

                    //validation for Location is remaining that should be added after system configuration module added to repo;
                    var empLocation = service.First<PersonEmployment>(x => x.PersonID == iD); // For: #149796676 Change done to fetch search employee location                       
                    var configData = service.All<PhoenixConfig>(x => x.Year == year && x.Location == (empLocation.OfficeLocation ?? 0)); // For: #149796676 Change done to fetch search employee location

                    if (configData != null)
                    {
                        foreach (var data in configData)
                        {
                            if (data.ConfigKey == "ML")
                            {
                                employeeAdminHistory.MaternityLeaveCount = Convert.ToInt32(data.ConfigValue);
                            }

                            if (data.ConfigKey == "PL")
                            {
                                employeeAdminHistory.PaternityLeaveCount = Convert.ToInt32(data.ConfigValue);
                            }

                            if (data.ConfigKey == "MTP")
                            {
                                employeeAdminHistory.MtpLeaveCount = Convert.ToInt32(data.ConfigValue);
                            }

                            if (data.ConfigKey == "EHLeave")
                            {
                                employeeAdminHistory.ElectionHolidayLeaveCount = Convert.ToInt32(data.ConfigValue);
                            }
                        }
                    }

                });
            }
            catch
            {
                throw;
            }
            return employeeAdminHistory;
        }

        public ChangeSet<EmployeePersonalDetails> UpdateEmployeePersonal(int id, ChangeSet<EmployeePersonalDetails> model, bool isMulti = false)
        {
            //Change related to #128220715
            if (model != null)
            {
                model.NewModel.PersonalEmail = string.IsNullOrEmpty(model.NewModel.PersonalEmail) ? "" : model.NewModel.PersonalEmail.ToLower();
                model.OldModel.PersonalEmail = string.IsNullOrEmpty(model.OldModel.PersonalEmail) ? "" : model.OldModel.PersonalEmail.ToLower();
            }

            var employee = new Person();
            //Commented on 19/01/2018
            //if (id == model.NewModel.SearchUserID)
            //    employee = service.First<Person>(x => x.ID == id);
            //else
            //    employee = service.First<Person>(x => x.ID == model.NewModel.SearchUserID);

            if (id != model.NewModel.SearchUserID && model.NewModel.SearchUserID != 0)
                employee = service.First<Person>(x => x.ID == model.NewModel.SearchUserID);
            else
                employee = service.First<Person>(x => x.ID == id);

            if (employee != null)
            {
                // Check for ChangeSet and send to Stage Table for Approval.
                if (model.SendForApproval)
                {
                    int stageSaveCount = _stageService.SaveModelToStage<EmployeePersonalDetails>(id, id, model);
                    if (stageSaveCount == -999)
                        return null;

                    int approverID = HookApproval(id, id, model.ModuleId);
                    int approvalID = GetApprovalID(employee.ID, model.ModuleId);
                    model.NewModel.ApprovalID = approvalID;
                    UpdateNewEntryJSONStringInDB<EmployeePersonalDetails>(model, employee.ID, model.ModuleId);
                    var approverPerson = service.First<Person>(x => x.ID == approverID);
                    string empName = employee.FirstName + " " + employee.LastName;
                    emailService.SendUserProfileApproval(id, empName, "Personal Detail", employee.PersonEmployment.First().OrganizationEmail, approverPerson.PersonEmployment.First().OrganizationEmail, employee.Image);
                }
                else if (id != model.NewModel.SearchUserID && !model.SendForApproval && model.NewModel.SearchUserID != 0) //When HR update other employee card details 
                {
                    employee = service.First<Person>(x => x.ID == model.NewModel.SearchUserID);
                    int approverID = HREditingApproval(model.NewModel.SearchUserID, model.NewModel.SearchUserID, model.ModuleId, id);

                    bool isBasicUpdated, isPersonalUpdated, isEmploymentUpdated = true, isDependedUpdated = true;

                    var viewModel = Mapper.Map<EmployeePersonalDetails, EmployeeProfileViewModel>(model.NewModel);
                    viewModel.ID = model.NewModel.SearchUserID;
                    var newPerson = Mapper.Map<EmployeeProfileViewModel, Person>(viewModel);
                    newPerson.Active = employee.Active;
                    var newPersonEmployment = Mapper.Map<EmployeeProfileViewModel, PersonEmployment>(viewModel);
                    var newPersonPersonal = Mapper.Map<EmployeeProfileViewModel, PersonPersonal>(viewModel);
                    var newPersonDependent = Mapper.Map<EmployeeProfileViewModel, PersonDependent>(viewModel);

                    // One which is having one-to-one relationship will not check for existence and will directly go for Update.
                    isBasicUpdated = service.Update<Person>(newPerson, employee);
                    isPersonalUpdated = service.Update<PersonPersonal>(newPersonPersonal, employee.PersonPersonal);
                    var checkIfEmploymentExists = employee.PersonEmployment.Where(x => x.PersonID == model.NewModel.SearchUserID).FirstOrDefault();
                    var checkIfChildExists = employee.PersonDependents.Where(x => x.Relation == 6 || x.Relation == 7)
                                                                                .OrderBy(x => x.DateOfBirth).FirstOrDefault();
                    isEmploymentUpdated = true;
                    isDependedUpdated = true;
                    // One which is having one-to-many relationship will check for existence and then only it will go for Update.
                    if (checkIfEmploymentExists != null)
                        isEmploymentUpdated = service.Update<PersonEmployment>(newPersonEmployment, employee.PersonEmployment.FirstOrDefault());
                    if (checkIfChildExists != null)
                        isDependedUpdated = service.Update<PersonDependent>(newPersonDependent, checkIfChildExists);

                    if (isBasicUpdated && isPersonalUpdated && isEmploymentUpdated && isDependedUpdated)
                        service.Finalize(true);

                    employee = service.First<Person>(x => x.ID == model.NewModel.SearchUserID);
                    viewModel = Mapper.Map<Person, EmployeeProfileViewModel>(employee);
                    model.NewModel = Mapper.Map<EmployeeProfileViewModel, EmployeePersonalDetails>(viewModel);
                }
                else
                {
                    bool isBasicUpdated, isPersonalUpdated, isEmploymentUpdated = true, isDependedUpdated = true;

                    var viewModel = Mapper.Map<EmployeePersonalDetails, EmployeeProfileViewModel>(model.NewModel);
                    viewModel.ID = id;
                    var newPerson = Mapper.Map<EmployeeProfileViewModel, Person>(viewModel);
                    newPerson.Active = employee.Active;
                    var newPersonEmployment = Mapper.Map<EmployeeProfileViewModel, PersonEmployment>(viewModel);
                    var newPersonPersonal = Mapper.Map<EmployeeProfileViewModel, PersonPersonal>(viewModel);
                    var newPersonDependent = Mapper.Map<EmployeeProfileViewModel, PersonDependent>(viewModel);

                    // One which is having one-to-one relationship will not check for existence and will directly go for Update.
                    isBasicUpdated = service.Update<Person>(newPerson, employee);
                    isPersonalUpdated = service.Update<PersonPersonal>(newPersonPersonal, employee.PersonPersonal);
                    var checkIfEmploymentExists = employee.PersonEmployment.Where(x => x.PersonID == id).FirstOrDefault();
                    var checkIfChildExists = employee.PersonDependents.Where(x => x.Relation == 6 || x.Relation == 7)
                                                                                .OrderBy(x => x.DateOfBirth).FirstOrDefault();
                    isEmploymentUpdated = true;
                    isDependedUpdated = true;
                    // One which is having one-to-many relationship will check for existence and then only it will go for Update.
                    if (checkIfEmploymentExists != null)
                        isEmploymentUpdated = service.Update<PersonEmployment>(newPersonEmployment, employee.PersonEmployment.FirstOrDefault());
                    if (checkIfChildExists != null)
                        isDependedUpdated = service.Update<PersonDependent>(newPersonDependent, checkIfChildExists);

                    if (isBasicUpdated && isPersonalUpdated && isEmploymentUpdated && isDependedUpdated)
                        service.Finalize(true);

                    employee = service.First<Person>(x => x.ID == id);
                    viewModel = Mapper.Map<Person, EmployeeProfileViewModel>(employee);
                    model.NewModel = Mapper.Map<EmployeeProfileViewModel, EmployeePersonalDetails>(viewModel);
                }
                return model;
            }

            return null;
        }

        public async Task<bool> AddEmployeeProfileAttachmentDetails(IEnumerable<EmployeeProfileAttachmentDetails> model)
        {
            using (PhoenixEntities context = new PhoenixEntities())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        bool isCreated = false;
                        foreach (var item in model)
                        {
                            var record = new EmployeeProfileAttachment()
                            {
                                ApprovalID = item.ApprovalID,
                                FieldName = item.FieldName,
                                FileName = item.FileName,
                                UniqueFileName = item.UniqueFileName,
                                IsDeleted = false
                            };

                            isCreated = service.Create<EmployeeProfileAttachment>(record, x => x.ID == 0);
                        }

                        if (isCreated)
                        {
                            service.Finalize(isCreated);
                        }

                        transaction.Commit();
                        return await Task.FromResult(true).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw ex;
                    }
                }

            }
        }

        public ChangeSet<EmployeeAddress> UpdateEmployeeAddress(int id, ChangeSet<EmployeeAddress> model, bool isMulti = false)
        {
            var employeeAddress = new PersonAddress();
            if (model.NewModel.SearchUserID == id || model.NewModel.SearchUserID == 0)
                employeeAddress = service.First<PersonAddress>(x => x.Person.ID == id && x.IsDeleted == false && x.ID == model.OldModel.ID);
            else
                employeeAddress = service.First<PersonAddress>(x => x.Person.ID == model.NewModel.SearchUserID && x.IsDeleted == false && x.ID == model.OldModel.ID);

            bool isAddressUpdated = true;

            if (employeeAddress != null)
            {
                // Check for ChangeSet and send to Stage Table for Approval.
                if (model.SendForApproval)
                {
                    int stageSaveCount = _stageService.SaveModelToMultiRecordStage<EmployeeAddress>(id, model, false);
                    if (stageSaveCount == -999)
                        return null;

                    int approverID = HookApproval(id, model.NewModel.ID, model.ModuleId);
                    int approvalID = GetApprovalID(id, model.ModuleId);
                    model.NewModel.ApprovalID = approvalID;
                    UpdateNewEntryJSONStringInDB<EmployeeAddress>(model, id, model.ModuleId);
                    var approverPerson = service.First<Person>(x => x.ID == approverID);
                    string empName = employeeAddress.Person.FirstName + " " + employeeAddress.Person.LastName;
                    emailService.SendUserProfileApproval(id, empName, "Contact Detail", employeeAddress.Person.PersonEmployment.First().OrganizationEmail, approverPerson.PersonEmployment.First().OrganizationEmail, employeeAddress.Person.Image);
                }
                else if (id != model.NewModel.SearchUserID && !model.SendForApproval && model.NewModel.SearchUserID != 0) //When HR update other employee card details 
                {
                    employeeAddress = service.First<PersonAddress>(x => x.Person.ID == model.NewModel.SearchUserID && x.IsDeleted == false && x.ID == model.OldModel.ID);
                    int approverID = HREditingApproval(model.NewModel.SearchUserID, model.NewModel.ID, model.ModuleId, id);

                    var oldPersonAddress = employeeAddress;
                    var allAddress = service.All<PersonAddress>().Where(x => x.PersonID == model.NewModel.SearchUserID).ToList();
                    //This logic is added for conditions like where current address and permanent address is same and user updates the current address.
                    if (allAddress.Count == 1)
                    {
                        oldPersonAddress.IsCurrent = false;
                        isAddressUpdated = service.Create<PersonAddress>(oldPersonAddress, x => x.IsCurrent == false && x.Person.ID == model.NewModel.SearchUserID && x.IsDeleted == false);
                        if (isAddressUpdated)
                            service.Finalize(true);
                    }

                    var oldPersonAdd = service.First<PersonAddress>(x => x.Person.ID == model.NewModel.SearchUserID && x.IsDeleted == false && x.ID == model.OldModel.ID);

                    var newPersonAddress = Mapper.Map<EmployeeAddress, PersonAddress>(model.NewModel);
                    isAddressUpdated = service.Update<PersonAddress>(newPersonAddress, oldPersonAdd);
                    if (isAddressUpdated)
                        service.Finalize(true);

                    employeeAddress = service.First<PersonAddress>(x => x.Person.ID == model.NewModel.SearchUserID && x.IsDeleted == false && x.ID == model.NewModel.ID);
                    if (employeeAddress != null)
                    {
                        model.NewModel = Mapper.Map<PersonAddress, EmployeeAddress>(employeeAddress);
                    }
                }
                else
                {
                    var oldPersonAddress = employeeAddress;
                    var allAddress = service.All<PersonAddress>(x => x.PersonID == id).ToList();
                    if (allAddress.Count == 1)
                    {
                        oldPersonAddress.IsCurrent = false;
                        isAddressUpdated = service.Create<PersonAddress>(oldPersonAddress, x => x.IsCurrent == false && x.Person.ID == id && x.IsDeleted == false);
                        if (isAddressUpdated)
                            service.Finalize(true);
                    }

                    var oldPersonAdd = service.First<PersonAddress>(x => x.Person.ID == id && x.IsDeleted == false && x.ID == model.OldModel.ID);

                    var newPersonAddress = Mapper.Map<EmployeeAddress, PersonAddress>(model.NewModel);
                    isAddressUpdated = service.Update<PersonAddress>(newPersonAddress, oldPersonAdd);
                    if (isAddressUpdated)
                        service.Finalize(true);

                    employeeAddress = service.First<PersonAddress>(x => x.Person.ID == id && x.IsDeleted == false && x.ID == model.NewModel.ID);
                    if (employeeAddress != null)
                    {
                        model.NewModel = Mapper.Map<PersonAddress, EmployeeAddress>(employeeAddress);
                    }
                }
                return model;
            }
            if (employeeAddress == null && model.NewModel.ID == 0)
            {
                if (model.SendForApproval)
                {
                    int stageSaveCount = _stageService.SaveModelToMultiRecordStage<EmployeeAddress>(id, model, false);
                    if (stageSaveCount == -999)
                        return null;

                    int approverID = HookApproval(id, model.NewModel.ID, model.ModuleId);
                    int approvalID = GetApprovalID(id, model.ModuleId);
                    model.NewModel.ApprovalID = approvalID;
                    UpdateNewEntryJSONStringInDB<EmployeeAddress>(model, id, model.ModuleId);
                    var approverPerson = service.First<Person>(x => x.ID == approverID);
                    var person = service.First<Person>(x => x.ID == id);
                    string empName = person.FirstName + " " + person.LastName;
                    emailService.SendUserProfileApproval(id, empName, "Contact Detail", person.PersonEmployment.First().OrganizationEmail, approverPerson.PersonEmployment.First().OrganizationEmail, person.Image);
                }
                else
                {
                    var newPersonAddress = Mapper.Map<EmployeeAddress, PersonAddress>(model.NewModel);
                    if (model.OldModel.ID == 0 && model.OldModel.AddressLabel == "Permanent Address")
                    {
                        newPersonAddress.Person = service.First<Person>(x => x.ID == model.NewModel.SearchUserID);
                        isAddressUpdated = service.Create<PersonAddress>(newPersonAddress, x => x.IsCurrent == model.NewModel.IsCurrent && x.Person.ID == model.NewModel.SearchUserID && x.IsDeleted == false);
                        if (isAddressUpdated)
                            service.Finalize(true);

                        employeeAddress = service.First<PersonAddress>(x => x.Person.ID == model.NewModel.SearchUserID && x.IsDeleted == false && x.ID == model.NewModel.ID);
                        if (employeeAddress != null)
                        {
                            model.NewModel = Mapper.Map<PersonAddress, EmployeeAddress>(employeeAddress);
                        }
                    }
                    else
                    {
                        newPersonAddress.Person = service.First<Person>(x => x.ID == id);
                        isAddressUpdated = service.Create<PersonAddress>(newPersonAddress, x => x.IsCurrent == model.NewModel.IsCurrent && x.Person.ID == id && x.IsDeleted == false);
                        if (isAddressUpdated)
                            service.Finalize(true);

                        employeeAddress = service.First<PersonAddress>(x => x.Person.ID == id && x.IsDeleted == false && x.ID == model.NewModel.ID);
                        if (employeeAddress != null)
                        {
                            model.NewModel = Mapper.Map<PersonAddress, EmployeeAddress>(employeeAddress);
                        }
                    }

                }
                return model;
            }
            return null;
        }

        public ChangeSet<EmployeePassportViewModel> ManageEmployeePassport(int id, ChangeSet<EmployeePassportViewModel> model, bool isDeleted)
        {
            var empPassport = new PersonPassport();
            if (model.OldModel != null && model.OldModel.ID != 0)
            {
                empPassport = service.Top<PersonPassport>(1, x => x.ID == model.OldModel.ID).FirstOrDefault();
            }

            // Check for ChangeSet and send to Stage Table for Approval.
            if (model.SendForApproval)
            {
                int stageSaveCount = _stageService.SaveModelToMultiRecordStage<EmployeePassportViewModel>(id, model, isDeleted);
                if (stageSaveCount == -999)
                    return null;

                int approverID = HookApproval(id, model.NewModel.ID, model.ModuleId);
                var approverPerson = service.First<Person>(x => x.ID == approverID);
                var employee = service.First<Person>(x => x.ID == id);
                string empName = employee.FirstName + " " + employee.LastName;
                emailService.SendUserProfileApproval(id, empName, "Passport Detail", employee.PersonEmployment.First().OrganizationEmail, approverPerson.PersonEmployment.First().OrganizationEmail, employee.Image);
            }
            else if (id != model.NewModel.SearchUserID && !model.SendForApproval && model.NewModel.SearchUserID != 0) //When HR update other employee card details 
            {
                if (isDeleted)
                {
                    if (service.SoftRemove<PersonPassport>(empPassport, x => x.Person.ID == model.NewModel.SearchUserID && x.RelationWithPPHolder == 1))
                    {
                        service.Finalize(true);
                    }
                }
                else
                {
                    int approverID = HREditingApproval(model.NewModel.SearchUserID, model.NewModel.ID, model.ModuleId, id);
                    var newPersonPassport = Mapper.Map<EmployeePassportViewModel, PersonPassport>(model.NewModel);
                    var oldPersonPassport = Mapper.Map<EmployeePassportViewModel, PersonPassport>(model.OldModel);

                    if (model.OldModel != null && !string.IsNullOrWhiteSpace(model.NewModel.PassportNumber))
                    {
                        bool isPassportUpdated = service.Update<PersonPassport>(newPersonPassport, empPassport);
                        if (isPassportUpdated)
                        {
                            service.Finalize(true);
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrWhiteSpace(model.NewModel.PassportNumber))
                        {
                            newPersonPassport.RelationWithPPHolder = 1;
                            newPersonPassport.Person = service.First<Person>(x => x.ID == model.NewModel.SearchUserID);
                            newPersonPassport.PersonDependent = service.First<PersonDependent>(x => x.PersonID == model.NewModel.SearchUserID);
                            bool isPassportAdded = service.Create<PersonPassport>(newPersonPassport, null);
                            if (isPassportAdded)
                            {
                                service.Finalize(true);
                                model.NewModel = Mapper.Map<PersonPassport, EmployeePassportViewModel>(newPersonPassport);
                                return model;
                            }
                        }
                    }
                }
            }
            else
            {
                if (isDeleted)
                {


                    if (service.SoftRemove<PersonPassport>(empPassport, x => x.Person.ID == id && x.RelationWithPPHolder == 1))
                    {
                        service.Finalize(true);
                    }
                }
                else
                {
                    var newPersonPassport = Mapper.Map<EmployeePassportViewModel, PersonPassport>(model.NewModel);
                    var oldPersonPassport = Mapper.Map<EmployeePassportViewModel, PersonPassport>(model.OldModel);

                    if (model.OldModel != null && !string.IsNullOrWhiteSpace(model.NewModel.PassportNumber))
                    {
                        bool isPassportUpdated = service.Update<PersonPassport>(newPersonPassport, empPassport);
                        if (isPassportUpdated)
                        {
                            service.Finalize(true);
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrWhiteSpace(model.NewModel.PassportNumber))
                        {
                            newPersonPassport.RelationWithPPHolder = 1;
                            newPersonPassport.Person = service.First<Person>(x => x.ID == id);
                            newPersonPassport.PersonDependent = service.First<PersonDependent>(x => x.PersonID == id);
                            bool isPassportAdded = service.Create<PersonPassport>(newPersonPassport, null);
                            if (isPassportAdded)
                            {
                                service.Finalize(true);
                                model.NewModel = Mapper.Map<PersonPassport, EmployeePassportViewModel>(newPersonPassport);
                                return model;
                            }
                        }
                    }
                }
            }
            return model;
        }

        public ChangeSet<EmployeeCertification> ManageEmployeeCertifications(int id, ChangeSet<EmployeeCertification> model, bool isDeleted)
        {
            var empCertificationList = new PersonCertification();
            if (model.OldModel != null && model.OldModel.ID != 0)
            {
                empCertificationList = service.First<PersonCertification>(x => x.Person.ID == id && x.IsDeleted == false && x.ID == model.OldModel.ID);
            }
            bool isCertificationUpdated = true, isDataDeleted = true;

            // Check for ChangeSet and send to Stage Table for Approval.
            if (model.SendForApproval)
            {
                int stageSaveCount = _stageService.SaveModelToMultiRecordStage<EmployeeCertification>(id, model, isDeleted);
                if (stageSaveCount == -999)
                    return null;

                int approverID = HookApproval(id, model.NewModel.ID, model.ModuleId);
                int approvalID = GetApprovalID(id, model.ModuleId);
                model.NewModel.ApprovalID = approvalID;
                UpdateNewEntryJSONStringInDB<EmployeeCertification>(model, id, model.ModuleId);
                var approverPerson = service.First<Person>(x => x.ID == approverID);
                var employee = service.First<Person>(x => x.ID == id);
                string empName = employee.FirstName + " " + employee.LastName;
                emailService.SendUserProfileApproval(id, empName, "Certification Detail", employee.PersonEmployment.First().OrganizationEmail, approverPerson.PersonEmployment.First().OrganizationEmail, employee.Image);
            }
            else if (id != model.NewModel.SearchUserID && !model.SendForApproval && model.NewModel.SearchUserID != 0) //When HR update other employee card details 
            {
                if (model.OldModel != null && model.OldModel.ID != 0)
                {
                    empCertificationList = service.First<PersonCertification>(x => x.Person.ID == model.NewModel.SearchUserID && x.IsDeleted == false && x.ID == model.OldModel.ID);
                }
                if (isDeleted)
                {
                    var deletedCertification = empCertificationList;
                    isDataDeleted = service.SoftRemove<PersonCertification>(deletedCertification, x => x.Person.ID == model.NewModel.SearchUserID);
                }
                else
                {
                    int approverID = HREditingApproval(model.NewModel.SearchUserID, model.NewModel.ID, model.ModuleId, id);

                    if (model.NewModel.ID == 0)
                    {
                        var newPersonCertification = Mapper.Map<EmployeeCertification, PersonCertification>(model.NewModel);
                        newPersonCertification.Person = service.First<Person>(x => x.ID == model.NewModel.SearchUserID);
                        newPersonCertification.Certifications = service.First<Certifications>(x => x.ID == model.NewModel.CertificationID);
                        isCertificationUpdated = service.Create<PersonCertification>(newPersonCertification, x => x.Certifications.ID == model.NewModel.CertificationID && x.Person.ID == model.NewModel.SearchUserID && x.ID == model.NewModel.ID);
                    }
                    else
                    {
                        var oldEmpCertification = empCertificationList;
                        var newPersonCertification = Mapper.Map<EmployeeCertification, PersonCertification>(model.NewModel);
                        isCertificationUpdated = service.Update<PersonCertification>(newPersonCertification, oldEmpCertification);
                        empCertificationList = service.First<PersonCertification>(x => x.Person.ID == model.NewModel.SearchUserID && x.IsDeleted == false && x.ID == oldEmpCertification.ID);
                    }
                }
                if (isCertificationUpdated || isDataDeleted)
                    service.Finalize(true);

                if (model.NewModel.ID == 0)
                    empCertificationList = service.Top<PersonCertification>(10, x => x.Person.ID == model.NewModel.SearchUserID && x.IsDeleted == false).OrderByDescending(x => x.ID).First();
                if (empCertificationList != null)
                {
                    model.NewModel = Mapper.Map<PersonCertification, EmployeeCertification>(empCertificationList);
                }
            }
            else
            {
                if (isDeleted)
                {
                    var deletedCertification = empCertificationList;
                    isDataDeleted = service.SoftRemove<PersonCertification>(deletedCertification, x => x.Person.ID == id);
                }
                else
                {
                    if (model.NewModel.ID == 0)
                    {
                        var newPersonCertification = Mapper.Map<EmployeeCertification, PersonCertification>(model.NewModel);
                        newPersonCertification.Person = service.First<Person>(x => x.ID == id);
                        newPersonCertification.Certifications = service.First<Certifications>(x => x.ID == model.NewModel.CertificationID);
                        isCertificationUpdated = service.Create<PersonCertification>(newPersonCertification, x => x.Certifications.ID == model.NewModel.CertificationID && x.Person.ID == id && x.ID == model.NewModel.ID);
                    }
                    else
                    {
                        var oldEmpCertification = empCertificationList;
                        var newPersonCertification = Mapper.Map<EmployeeCertification, PersonCertification>(model.NewModel);
                        isCertificationUpdated = service.Update<PersonCertification>(newPersonCertification, oldEmpCertification);
                        empCertificationList = service.First<PersonCertification>(x => x.Person.ID == id && x.IsDeleted == false && x.ID == oldEmpCertification.ID);
                    }
                }
                if (isCertificationUpdated || isDataDeleted)
                    service.Finalize(true);

                if (model.NewModel.ID == 0)
                    empCertificationList = service.Top<PersonCertification>(10, x => x.Person.ID == id && x.IsDeleted == false).OrderByDescending(x => x.ID).First();
                if (empCertificationList != null)
                {
                    model.NewModel = Mapper.Map<PersonCertification, EmployeeCertification>(empCertificationList);
                }
            }
            return model;
        }

        public ChangeSet<EmployeeEmploymentHistory> ManageEmployeeEmploymentHistory(int id, ChangeSet<EmployeeEmploymentHistory> model, bool isDeleted)
        {
            var empEmploymentHistoryList = new PersonEmploymentHistory();
            if (model.OldModel != null && model.OldModel.ID != 0)
            {
                empEmploymentHistoryList = service.First<PersonEmploymentHistory>(x => x.Person.ID == id && x.IsDeleted == false && x.ID == model.OldModel.ID);
            }
            bool isEmpHistoryUpdate = true, isDataDeleted = true;

            // Check for ChangeSet and send to Stage Table for Approval.
            if (model.SendForApproval)
            {
                int stageSaveCount = _stageService.SaveModelToMultiRecordStage<EmployeeEmploymentHistory>(id, model, isDeleted);
                if (stageSaveCount == -999)
                    return null;

                int approverID = HookApproval(id, model.NewModel.ID, model.ModuleId);
                var approverPerson = service.First<Person>(x => x.ID == approverID);
                var employee = service.First<Person>(x => x.ID == id);
                string empName = employee.FirstName + " " + employee.LastName;
                emailService.SendUserProfileApproval(id, empName, "Experience Detail", employee.PersonEmployment.First().OrganizationEmail, approverPerson.PersonEmployment.First().OrganizationEmail, employee.Image);
            }
            else if (id != model.NewModel.SearchUserID && !model.SendForApproval && model.NewModel.SearchUserID != 0) //When HR update other employee card details 
            {
                if (model.OldModel != null && model.OldModel.ID != 0)
                {
                    empEmploymentHistoryList = service.First<PersonEmploymentHistory>(x => x.Person.ID == model.NewModel.SearchUserID && x.IsDeleted == false && x.ID == model.OldModel.ID);
                }

                if (isDeleted)
                {
                    var deletedEmploymentHistory = empEmploymentHistoryList;
                    isDataDeleted = service.SoftRemove<PersonEmploymentHistory>(deletedEmploymentHistory, x => x.Person.ID == model.NewModel.SearchUserID);
                }
                else
                {
                    int approverID = HREditingApproval(model.NewModel.SearchUserID, model.NewModel.ID, model.ModuleId, id);

                    if (model.NewModel.ID == 0)
                    {
                        var newEmploymentHistory = Mapper.Map<EmployeeEmploymentHistory, PersonEmploymentHistory>(model.NewModel);
                        newEmploymentHistory.Person = service.First<Person>(x => x.ID == model.NewModel.SearchUserID);
                        isEmpHistoryUpdate = service.Create<PersonEmploymentHistory>(newEmploymentHistory, x => x.OrganisationName.Equals(model.NewModel.OrganisationName, StringComparison.InvariantCultureIgnoreCase) && x.JoiningDate == model.NewModel.JoiningDate && x.Person.ID == model.NewModel.SearchUserID && x.ID == model.NewModel.ID);
                    }
                    else
                    {
                        var oldEmploymentHistory = empEmploymentHistoryList;
                        var newEmploymentHistory = Mapper.Map<EmployeeEmploymentHistory, PersonEmploymentHistory>(model.NewModel);
                        isEmpHistoryUpdate = service.Update<PersonEmploymentHistory>(newEmploymentHistory, oldEmploymentHistory);
                        empEmploymentHistoryList = service.First<PersonEmploymentHistory>(x => x.Person.ID == model.NewModel.SearchUserID && x.IsDeleted == false && x.ID == oldEmploymentHistory.ID);
                    }
                }
                if (isEmpHistoryUpdate || isDataDeleted)
                    service.Finalize(true);

                if (model.NewModel.ID == 0)
                    empEmploymentHistoryList = service.Top<PersonEmploymentHistory>(0, x => x.Person.ID == model.NewModel.SearchUserID && x.IsDeleted == false).OrderByDescending(x => x.ID).First();
                if (empEmploymentHistoryList != null)
                {
                    model.NewModel = Mapper.Map<PersonEmploymentHistory, EmployeeEmploymentHistory>(empEmploymentHistoryList);
                }
            }
            else
            {
                if (isDeleted)
                {
                    var deletedEmploymentHistory = empEmploymentHistoryList;
                    isDataDeleted = service.SoftRemove<PersonEmploymentHistory>(deletedEmploymentHistory, x => x.Person.ID == id);
                }
                else
                {
                    if (model.NewModel.ID == 0)
                    {
                        var newEmploymentHistory = Mapper.Map<EmployeeEmploymentHistory, PersonEmploymentHistory>(model.NewModel);
                        newEmploymentHistory.Person = service.First<Person>(x => x.ID == id);
                        isEmpHistoryUpdate = service.Create<PersonEmploymentHistory>(newEmploymentHistory, x => x.OrganisationName.Equals(model.NewModel.OrganisationName, StringComparison.InvariantCultureIgnoreCase) && x.JoiningDate == model.NewModel.JoiningDate && x.Person.ID == id && x.ID == model.NewModel.ID);
                    }
                    else
                    {
                        var oldEmploymentHistory = empEmploymentHistoryList;
                        var newEmploymentHistory = Mapper.Map<EmployeeEmploymentHistory, PersonEmploymentHistory>(model.NewModel);
                        isEmpHistoryUpdate = service.Update<PersonEmploymentHistory>(newEmploymentHistory, oldEmploymentHistory);
                        empEmploymentHistoryList = service.First<PersonEmploymentHistory>(x => x.Person.ID == id && x.IsDeleted == false && x.ID == oldEmploymentHistory.ID);
                    }
                }
                if (isEmpHistoryUpdate || isDataDeleted)
                    service.Finalize(true);

                if (model.NewModel.ID == 0)
                    empEmploymentHistoryList = service.Top<PersonEmploymentHistory>(0, x => x.Person.ID == id && x.IsDeleted == false).OrderByDescending(x => x.ID).First();
                if (empEmploymentHistoryList != null)
                {
                    model.NewModel = Mapper.Map<PersonEmploymentHistory, EmployeeEmploymentHistory>(empEmploymentHistoryList);
                }
            }
            return model;
        }

        public ChangeSet<EmployeeDependent> ManageEmployeeDependent(int id, ChangeSet<EmployeeDependent> model, bool isDeleted)
        {
            var empDependentList = new PersonDependent();
            if (model.OldModel != null && model.OldModel.ID != 0)
            {
                empDependentList = service.First<PersonDependent>(x => x.Person.ID == id && x.IsDeleted == false && x.ID == model.OldModel.ID);
            }
            bool isDependentUpdated = true, isDataDeleted = true;

            // Check for ChangeSet and send to Stage Table for Approval.
            if (model.SendForApproval)
            {
                int stageSaveCount = _stageService.SaveModelToMultiRecordStage<EmployeeDependent>(id, model, isDeleted);
                if (stageSaveCount == -999)
                    return null;

                int approverID = HookApproval(id, model.NewModel.ID, model.ModuleId);
                var approverPerson = service.First<Person>(x => x.ID == approverID);
                var employee = service.First<Person>(x => x.ID == id);
                string empName = employee.FirstName + " " + employee.LastName;
                emailService.SendUserProfileApproval(id, empName, "Dependent", employee.PersonEmployment.First().OrganizationEmail, approverPerson.PersonEmployment.First().OrganizationEmail, employee.Image);
            }
            else if (id != model.NewModel.SearchUserID && !model.SendForApproval && model.NewModel.SearchUserID != 0) //When HR update other employee card details 
            {

                if (isDeleted)
                {
                    if (model.OldModel != null && model.OldModel.ID != 0)
                    {
                        empDependentList = service.First<PersonDependent>(x => x.Person.ID == model.NewModel.SearchUserID && x.IsDeleted == false && x.ID == model.OldModel.ID);
                    }
                    var deletedPersonDependent = empDependentList;
                    isDataDeleted = service.SoftRemove<PersonDependent>(deletedPersonDependent, x => x.Person.ID == model.NewModel.SearchUserID);
                }
                else
                {
                    if (model.NewModel.ID == 0)
                    {
                        var newEmployeeDependent = Mapper.Map<EmployeeDependent, PersonDependent>(model.NewModel);
                        newEmployeeDependent.Person = service.First<Person>(x => x.ID == model.NewModel.SearchUserID);
                        isDependentUpdated = service.Create<PersonDependent>(newEmployeeDependent, x => x.Name.Equals(model.NewModel.DependentName, StringComparison.InvariantCultureIgnoreCase) && x.Person.ID == model.NewModel.SearchUserID && x.ID == model.NewModel.ID);
                    }
                    else
                    {
                        var oldEmploymentDependent = service.First<PersonDependent>(x => x.Person.ID == model.NewModel.SearchUserID && x.IsDeleted == false && x.ID == model.OldModel.ID);
                        var newEmploymentDependent = Mapper.Map<EmployeeDependent, PersonDependent>(model.NewModel);
                        isDependentUpdated = service.Update<PersonDependent>(newEmploymentDependent, oldEmploymentDependent);
                        empDependentList = service.First<PersonDependent>(x => x.Person.ID == model.NewModel.SearchUserID && x.IsDeleted == false && x.ID == oldEmploymentDependent.ID);
                    }
                }
                if (isDependentUpdated || isDataDeleted)
                    service.Finalize(true);

                if (model.NewModel.ID == 0)
                    empDependentList = service.Top<PersonDependent>(0, x => x.Person.ID == model.NewModel.SearchUserID && x.IsDeleted == false).OrderByDescending(x => x.ID).First();
                if (empDependentList != null)
                {
                    model.NewModel = Mapper.Map<PersonDependent, EmployeeDependent>(empDependentList);
                }
            }
            else
            {
                if (isDeleted)
                {
                    var deletedPersonDependent = empDependentList;
                    isDataDeleted = service.SoftRemove<PersonDependent>(deletedPersonDependent, x => x.Person.ID == id);
                }
                else
                {
                    if (model.NewModel.ID == 0)
                    {
                        var newEmployeeDependent = Mapper.Map<EmployeeDependent, PersonDependent>(model.NewModel);
                        newEmployeeDependent.Person = service.First<Person>(x => x.ID == id);
                        isDependentUpdated = service.Create<PersonDependent>(newEmployeeDependent, x => x.Name.Equals(model.NewModel.DependentName, StringComparison.InvariantCultureIgnoreCase) && x.Person.ID == id && x.ID == model.NewModel.ID);
                    }
                    else
                    {
                        var oldEmploymentDependent = empDependentList;
                        var newEmploymentDependent = Mapper.Map<EmployeeDependent, PersonDependent>(model.NewModel);
                        isDependentUpdated = service.Update<PersonDependent>(newEmploymentDependent, oldEmploymentDependent);
                        empDependentList = service.First<PersonDependent>(x => x.Person.ID == id && x.IsDeleted == false && x.ID == oldEmploymentDependent.ID);
                    }
                }
                if (isDependentUpdated || isDataDeleted)
                    service.Finalize(true);

                if (model.NewModel.ID == 0)
                    empDependentList = service.Top<PersonDependent>(0, x => x.Person.ID == id && x.IsDeleted == false).OrderByDescending(x => x.ID).First();
                if (empDependentList != null)
                {
                    model.NewModel = Mapper.Map<PersonDependent, EmployeeDependent>(empDependentList);
                }
            }
            return model;
        }

        //public ChangeSet<PersonBGMapping> ManageEmployeeBGC(int id, ChangeSet<PersonBGMapping> model)
        //{
        //    bool isempBGMappingUpdated = true;
        //    var oldEmploymentBGMapping = service.First<PersonBGMapping>(x => x.PersonID == model.NewModel.PersonID && x.ID == model.OldModel.ID);
        //    var newEmploymentBGMapping = model.NewModel;
        //    isempBGMappingUpdated = service.Update<PersonBGMapping>(newEmploymentBGMapping, oldEmploymentBGMapping);
        //    if (isempBGMappingUpdated)
        //        service.Finalize(true);
        //    // update BG status in current resource allocation
        //    List<int> projectID = service.All<PMSResourceAllocation>(x => x.PersonID == model.NewModel.PersonID && x.BGStatus == 1).Select(x => x.ProjectID).ToList();
        //    foreach (var item in projectID)
        //    {
        //        List<int> requiredBGCList = GetRequiredBGCList(item, model.NewModel.PersonID);
        //        if (requiredBGCList.Count() == 0)
        //        {
        //            using (PhoenixEntities dbContext = new PhoenixEntities())
        //            {
        //                PMSResourceAllocation ca = (from x in dbContext.PMSResourceAllocation
        //                                            where x.ProjectID == item && x.PersonID == model.NewModel.PersonID && x.IsDeleted == false
        //                                            select x).FirstOrDefault();
        //                ca.BGStatus = 2; // 2 is completed
        //                ca.ModifyDate = DateTime.Now;
        //                dbContext.Entry(ca).State = EntityState.Modified;
        //                dbContext.SaveChanges();
        //                emailService.SendBGVerificationToRMG(id, item, model.NewModel.PersonID);
        //            }
        //        }
        //    }
        //    return model;
        //}

        //private List<int> GetRequiredBGCList(int projectID, int personID)
        //{
        //    using (PhoenixEntities dbContext = new PhoenixEntities())
        //    {

        //        List<int> missMatchedBGParameters = new List<int>();

        //        List<int> projectBGParameters = (from m in dbContext.CustomerBGMapping
        //                                         where m.CustomerID == (from c in dbContext.ProjectList where c.ID == projectID select c.CustomerID).FirstOrDefault()
        //                                         select m.BGParameterID).ToList();

        //        List<int> resourceBGParameters = (from m in dbContext.PersonBGMapping
        //                                          where m.PersonID == personID && m.BGStatus == 2
        //                                          select m.BGParameterID).ToList();

        //        return missMatchedBGParameters = projectBGParameters.Where(p => !resourceBGParameters.Any(p2 => p2 == p)).ToList();
        //    }
        //}

        public ChangeSet<EmployeeDeclaration> ManageEmployeeDeclaration(int id, ChangeSet<EmployeeDeclaration> model, bool isDeleted)
        {
            var empDeclarationList = new PersonDeclaration();
            if (model.OldModel != null && model.OldModel.ID != 0)
            {
                empDeclarationList = service.First<PersonDeclaration>(x => x.Person.ID == id && x.IsDeleted == false && x.ID == model.OldModel.ID);
            }
            bool isDeclarationUpdated = true, isDataDeleted = true;

            // Check for ChangeSet and send to Stage Table for Approval.
            if (model.SendForApproval)
            {
                int stageSaveCount = _stageService.SaveModelToMultiRecordStage<EmployeeDeclaration>(id, model, isDeleted);
                if (stageSaveCount == -999)
                    return null;

                int approverID = HookApproval(id, model.NewModel.ID, model.ModuleId);
                var approverPerson = service.First<Person>(x => x.ID == approverID);
                var employee = service.First<Person>(x => x.ID == id);
                string empName = employee.FirstName + " " + employee.LastName;
                emailService.SendUserProfileApproval(id, empName, "Declaration", employee.PersonEmployment.First().OrganizationEmail, approverPerson.PersonEmployment.First().OrganizationEmail, employee.Image);
            }
            else if (id != model.NewModel.SearchUserID && !model.SendForApproval && model.NewModel.SearchUserID != 0) //When HR update other employee card details 
            {
                if (model.OldModel != null && model.OldModel.ID != 0)
                {
                    empDeclarationList = service.First<PersonDeclaration>(x => x.Person.ID == model.NewModel.SearchUserID && x.IsDeleted == false && x.ID == model.OldModel.ID);
                }

                if (isDeleted)
                {
                    var deletedPersonDeclaration = empDeclarationList;
                    isDataDeleted = service.SoftRemove<PersonDeclaration>(deletedPersonDeclaration, x => x.Person.ID == model.NewModel.SearchUserID);
                }
                else
                {
                    if (model.NewModel.ID == 0)
                    {
                        var newEmployeeDeclaration = Mapper.Map<EmployeeDeclaration, PersonDeclaration>(model.NewModel);
                        newEmployeeDeclaration.Person = service.First<Person>(x => x.ID == model.NewModel.SearchUserID);
                        isDeclarationUpdated = service.Create<PersonDeclaration>(newEmployeeDeclaration, x => x.Name.Equals(model.NewModel.DeclaredPerson, StringComparison.InvariantCultureIgnoreCase) && x.Person.ID == model.NewModel.SearchUserID && x.ID == model.NewModel.ID);
                    }
                    else
                    {
                        var oldEmployeeDeclaration = empDeclarationList;
                        var newEmployeeDeclaration = Mapper.Map<EmployeeDeclaration, PersonDeclaration>(model.NewModel);
                        isDeclarationUpdated = service.Update<PersonDeclaration>(newEmployeeDeclaration, oldEmployeeDeclaration);
                        empDeclarationList = service.First<PersonDeclaration>(x => x.Person.ID == model.NewModel.SearchUserID && x.IsDeleted == false && x.ID == oldEmployeeDeclaration.ID);
                    }
                }
                if (isDeclarationUpdated)
                    service.Finalize(true);

                if (model.NewModel.ID == 0)
                    empDeclarationList = service.Top<PersonDeclaration>(0, x => x.Person.ID == model.NewModel.SearchUserID && x.IsDeleted == false).OrderByDescending(x => x.ID).First();
                if (empDeclarationList != null)
                {
                    model.NewModel = Mapper.Map<PersonDeclaration, EmployeeDeclaration>(empDeclarationList);
                }
            }
            else
            {
                if (isDeleted)
                {
                    var deletedPersonDeclaration = empDeclarationList;
                    isDataDeleted = service.SoftRemove<PersonDeclaration>(deletedPersonDeclaration, x => x.Person.ID == id);
                }
                else
                {
                    if (model.NewModel.ID == 0)
                    {
                        var newEmployeeDeclaration = Mapper.Map<EmployeeDeclaration, PersonDeclaration>(model.NewModel);
                        newEmployeeDeclaration.Person = service.First<Person>(x => x.ID == id);
                        isDeclarationUpdated = service.Create<PersonDeclaration>(newEmployeeDeclaration, x => x.Name.Equals(model.NewModel.DeclaredPerson, StringComparison.InvariantCultureIgnoreCase) && x.Person.ID == id && x.ID == model.NewModel.ID);
                    }
                    else
                    {
                        var oldEmployeeDeclaration = empDeclarationList;
                        var newEmployeeDeclaration = Mapper.Map<EmployeeDeclaration, PersonDeclaration>(model.NewModel);
                        isDeclarationUpdated = service.Update<PersonDeclaration>(newEmployeeDeclaration, oldEmployeeDeclaration);
                        empDeclarationList = service.First<PersonDeclaration>(x => x.Person.ID == id && x.IsDeleted == false && x.ID == oldEmployeeDeclaration.ID);
                    }
                }
                if (isDeclarationUpdated)
                    service.Finalize(true);

                if (model.NewModel.ID == 0)
                    empDeclarationList = service.Top<PersonDeclaration>(0, x => x.Person.ID == id && x.IsDeleted == false).OrderByDescending(x => x.ID).First();
                if (empDeclarationList != null)
                {
                    model.NewModel = Mapper.Map<PersonDeclaration, EmployeeDeclaration>(empDeclarationList);
                }
            }
            return model;
        }

        public ChangeSet<EmployeeSkill> ManageEmployeeSkills(int id, ChangeSet<EmployeeSkill> model, bool isDeleted)
        {
            var empSkillsList = new PersonSkillMapping();
            if (model.OldModel != null && model.OldModel.ID != 0)
            {
                empSkillsList = service.First<PersonSkillMapping>(x => x.Person.ID == id && x.IsDeleted == false && x.ID == model.OldModel.ID);
            }
            bool isSkillsUpdated = true, isDataDeleted = true;

            // Check for ChangeSet and send to Stage Table for Approval.
            if (model.SendForApproval)
            {
                //bool isSentForApproval =
                int stageSaveCount = _stageService.SaveModelToMultiRecordStage<EmployeeSkill>(id, model, isDeleted);
                if (stageSaveCount == -999)
                    return null;

                int approverID = HookApproval(id, model.NewModel.ID, model.ModuleId);
                var approverPerson = service.First<Person>(x => x.ID == approverID);
                var employee = service.First<Person>(x => x.ID == id);
                string empName = employee.FirstName + " " + employee.LastName;
                emailService.SendUserProfileApproval(id, empName, "Skills", employee.PersonEmployment.First().OrganizationEmail, approverPerson.PersonEmployment.First().OrganizationEmail, employee.Image);
            }
            else if (id != model.NewModel.SearchUserID && !model.SendForApproval && model.NewModel.SearchUserID != 0)
            {
                if (model.OldModel != null && model.OldModel.ID != 0)
                {
                    empSkillsList = service.First<PersonSkillMapping>(x => x.Person.ID == model.NewModel.SearchUserID && x.IsDeleted == false && x.ID == model.OldModel.ID);
                }

                if (isDeleted)
                {
                    var deletedPersonSkill = empSkillsList;
                    isDataDeleted = service.SoftRemove<PersonSkillMapping>(deletedPersonSkill, x => x.Person.ID == model.NewModel.SearchUserID);
                }
                else
                {
                    if (model.NewModel.ID == 0)
                    {
                        var newEmployeeSkills = Mapper.Map<EmployeeSkill, PersonSkillMapping>(model.NewModel);
                        newEmployeeSkills.Person = service.First<Person>(x => x.ID == model.NewModel.SearchUserID);
                        newEmployeeSkills.IsPrimary = model.NewModel.IsPrimary; // Set IsPrimary flag
                        isSkillsUpdated = service.Create<PersonSkillMapping>(newEmployeeSkills, x => x.SkillID == model.NewModel.SkillID && x.Person.ID == model.NewModel.SearchUserID && x.ID == model.NewModel.ID);
                    }
                    else
                    {
                        var oldEmployeeSkills = empSkillsList;
                        var newEmployeeSkills = Mapper.Map<EmployeeSkill, PersonSkillMapping>(model.NewModel);
                        newEmployeeSkills.IsPrimary = model.NewModel.IsPrimary; // Set IsPrimary flag
                        isSkillsUpdated = service.Update<PersonSkillMapping>(newEmployeeSkills, oldEmployeeSkills);
                        empSkillsList = service.First<PersonSkillMapping>(x => x.Person.ID == model.NewModel.SearchUserID && x.IsDeleted == false && x.ID == oldEmployeeSkills.ID);
                    }
                }
                if (isSkillsUpdated || isDataDeleted)
                    service.Finalize(true);

                if (model.NewModel.ID == 0)
                    empSkillsList = service.Top<PersonSkillMapping>(0, x => x.Person.ID == model.NewModel.SearchUserID && x.IsDeleted == false).OrderByDescending(x => x.ID).First();
                if (empSkillsList != null)
                {
                    model.NewModel = Mapper.Map<PersonSkillMapping, EmployeeSkill>(empSkillsList);
                }
            }
            else
            {
                if (isDeleted)
                {
                    var deletedPersonSkill = empSkillsList;
                    isDataDeleted = service.SoftRemove<PersonSkillMapping>(deletedPersonSkill, x => x.Person.ID == id);
                }
                else
                {
                    if (model.NewModel.ID == 0)
                    {
                        empSkillsList = service.First<PersonSkillMapping>(x => x.Person.ID == model.NewModel.SearchUserID && x.IsDeleted == false && x.SkillID == model.NewModel.ID);

                        if (empSkillsList != null)
                        {
                            // update exiting to Secondary skill to Primary Skill
                            var oldEmployeeSkills = empSkillsList;
                            var newEmployeeSkills = Mapper.Map<EmployeeSkill, PersonSkillMapping>(model.NewModel);
                            newEmployeeSkills.IsPrimary = model.NewModel.IsPrimary; // Set IsPrimary flag
                            isSkillsUpdated = service.Update<PersonSkillMapping>(newEmployeeSkills, oldEmployeeSkills);
                            empSkillsList = service.First<PersonSkillMapping>(x => x.Person.ID == id && x.IsDeleted == false && x.ID == oldEmployeeSkills.ID);
                        }
                        else
                        {
                            var newEmployeeSkills = Mapper.Map<EmployeeSkill, PersonSkillMapping>(model.NewModel);
                            newEmployeeSkills.Person = service.First<Person>(x => x.ID == id);
                            newEmployeeSkills.IsPrimary = model.NewModel.IsPrimary; // Set IsPrimary flag
                            isSkillsUpdated = service.Create<PersonSkillMapping>(newEmployeeSkills, x => x.SkillID == model.NewModel.SkillID && x.Person.ID == id && x.ID == model.NewModel.ID);
                        }
                    }
                    else
                    {
                        var oldEmployeeSkills = empSkillsList;
                        var newEmployeeSkills = Mapper.Map<EmployeeSkill, PersonSkillMapping>(model.NewModel);
                        newEmployeeSkills.IsPrimary = model.NewModel.IsPrimary; // Set IsPrimary flag
                        isSkillsUpdated = service.Update<PersonSkillMapping>(newEmployeeSkills, oldEmployeeSkills);
                        empSkillsList = service.First<PersonSkillMapping>(x => x.Person.ID == id && x.IsDeleted == false && x.ID == oldEmployeeSkills.ID);
                    }
                }
                if (isSkillsUpdated || isDataDeleted)
                    service.Finalize(true);

                if (model.NewModel.ID == 0)
                    empSkillsList = service.Top<PersonSkillMapping>(0, x => x.Person.ID == id && x.IsDeleted == false).OrderByDescending(x => x.ID).First();
                if (empSkillsList != null)
                {
                    model.NewModel = Mapper.Map<PersonSkillMapping, EmployeeSkill>(empSkillsList);
                }
            }
            return model;
        }

        public EmployeeCompetency ManageEmployeeSkillsDescription(int id, EmployeeCompetency model)
        {
            string skillDescription = model.SkillDescription;  // Extract SkillDescription from the model

            // Update SkillDescription in PersonEmployment table
            var personEmployment = service.First<PersonEmployment>(x => x.Person.ID == id);
            if (personEmployment != null)
            {
                personEmployment.SkillDescription = skillDescription;
                service.Update<PersonEmployment>(personEmployment);
                service.Finalize(true);
            }
            else
            {
                // If PersonEmployment doesn't exist, create a new record
                var newPersonEmployment = new PersonEmployment
                {
                    Person = service.First<Person>(x => x.ID == id),
                    SkillDescription = skillDescription
                };
                service.Update<PersonEmployment>(newPersonEmployment);
                service.Finalize(true);
            }
            return model;
        }

        public EmployeeCompetency ManageEmployeeCompetency(int id, EmployeeCompetency model)
        {
            // Retrieve the employee's PersonEmployment record
            var employment = service.First<PersonEmployment>(x => x.Person.ID == id);

            if (employment != null)
            {
                // Update the CompetencyID in the PersonEmployment table
                employment.CompetencyID = model.CompetencyID;
                service.Update<PersonEmployment>(employment);
                service.Finalize(true);
            }

            return model;
        }

        public ChangeSet<EmployeeVisa> ManageEmployeeVisa(int id, ChangeSet<EmployeeVisa> model, bool isDeleted)
        {
            var empVisaList = new PersonVisa();
            if (model.OldModel != null && model.OldModel.ID != 0)
            {
                empVisaList = service.First<PersonVisa>(x => x.Person.ID == id && x.IsDeleted == false && x.ID == model.OldModel.ID);
            }
            bool isVisaUpdated = true, isDataDeleted = true;

            // Check for ChangeSet and send to Stage Table for Approval.
            if (model.SendForApproval)
            {
                int stageSaveCount = _stageService.SaveModelToMultiRecordStage<EmployeeVisa>(id, model, isDeleted);
                if (stageSaveCount == -999)
                    return null;

                int approverID = HookApproval(id, model.NewModel.ID, model.ModuleId);
                var approverPerson = service.First<Person>(x => x.ID == approverID);
                var employee = service.First<Person>(x => x.ID == id);
                string empName = employee.FirstName + " " + empVisaList.Person.LastName;
                emailService.SendUserProfileApproval(id, empName, "Visa Detail", employee.PersonEmployment.First().OrganizationEmail, approverPerson.PersonEmployment.First().OrganizationEmail, employee.Image);
            }
            else if (id != model.NewModel.SearchUserID && !model.SendForApproval && model.NewModel.SearchUserID != 0) //When HR update other employee card details 
            {
                if (model.OldModel != null && model.OldModel.ID != 0)
                {
                    empVisaList = service.First<PersonVisa>(x => x.Person.ID == model.NewModel.SearchUserID && x.IsDeleted == false && x.ID == model.OldModel.ID);
                }
                if (isDeleted)
                {
                    var deletedVisa = empVisaList;
                    isDataDeleted = service.SoftRemove<PersonVisa>(deletedVisa, x => x.Person.ID == model.NewModel.SearchUserID);
                }
                else
                {
                    if (model.NewModel.ID == 0)
                    {
                        var newEmployeeVisa = Mapper.Map<EmployeeVisa, PersonVisa>(model.NewModel);
                        newEmployeeVisa.Person = service.First<Person>(x => x.ID == model.NewModel.SearchUserID);
                        newEmployeeVisa.VisaType = service.First<VisaType>(x => x.ID == model.NewModel.VisaTypeID);
                        newEmployeeVisa.Country = service.First<Country>(x => x.ID == model.NewModel.CountryID);
                        isVisaUpdated = service.Create<PersonVisa>(newEmployeeVisa, x => x.VisaType.ID == model.NewModel.VisaTypeID && x.Person.ID == model.NewModel.SearchUserID && x.ID == model.NewModel.ID);
                    }
                    else
                    {
                        var oldEmployeeVisa = empVisaList;
                        var newEmployeeVisa = Mapper.Map<EmployeeVisa, PersonVisa>(model.NewModel);
                        isVisaUpdated = service.Update<PersonVisa>(newEmployeeVisa, oldEmployeeVisa);
                        empVisaList = service.First<PersonVisa>(x => x.Person.ID == model.NewModel.SearchUserID && x.IsDeleted == false && x.ID == oldEmployeeVisa.ID);
                    }
                }
                if (isVisaUpdated || isDataDeleted)
                    service.Finalize(true);

                if (model.NewModel.ID == 0)
                    empVisaList = service.Top<PersonVisa>(0, x => x.Person.ID == model.NewModel.SearchUserID && x.IsDeleted == false).OrderByDescending(x => x.ID).First();
                if (empVisaList != null)
                {
                    model.NewModel = Mapper.Map<PersonVisa, EmployeeVisa>(empVisaList);
                }
            }
            else
            {
                if (isDeleted)
                {
                    var deletedVisa = empVisaList;
                    isDataDeleted = service.SoftRemove<PersonVisa>(deletedVisa, x => x.Person.ID == id);
                }
                else
                {
                    if (model.NewModel.ID == 0)
                    {
                        var newEmployeeVisa = Mapper.Map<EmployeeVisa, PersonVisa>(model.NewModel);
                        newEmployeeVisa.Person = service.First<Person>(x => x.ID == id);
                        newEmployeeVisa.VisaType = service.First<VisaType>(x => x.ID == model.NewModel.VisaTypeID);
                        newEmployeeVisa.Country = service.First<Country>(x => x.ID == model.NewModel.CountryID);
                        isVisaUpdated = service.Create<PersonVisa>(newEmployeeVisa, x => x.VisaType.ID == model.NewModel.VisaTypeID && x.Person.ID == id && x.ID == model.NewModel.ID);
                    }
                    else
                    {
                        var oldEmployeeVisa = empVisaList;
                        var newEmployeeVisa = Mapper.Map<EmployeeVisa, PersonVisa>(model.NewModel);
                        isVisaUpdated = service.Update<PersonVisa>(newEmployeeVisa, oldEmployeeVisa);
                        empVisaList = service.First<PersonVisa>(x => x.Person.ID == id && x.IsDeleted == false && x.ID == oldEmployeeVisa.ID);
                    }
                }
                if (isVisaUpdated || isDataDeleted)
                    service.Finalize(true);

                if (model.NewModel.ID == 0)
                    empVisaList = service.Top<PersonVisa>(0, x => x.Person.ID == id && x.IsDeleted == false).OrderByDescending(x => x.ID).First();
                if (empVisaList != null)
                {
                    model.NewModel = Mapper.Map<PersonVisa, EmployeeVisa>(empVisaList);
                }
            }
            return model;
        }

        public ChangeSet<EmployeeQualification> ManageEmployeeQualifications(int id, ChangeSet<EmployeeQualification> model, bool isDeleted)
        {
            var empQualificationList = new PersonQualificationMapping();
            if (model.OldModel != null && model.OldModel.ID != 0)
            {
                empQualificationList = service.First<PersonQualificationMapping>(x => x.Person.ID == id && x.IsDeleted == false && x.ID == model.OldModel.ID);
            }
            bool isQualificationUpdated = true, isDataDeleted = true;

            // Check for ChangeSet and send to Stage Table for Approval.
            if (model.SendForApproval)
            {
                int stageSaveCount = _stageService.SaveModelToMultiRecordStage<EmployeeQualification>(id, model, isDeleted);
                if (stageSaveCount == -999)
                    return null;

                int approverID = HookApproval(id, model.NewModel.ID, model.ModuleId);
                int approvalID = GetApprovalID(id, model.ModuleId);
                model.NewModel.ApprovalID = approvalID;
                UpdateNewEntryJSONStringInDB<EmployeeQualification>(model, id, model.ModuleId);
                var approverPerson = service.First<Person>(x => x.ID == approverID);
                var employee = service.First<Person>(x => x.ID == id);
                string empName = employee.FirstName + " " + employee.LastName;
                emailService.SendUserProfileApproval(id, empName, "Education Detail", employee.PersonEmployment.First().OrganizationEmail, approverPerson.PersonEmployment.First().OrganizationEmail, employee.Image);
            }
            else if (id != model.NewModel.SearchUserID && !model.SendForApproval && model.NewModel.SearchUserID != 0) //When HR update other employee card details 
            {
                if (model.OldModel != null && model.OldModel.ID != 0)
                {
                    empQualificationList = service.First<PersonQualificationMapping>(x => x.Person.ID == model.NewModel.SearchUserID && x.IsDeleted == false && x.ID == model.OldModel.ID);
                }

                if (isDeleted)
                {
                    var deletedQualification = empQualificationList;
                    isDataDeleted = service.SoftRemove<PersonQualificationMapping>(deletedQualification, x => x.Person.ID == model.NewModel.SearchUserID);
                }
                else
                {
                    int approverID = HREditingApproval(model.NewModel.SearchUserID, model.NewModel.ID, model.ModuleId, id);

                    if (model.NewModel.ID == 0)
                    {
                        var newEmployeeQualifications = Mapper.Map<EmployeeQualification, PersonQualificationMapping>(model.NewModel);
                        newEmployeeQualifications.Person = service.First<Person>(x => x.ID == model.NewModel.SearchUserID);
                        newEmployeeQualifications.Qualification = service.First<Qualification>(x => x.ID == model.NewModel.QualificationID);
                        isQualificationUpdated = service.Create<PersonQualificationMapping>(newEmployeeQualifications, x => x.Qualification.ID == model.NewModel.QualificationID && x.Person.ID == model.NewModel.SearchUserID && x.ID == model.NewModel.ID);
                    }
                    else
                    {
                        var oldEmployeeQualifications = empQualificationList;
                        var newEmployeeQualifications = Mapper.Map<EmployeeQualification, PersonQualificationMapping>(model.NewModel);
                        isQualificationUpdated = service.Update<PersonQualificationMapping>(newEmployeeQualifications, oldEmployeeQualifications);
                        empQualificationList = service.First<PersonQualificationMapping>(x => x.Person.ID == model.NewModel.SearchUserID && x.IsDeleted == false && x.ID == oldEmployeeQualifications.ID);
                    }
                }
                if (isQualificationUpdated || isDataDeleted)
                    service.Finalize(true);

                if (model.NewModel.ID == 0)
                    empQualificationList = service.Top<PersonQualificationMapping>(0, x => x.Person.ID == model.NewModel.SearchUserID && x.IsDeleted == false).OrderByDescending(x => x.ID).First();
                if (empQualificationList != null)
                {
                    model.NewModel = Mapper.Map<PersonQualificationMapping, EmployeeQualification>(empQualificationList);
                }
            }
            else
            {
                if (isDeleted)
                {
                    var deletedQualification = empQualificationList;
                    isDataDeleted = service.SoftRemove<PersonQualificationMapping>(deletedQualification, x => x.Person.ID == id);
                }
                else
                {
                    if (model.NewModel.ID == 0)
                    {
                        var newEmployeeQualifications = Mapper.Map<EmployeeQualification, PersonQualificationMapping>(model.NewModel);
                        newEmployeeQualifications.Person = service.First<Person>(x => x.ID == id);
                        newEmployeeQualifications.Qualification = service.First<Qualification>(x => x.ID == model.NewModel.QualificationID);
                        isQualificationUpdated = service.Create<PersonQualificationMapping>(newEmployeeQualifications, x => x.Qualification.ID == model.NewModel.QualificationID && x.Person.ID == id && x.ID == model.NewModel.ID);
                    }
                    else
                    {
                        var oldEmployeeQualifications = empQualificationList;
                        var newEmployeeQualifications = Mapper.Map<EmployeeQualification, PersonQualificationMapping>(model.NewModel);
                        isQualificationUpdated = service.Update<PersonQualificationMapping>(newEmployeeQualifications, oldEmployeeQualifications);
                        empQualificationList = service.First<PersonQualificationMapping>(x => x.Person.ID == id && x.IsDeleted == false && x.ID == oldEmployeeQualifications.ID);
                    }
                }
                if (isQualificationUpdated || isDataDeleted)
                    service.Finalize(true);

                if (model.NewModel.ID == 0)
                    empQualificationList = service.Top<PersonQualificationMapping>(0, x => x.Person.ID == id && x.IsDeleted == false).OrderByDescending(x => x.ID).First();
                if (empQualificationList != null)
                {
                    model.NewModel = Mapper.Map<PersonQualificationMapping, EmployeeQualification>(empQualificationList);
                }
            }
            return model;
        }

        public async Task<ChangeSet<EmployeeProfileViewModel>> UpdateEmployeeJoiningDetail(int id, ChangeSet<EmployeeProfileViewModel> model)
        {
            return await Task.Run(() =>
            {
                var oldPersonEmployment = new PersonEmployment();
                if (id == model.NewModel.SearchUserID)
                    oldPersonEmployment = service.First<PersonEmployment>(x => x.Person.ID == id);
                else
                    oldPersonEmployment = service.First<PersonEmployment>(x => x.Person.ID == model.NewModel.SearchUserID);

                if (oldPersonEmployment != null)
                {
                    // Check for ChangeSet and send to Stage Table for Approval.
                    if (model.SendForApproval)
                    {
                        int stageSaveCount = _stageService.SaveModelToStage<EmployeeProfileViewModel>(id, model.NewModel.ID, model);
                        if (stageSaveCount == -999)
                            return null;

                        int approverID = HookApproval(id, model.NewModel.ID, model.ModuleId);
                        var approverPerson = service.First<Person>(x => x.ID == approverID);
                        var employee = service.First<Person>(x => x.ID == id);
                        string empName = employee.FirstName + " " + employee.LastName;
                        emailService.SendUserProfileApproval(id, empName, "Joining Detail", employee.PersonEmployment.First().OrganizationEmail, approverPerson.PersonEmployment.First().OrganizationEmail, employee.Image);
                    }
                    else if (id != model.NewModel.SearchUserID && !model.SendForApproval && model.NewModel.SearchUserID != 0) //When HR update other employee card details 
                    {
                        var employee = new Person();
                        var viewModel1 = model.NewModel;
                        viewModel1.ID = model.NewModel.SearchUserID;
                        //var newPersonEmployment = Mapper.Map<EmployeeProfileViewModel, PersonEmployment>(viewModel);
                        int approverID = HREditingApproval(model.NewModel.SearchUserID, model.NewModel.ID, model.ModuleId, id);
                        // One which is having one-to-one relationship will not check for existence and will directly go for Update.
                        //bool isEmploymentUpdated = service.Update<PersonEmployment>(newPersonEmployment, oldPersonEmployment);
                        //if (isEmploymentUpdated)
                        //    service.Finalize(true);

                        //oldPersonEmployment = service.First<PersonEmployment>(x => x.Person.ID == model.NewModel.SearchUserID);
                        //viewModel = Mapper.Map<PersonEmployment, EmployeeProfileViewModel>(oldPersonEmployment);


                        employee = service.First<Person>(x => x.ID == model.NewModel.SearchUserID);

                        bool isBasicUpdated, isPersonalUpdated, isEmploymentUpdated = true, isDependedUpdated = true;

                        var viewModel = Mapper.Map<EmployeeProfileViewModel, EmployeePersonalDetails>(model.NewModel);
                        //viewModel.ID = model.NewModel.SearchUserID;
                        var newPerson = Mapper.Map<EmployeeProfileViewModel, Person>(viewModel1);
                        newPerson.Active = employee.Active;// this need to be change for Hr can Active/InActive Emp
                        newPerson.Gender = employee.Gender;//#159726912: 'Gender' field on the personal page wiped out on changing the office location
                                                           //var newPersonEmployment = Mapper.Map<EmployeeProfileViewModel, PersonEmployment>(viewModel1);
                        var newPersonPersonal = Mapper.Map<EmployeeProfileViewModel, PersonPersonal>(viewModel1);


                        // One which is having one-to-one relationship will not check for existence and will directly go for Update.
                        isBasicUpdated = service.Update<Person>(newPerson, employee);
                        isPersonalUpdated = service.Update<PersonPersonal>(newPersonPersonal, employee.PersonPersonal);
                        var checkIfEmploymentExists = employee.PersonEmployment.Where(x => x.PersonID == model.NewModel.SearchUserID).FirstOrDefault();
                        var checkIfChildExists = employee.PersonDependents.Where(x => x.Relation == 6 || x.Relation == 7)
                                                                                    .OrderBy(x => x.DateOfBirth).FirstOrDefault();
                        isEmploymentUpdated = true;
                        isDependedUpdated = true;
                        // One which is having one-to-many relationship will check for existence and then only it will go for Update.
                        //if (checkIfEmploymentExists != null)
                        //    isEmploymentUpdated = service.Update<PersonEmployment>(newPersonEmployment, employee.PersonEmployment.FirstOrDefault());
                        PhoenixEntities context = new PhoenixEntities();

                        //var personEmployment = service.First<PersonEmployment>(x => x.PersonID == model.NewModel.SearchUserID);
                        var personEmployment = context.PersonEmployment.FirstOrDefault(t => t.PersonID == model.NewModel.SearchUserID);

                        personEmployment.ConfirmationDate = model.NewModel.ConfirmationDate;
                        personEmployment.DesignationID = model.NewModel.CurrentDesignationID;
                        personEmployment.ProbationReviewDate = model.NewModel.ProbationReviewDate;
                        personEmployment.OrganizationEmail = model.NewModel.OrganizationEmail;
                        personEmployment.OfficeLocation = Convert.ToInt32(model.NewModel.OL);
                        personEmployment.ExitDate = model.NewModel.ExitDate;
                        personEmployment.RejoinedWithinYear = model.NewModel.RejoinedWithinYear;
                        //personEmployment.WorkLocation = model.NewModel.EmployeeOrganizationdetails.WL;
                        personEmployment.JoiningDate = model.NewModel.JoiningDate;
                        personEmployment.EmploymentStatus = Convert.ToInt32(model.NewModel.employmentStatusID);
                        context.SaveChanges();



                        if (isBasicUpdated && isPersonalUpdated && isEmploymentUpdated && isDependedUpdated)
                            service.Finalize(true);

                        employee = service.First<Person>(x => x.ID == model.NewModel.SearchUserID);
                        viewModel1 = Mapper.Map<Person, EmployeeProfileViewModel>(employee);
                        model.NewModel = Mapper.Map<EmployeePersonalDetails, EmployeeProfileViewModel>(viewModel);
                    }
                    else
                    {
                        var viewModel = model.NewModel;
                        viewModel.ID = id;
                        var newPersonEmployment = Mapper.Map<EmployeeProfileViewModel, PersonEmployment>(viewModel);

                        // One which is having one-to-one relationship will not check for existence and will directly go for Update.
                        bool isEmploymentUpdated = service.Update<PersonEmployment>(newPersonEmployment, oldPersonEmployment);
                        if (isEmploymentUpdated)
                            service.Finalize(true);

                        oldPersonEmployment = service.First<PersonEmployment>(x => x.Person.ID == id);
                        viewModel = Mapper.Map<PersonEmployment, EmployeeProfileViewModel>(oldPersonEmployment);
                    }
                    return model;
                }
                return null;
            });
        }

        public async Task<ChangeSet<EmployeeOrganizaionDetails>> UpdateEmployeeOrgDetail(int personId, int id, ChangeSet<EmployeeOrganizaionDetails> model)
        {
            return await Task.Run(() =>
            {
                var oldPersonEmployment = service.First<PersonEmployment>(x => x.Person.ID == id);
                if (oldPersonEmployment != null)
                {
                    bool isEmploymentUpdated, isReportingUpdated = true;

                    var viewModel = model.NewModel;
                    viewModel.ID = id;
                    var newPersonEmployment = Mapper.Map<EmployeeOrganizaionDetails, PersonEmployment>(viewModel);

                    // Update employment details
                    isEmploymentUpdated = service.Update<PersonEmployment>(newPersonEmployment, oldPersonEmployment);

                    var oldPersonReporting = service.First<PersonReporting>(x => x.PersonID == id);
                    if (oldPersonReporting != null)
                    {
                        var newPersonReporting = Mapper.Map<EmployeeOrganizaionDetails, PersonReporting>(viewModel);
                        isReportingUpdated = service.Update<PersonReporting>(newPersonReporting, oldPersonReporting);
                    }

                    bool isReportingManagerChanged = model.NewModel.ReportingTo != model.OldModel.ReportingTo;
                    bool isExitManagerChanged = model.NewModel.ExitProcessManager != model.OldModel.ExitProcessManager;

                    if (isReportingManagerChanged)
                    {
                        using (PhoenixEntities context = new PhoenixEntities())
                        {
                            QueryHelper.changeRepotingManger(id, model.NewModel.ReportingTo);
                        }
                    }

                    if (isExitManagerChanged)
                    {
                        using (PhoenixEntities context = new PhoenixEntities())
                        {
                            QueryHelper.changeExitManager(id, model.OldModel.ExitProcessManager, model.NewModel.ExitProcessManager);
                        }
                    }

                    if (isEmploymentUpdated && isReportingUpdated)
                        service.Finalize(true);

                    oldPersonEmployment = service.First<PersonEmployment>(x => x.Person.ID == id);
                    viewModel = Mapper.Map<PersonEmployment, EmployeeOrganizaionDetails>(oldPersonEmployment);

                    // Send email only when Reporting Manager or Exit Manager is change
                    if (isReportingManagerChanged || isExitManagerChanged)
                    {
                        emailService.SendOrgDetailsUpdateStatus(personId, id, model.NewModel);
                    }

                    return model;
                }
                return null;
            });
        }

        public ChangeSet<EmployeeEmergencyContact> ManageEmployeeEmergencyContacts(int id, ChangeSet<EmployeeEmergencyContact> model, bool isDeleted)
        {
            var empContactList = new PersonContact();
            if (model.OldModel != null && model.OldModel.ID != 0)
            {
                empContactList = service.First<PersonContact>(x => x.Person.ID == id && x.IsDeleted == false && x.ID == model.OldModel.ID);
            }
            bool isContactUpdated = true, isDataDeleted = true;

            // Check for ChangeSet and send to Stage Table for Approval.
            if (model.SendForApproval)
            {
                int stageSaveCount = _stageService.SaveModelToMultiRecordStage<EmployeeEmergencyContact>(id, model, isDeleted);
                if (stageSaveCount == -999)
                    return null;

                int approverID = HookApproval(id, model.NewModel.ID, model.ModuleId);
                int approvalID = GetApprovalID(id, model.ModuleId);
                model.NewModel.ApprovalID = approvalID;
                UpdateNewEntryJSONStringInDB<EmployeeEmergencyContact>(model, id, model.ModuleId);
                var approverPerson = service.First<Person>(x => x.ID == approverID);
                var employee = service.First<Person>(x => x.ID == id);
                string empName = employee.FirstName + " " + employee.LastName;
                emailService.SendUserProfileApproval(id, empName, "Emergency Contact", employee.PersonEmployment.First().OrganizationEmail, approverPerson.PersonEmployment.First().OrganizationEmail, employee.Image);
            }
            else if (id != model.NewModel.SearchUserID && !model.SendForApproval && model.NewModel.SearchUserID != 0) //When HR update other employee card details 
            {
                if (model.OldModel != null && model.OldModel.ID != 0)
                {
                    empContactList = service.First<PersonContact>(x => x.Person.ID == model.NewModel.SearchUserID && x.IsDeleted == false && x.ID == model.OldModel.ID);
                }

                int approverID = HREditingApproval(model.NewModel.SearchUserID, model.NewModel.ID, model.ModuleId, id);

                if (isDeleted)
                {
                    var deletedEmergencyContact = empContactList;
                    isDataDeleted = service.SoftRemove<PersonContact>(deletedEmergencyContact, x => x.Person.ID == model.NewModel.SearchUserID);
                }
                else
                {
                    if (model.NewModel.ID == 0)
                    {
                        var newEmployeeQualifications = Mapper.Map<EmployeeEmergencyContact, PersonContact>(model.NewModel);
                        newEmployeeQualifications.Person = service.First<Person>(x => x.ID == model.NewModel.SearchUserID);
                        isContactUpdated = service.Create<PersonContact>(newEmployeeQualifications, x => x.Name.Equals(model.NewModel.ContactPersonName, StringComparison.InvariantCultureIgnoreCase) && x.Person.ID == model.NewModel.SearchUserID && x.ID == model.NewModel.ID);
                    }
                    else
                    {
                        var oldEmployeeQualifications = empContactList;
                        var newEmployeeQualifications = Mapper.Map<EmployeeEmergencyContact, PersonContact>(model.NewModel);
                        isContactUpdated = service.Update<PersonContact>(newEmployeeQualifications, oldEmployeeQualifications);
                        empContactList = service.First<PersonContact>(x => x.Person.ID == model.NewModel.SearchUserID && x.IsDeleted == false && x.ID == oldEmployeeQualifications.ID);
                    }
                }
                if (isContactUpdated)
                    service.Finalize(true);

                if (model.NewModel.ID == 0)
                    empContactList = service.Top<PersonContact>(0, x => x.Person.ID == model.NewModel.SearchUserID && x.IsDeleted == false).OrderByDescending(x => x.ID).First();
                if (empContactList != null)
                {
                    model.NewModel = Mapper.Map<PersonContact, EmployeeEmergencyContact>(empContactList);
                }
            }
            else
            {
                if (isDeleted)
                {
                    var deletedEmergencyContact = empContactList;
                    isDataDeleted = service.SoftRemove<PersonContact>(deletedEmergencyContact, x => x.Person.ID == id);
                }
                else
                {
                    if (model.NewModel.ID == 0)
                    {
                        var newEmployeeQualifications = Mapper.Map<EmployeeEmergencyContact, PersonContact>(model.NewModel);
                        newEmployeeQualifications.Person = service.First<Person>(x => x.ID == id);
                        isContactUpdated = service.Create<PersonContact>(newEmployeeQualifications, x => x.Name.Equals(model.NewModel.ContactPersonName, StringComparison.InvariantCultureIgnoreCase) && x.Person.ID == id && x.ID == model.NewModel.ID);
                    }
                    else
                    {
                        var oldEmployeeQualifications = empContactList;
                        var newEmployeeQualifications = Mapper.Map<EmployeeEmergencyContact, PersonContact>(model.NewModel);
                        isContactUpdated = service.Update<PersonContact>(newEmployeeQualifications, oldEmployeeQualifications);
                        empContactList = service.First<PersonContact>(x => x.Person.ID == id && x.IsDeleted == false && x.ID == oldEmployeeQualifications.ID);
                    }
                }
                if (isContactUpdated)
                    service.Finalize(true);

                if (model.NewModel.ID == 0)
                    empContactList = service.Top<PersonContact>(0, x => x.Person.ID == id && x.IsDeleted == false).OrderByDescending(x => x.ID).First();
                if (empContactList != null)
                {
                    model.NewModel = Mapper.Map<PersonContact, EmployeeEmergencyContact>(empContactList);
                }
            }
            return model;
        }

        public ChangeSet<EmployeeMedicalHistory> ManageEmployeeMedicalHistory(int id, ChangeSet<EmployeeMedicalHistory> model, bool isDeleted)
        {
            var empMedicalHistoryList = new PersonMedicalHistory();
            if (model.OldModel != null && model.OldModel.ID != 0)
            {
                empMedicalHistoryList = service.First<PersonMedicalHistory>(x => x.Person.ID == id && x.IsDeleted == false && x.ID == model.OldModel.ID);
            }
            bool isContactUpdated = true, isDataDeleted = true;

            // Check for ChangeSet and send to Stage Table for Approval.
            if (model.SendForApproval)
            {
                int stageSaveCount = _stageService.SaveModelToMultiRecordStage<EmployeeMedicalHistory>(id, model, isDeleted);
                if (stageSaveCount == -999)
                    return null;

                int approverID = HookApproval(id, model.NewModel.ID, model.ModuleId);
                int approvalID = GetApprovalID(id, model.ModuleId);
                model.NewModel.ApprovalID = approvalID;
                UpdateNewEntryJSONStringInDB<EmployeeMedicalHistory>(model, id, model.ModuleId);
                var approverPerson = service.First<Person>(x => x.ID == approverID);
                var employee = service.First<Person>(x => x.ID == id);
                string empName = employee.FirstName + " " + employee.LastName;
                emailService.SendUserProfileApproval(id, empName, "Medical History", employee.PersonEmployment.First().OrganizationEmail, approverPerson.PersonEmployment.First().OrganizationEmail, employee.Image);
            }
            else if (id != model.NewModel.SearchUserID && !model.SendForApproval && model.NewModel.SearchUserID != 0) //When HR update other employee card details 
            {
                if (model.OldModel != null && model.OldModel.ID != 0)
                {
                    empMedicalHistoryList = service.First<PersonMedicalHistory>(x => x.Person.ID == model.NewModel.SearchUserID && x.IsDeleted == false && x.ID == model.OldModel.ID);
                }
                if (isDeleted)
                {
                    var deletedPersonSkill = empMedicalHistoryList;
                    isDataDeleted = service.SoftRemove<PersonMedicalHistory>(deletedPersonSkill, x => x.Person.ID == model.NewModel.SearchUserID);
                }
                else
                {
                    int approverID = HREditingApproval(model.NewModel.SearchUserID, model.NewModel.ID, model.ModuleId, id);

                    if (model.NewModel.ID == 0)
                    {
                        var newEmployeeQualifications = Mapper.Map<EmployeeMedicalHistory, PersonMedicalHistory>(model.NewModel);
                        newEmployeeQualifications.Person = service.First<Person>(x => x.ID == model.NewModel.SearchUserID);
                        isContactUpdated = service.Create<PersonMedicalHistory>(newEmployeeQualifications, x => x.Year == model.NewModel.Year && x.ID == model.NewModel.ID);
                    }
                    else
                    {
                        var oldEmployeeQualifications = empMedicalHistoryList;
                        var newEmployeeQualifications = Mapper.Map<EmployeeMedicalHistory, PersonMedicalHistory>(model.NewModel);
                        isContactUpdated = service.Update<PersonMedicalHistory>(newEmployeeQualifications, oldEmployeeQualifications);
                        empMedicalHistoryList = service.First<PersonMedicalHistory>(x => x.Person.ID == model.NewModel.SearchUserID && x.IsDeleted == false && x.ID == oldEmployeeQualifications.ID);
                    }
                }
                if (isContactUpdated)
                    service.Finalize(true);

                if (model.NewModel.ID == 0)
                    empMedicalHistoryList = service.Top<PersonMedicalHistory>(0, x => x.Person.ID == model.NewModel.SearchUserID && x.IsDeleted == false).OrderByDescending(x => x.ID).First();
                if (empMedicalHistoryList != null)
                {
                    model.NewModel = Mapper.Map<PersonMedicalHistory, EmployeeMedicalHistory>(empMedicalHistoryList);
                }
            }
            else
            {
                if (isDeleted)
                {
                    var deletedPersonSkill = empMedicalHistoryList;
                    isDataDeleted = service.SoftRemove<PersonMedicalHistory>(deletedPersonSkill, x => x.Person.ID == id);
                }
                else
                {
                    if (model.NewModel.ID == 0)
                    {
                        var newEmployeeQualifications = Mapper.Map<EmployeeMedicalHistory, PersonMedicalHistory>(model.NewModel);
                        newEmployeeQualifications.Person = service.First<Person>(x => x.ID == id);
                        isContactUpdated = service.Create<PersonMedicalHistory>(newEmployeeQualifications, x => x.Year == model.NewModel.Year && x.ID == model.NewModel.ID);
                    }
                    else
                    {
                        var oldEmployeeQualifications = empMedicalHistoryList;
                        var newEmployeeQualifications = Mapper.Map<EmployeeMedicalHistory, PersonMedicalHistory>(model.NewModel);
                        isContactUpdated = service.Update<PersonMedicalHistory>(newEmployeeQualifications, oldEmployeeQualifications);
                        empMedicalHistoryList = service.First<PersonMedicalHistory>(x => x.Person.ID == id && x.IsDeleted == false && x.ID == oldEmployeeQualifications.ID);
                    }
                }
                if (isContactUpdated)
                    service.Finalize(true);

                if (model.NewModel.ID == 0)
                    empMedicalHistoryList = service.Top<PersonMedicalHistory>(0, x => x.Person.ID == id && x.IsDeleted == false).OrderByDescending(x => x.ID).First();
                if (empMedicalHistoryList != null)
                {
                    model.NewModel = Mapper.Map<PersonMedicalHistory, EmployeeMedicalHistory>(empMedicalHistoryList);
                }
            }
            return model;
        }

        public async Task<List<DropdownItems>> GetDropdownValueVisaType()
        {
            return await Task.Run(() =>
            {
                List<DropdownItems> itemList = new List<DropdownItems>();

                var visaTypeList = service.All<VisaType>(x => x.VisaType1 != null).ToList();
                foreach (var visaType in visaTypeList.OrderBy(x => x.VisaType1))
                {
                    DropdownItems dropDownItems = new DropdownItems
                    {
                        ID = visaType.ID,
                        Text = visaType.VisaType1
                    };
                    itemList.Add(dropDownItems);
                }
                return itemList;
            });
        }

        public async Task<List<DropdownItems>> GetDropdownValueCountry()
        {
            return await Task.Run(() =>
            {
                var countryList = service.All<Country>(x => x.Name != null).OrderBy(x => x.Name)
                          .Select(t => new DropdownItems()
                          {
                              ID = t.ID,
                              Text = t.Name
                          }).ToList();
                return countryList;
            });
        }

        public async Task<List<DropdownItems>> GetDropdownValueQualification()
        {
            return await Task.Run(() =>
            {
                var qualificationList = service.All<Qualification>().OrderBy(x => x.QualificationName)
                                          .Select(t => new DropdownItems()
                                          {
                                              ID = t.ID,
                                              Text = t.QualificationName
                                          }).ToList();
                return qualificationList;
            });
        }

        public async Task<List<DropdownItems>> GetDropdownValueSkills()
        {
            return await Task.Run(() =>
            {
                var skillsList = service.All<SkillMatrix>()
                                       //.Where(x => x.Active && !x.IsDeleted)
                                       .OrderBy(x => x.Name)
                                       .Select(t => new DropdownItems()
                                       {
                                           ID = t.ID,
                                           Text = t.Name
                                       }).ToList();

                return skillsList;
            });
        }

        public async Task<List<DropdownItems>> GetDropdownValueCompetency()
        {
            return await Task.Run(() =>
            {
                var competencyList = service.All<Competency>()
                                       //.Where(x => x.Active && !x.IsDeleted)
                                       .OrderBy(x => x.Name)
                                       .Select(t => new DropdownItems()
                                       {
                                           ID = t.ID,
                                           Text = t.Name
                                       }).ToList();

                return competencyList;
            });
        }

        public async Task<List<DropdownItems>> GetDropdownValueCertification()
        {
            return await Task.Run(() =>
            {
                var certificationList = service.All<Certifications>()
                                          .OrderBy(x => x.CertificateName)
                                          .Select(t => new DropdownItems()
                                          {
                                              ID = t.ID,
                                              Text = t.CertificateName
                                          }).ToList();
                return certificationList;
            });
        }

        public async Task<List<DropdownItems>> GetDropdownValueDeliveryUnit()
        {
            return await Task.Run(() =>
            {
                var deliveryUnitList = service.All<DeliveryUnit>(t => t.Active ?? false)
                                                .OrderBy(x => x.Name)
                                                .Select(t => new DropdownItems()
                                                {
                                                    ID = t.ID,
                                                    Text = t.Name
                                                });
                return deliveryUnitList.ToList();
            });
        }

        public async Task<List<DropdownItems>> GetDropdownValueDeliveryTeam()
        {
            return await Task.Run(() =>
            {
                var deliveryTeamList = service.All<DeliveryTeam>(t => t.IsActive ?? false)
                                            .Select(t => new DropdownItems()
                                            {
                                                ID = t.ID,
                                                Text = t.Name
                                            });

                return deliveryTeamList.OrderBy(x => x.Text).ToList();
            });
        }

        public async Task<List<DropdownItems>> GetDropdownValueResourcePool()
        {
            return await Task.Run(() =>
            {
                var resourcePool = service.All<ResourcePool>(t => !t.IsDeleted)
                                            .OrderBy(x => x.Name)
                                            .Select(t => new DropdownItems()
                                            {
                                                ID = t.ID,
                                                Text = t.Name
                                            });
                return resourcePool.ToList();
            });
        }

        public async Task<List<DropdownItems>> GetDropdownValueReportingManager()
        {
            return await Task.Run(() =>
            {
                PhoenixEntities dbContext = new PhoenixEntities();
                List<DropdownItems> itemList = new List<DropdownItems>();

                var reportingManager = from p in dbContext.People
                                       join pe in dbContext.PersonEmployment on p.ID equals pe.PersonID
                                       join d in dbContext.Designations on pe.DesignationID equals d.ID into managers
                                       from m in managers.DefaultIfEmpty()
                                       where m.Grade >= 3 && p.Active == true
                                       select new { p.ID, p.FirstName, p.LastName };

                foreach (var person in reportingManager)
                {
                    //var person = reportingTo;
                    if (person != null)
                    {
                        DropdownItems dropDownItems = new DropdownItems
                        {
                            ID = person.ID,
                            Text = person.FirstName + " " + person.LastName + "(" + person.ID + ")"
                        };
                        itemList.Add(dropDownItems);
                    }
                }
                return itemList.OrderBy(x => x.Text).ToList();
            });
        }

        public async Task<List<DropdownItems>> GetDropdownValueExitProcessManager()
        {
            return await Task.Run(() =>
            {
                PhoenixEntities dbContext = new PhoenixEntities();
                List<DropdownItems> itemList = new List<DropdownItems>();

                var exitProcessManager = from p in dbContext.People
                                         join pe in dbContext.PersonEmployment on p.ID equals pe.PersonID
                                         join d in dbContext.Designations on pe.DesignationID equals d.ID into managers
                                         from m in managers.DefaultIfEmpty()
                                         where m.Grade >= 3 && p.Active == true
                                         select new { p.ID, p.FirstName, p.LastName };

                foreach (var person in exitProcessManager)
                {
                    //var person = reportingTo;
                    if (person != null)
                    {
                        DropdownItems dropDownItems = new DropdownItems
                        {
                            ID = person.ID,
                            Text = person.FirstName + " " + person.LastName + "(" + person.ID + ")"
                        };
                        itemList.Add(dropDownItems);
                    }
                }
                return itemList.OrderBy(x => x.Text).ToList();
            });
        }

        public async Task<List<DropdownItems>> GetDropdownValueSeparationReason()
        {
            return await Task.Run(() =>
            {
                var reasonList = service.All<SeparationReasons>(x => x.IsActive == true).
                                       Select(t => new DropdownItems()
                                       {
                                           ID = t.ID,
                                           Text = t.ReasonDescription
                                       }).ToList();
                return reasonList.OrderBy(x => x.Text).ToList();
            });
        }

        public async Task<List<DropdownItems>> GetDropdownValueReasonMaster()
        {
            return await Task.Run(() =>
            {
                var reasonMstList = service.All<SeparationReasonMaster>(x => x.IsActive == true)
                                        .Select(t => new DropdownItems
                                        {
                                            ID = t.ID,
                                            Text = t.ReasonCategoty.ToString()
                                        }).ToList();
                return reasonMstList.OrderBy(x => x.Text).ToList();
            });
        }

        public async Task<List<DropdownItems>> GetDropdownValueOfficeLocation()
        {
            return await Task.Run(() =>
            {
                var locationMstList = service.All<WorkLocation>(x => x.IsDeleted == false && x.ParentLocation == null)
                                        .Select(t => new DropdownItems
                                        {
                                            ID = t.ID,
                                            Text = t.LocationName.ToString()
                                        }).ToList();
                return locationMstList.OrderBy(x => x.Text).ToList();
            });
        }

        public async Task<List<DropdownItems>> GetDropdownValueWorkLocation()
        {
            return await Task.Run(() =>
            {
                var locationMstList = service.All<WorkLocation>(x => x.IsDeleted == false && x.ParentLocation != null)
                                        .Select(t => new DropdownItems
                                        {
                                            ID = t.ID,
                                            Text = t.LocationName.ToString()
                                        }).ToList();
                return locationMstList.OrderBy(x => x.Text).ToList();
            });
        }
        //public async Task<List<DropdownItems>> GetDropdownValueBGParameter()
        //{
        //    return await Task.Run(() =>
        //    {
        //        var bgParameterList = service.All<BGParameterList>(x => x.IsDeleted == false)
        //                                .Select(t => new DropdownItems
        //                                {
        //                                    ID = t.ID,
        //                                    Text = t.Name.ToString()
        //                                }).ToList();
        //        return bgParameterList.OrderBy(x => x.Text).ToList();
        //    });
        //}
        public async Task<Dictionary<string, List<DropdownItems>>> GetDropdowns(string moduleName)
        {
            var lists = moduleName.Split(',');

            Dictionary<string, List<DropdownItems>> outputList = new Dictionary<string, List<DropdownItems>>();

            foreach (var item in lists)
            {
                PhoenixEntities dbContext = new PhoenixEntities();
                List<DropdownItems> itemList = new List<DropdownItems>();
                // The switch case needs to be refactored. Now it is hardcoded as "VisaType" string is not getting converted into VisaType DbEntity.
                switch (item.Trim())
                {

                    case "VisaType":
                        itemList = await this.GetDropdownValueVisaType();
                        break;

                    case "Country":
                        itemList = await this.GetDropdownValueCountry();
                        break;

                    case "Qualification":
                        itemList = await this.GetDropdownValueQualification();
                        break;

                    case "Relation":
                        var relationList = new Dictionary<int, string>();
                        relationList.Add(1, "Mother");
                        relationList.Add(2, "Father");
                        relationList.Add(3, "Brother");
                        relationList.Add(4, "Sister");
                        relationList.Add(5, "Wife");
                        relationList.Add(6, "Son");
                        relationList.Add(7, "Daughter");
                        relationList.Add(8, "Mother-in-law");
                        relationList.Add(9, "Father-in-law");
                        relationList.Add(10, "Grandfather");
                        relationList.Add(11, "Grandmother");
                        relationList.Add(12, "Friend");
                        relationList.Add(13, "Husband");
                        relationList.Add(14, "Other");
                        foreach (var relation in relationList)
                        {
                            DropdownItems dropDownItems = new DropdownItems
                            {
                                ID = relation.Key,
                                Text = relation.Value
                            };
                            itemList.Add(dropDownItems);
                        }
                        break;

                    case "Skills":
                        itemList = await this.GetDropdownValueSkills();
                        break;

                    case "Competency":
                        itemList = await this.GetDropdownValueCompetency();
                        break;

                    case "Certification":
                        itemList = await this.GetDropdownValueCertification();
                        break;

                    case "Gender":
                        var genderList = new Dictionary<int, string>();
                        genderList.Add(1, "Male");
                        genderList.Add(2, "Female");
                        foreach (var gender in genderList)
                        {
                            DropdownItems dropDownItems = new DropdownItems
                            {
                                ID = gender.Key,
                                Text = gender.Value
                            };
                            itemList.Add(dropDownItems);
                        }
                        break;

                    case "DeliveryUnit":
                        itemList = await this.GetDropdownValueDeliveryUnit();
                        break;

                    case "DeliveryTeam":
                        itemList = await this.GetDropdownValueDeliveryTeam();
                        break;

                    case "ResourcePool":
                        itemList = await this.GetDropdownValueResourcePool();
                        break;

                    case "ReportingManager":
                        itemList = await this.GetDropdownValueReportingManager();
                        break;

                    case "ExitProcessManager":
                        itemList = await this.GetDropdownValueExitProcessManager();
                        break;


                    case "SeparationReason":
                        itemList = await this.GetDropdownValueSeparationReason();
                        break;

                    case "ReasonMaster":
                        itemList = await this.GetDropdownValueReasonMaster();
                        break;

                    case "DeptClearanceStatus":
                        var statusList = new Dictionary<int, string>();
                        statusList.Add(1, "Pending");
                        statusList.Add(2, "Completed");
                        statusList.Add(3, "Not Required");

                        foreach (var reason in statusList)
                        {
                            DropdownItems dropDownItems = new DropdownItems
                            {
                                ID = reason.Key,
                                Text = reason.Value
                            };
                            itemList.Add(dropDownItems);
                        }
                        break;
                    case "YearList":
                        var yearList = new Dictionary<int, int>();
                        var currentyear = DateTime.Now.Year + 1;
                        for (int i = currentyear; i >= currentyear - 2; i--)
                        {
                            yearList.Add(i, i);
                        }

                        foreach (var y in yearList)
                        {
                            DropdownItems dropDownItems = new DropdownItems
                            {
                                ID = y.Key,
                                Text = y.Value.ToString()
                            };
                            itemList.Add(dropDownItems);
                        }
                        break;
                    case "OfficeLocation":
                        itemList = await this.GetDropdownValueOfficeLocation();
                        break;
                    case "WorkLocation":
                        itemList = await this.GetDropdownValueWorkLocation();
                        break;
                    //case "BGCParameter":
                    //    itemList = await this.GetDropdownValueBGParameter();
                    //    break;
                    default:
                        break;
                }
                outputList.Add(item.Trim(), itemList);
            }

            return outputList;
        }

        public async Task<IEnumerable<EmployeeApproval>> GetApprovalsList()
        {
            return await Task.Run(() =>
            {
                var viewModel = new List<EmployeeApproval>();
                var stageApprovalsList = new List<Stage>();
                var multiRecordApprovalList = new List<MultiRecordStage>();
                stageApprovalsList = service.Top<Stage>(0, x => x.IsDeleted == false && (x.ApprovalStatus == 0 || x.ApprovalStatus == 3)).OrderByDescending(x => x.By).ToList();
                multiRecordApprovalList = service.Top<MultiRecordStage>(0, x => x.IsDeleted == false && (x.ApprovalStatus == 0 || x.ApprovalStatus == 3)).OrderByDescending(x => x.By).ToList();

                foreach (var stageItem in stageApprovalsList.GroupBy(x => x.By))
                {
                    Person person = new Person();

                    person = service.First<Person>(x => x.ID == stageItem.Key);
                    int? ol = person.PersonEmployment.First().OfficeLocation;
                    var officeLocation = service.Top<WorkLocation>(0, x => x.ID == ol).Select(x => x.LocationName).FirstOrDefault();
                    viewModel.Add(new EmployeeApproval
                    {
                        ID = (int)stageItem.Key,
                        Designation = person.PersonEmployment.First().Designation.Name,
                        Name = person.FirstName + " " + person.LastName,
                        OfficialEmail = person.PersonEmployment.First().OrganizationEmail,
                        OLText = Convert.ToString(officeLocation),
                        ImagePath = person.Image,
                        //EmployeeApprovalViewModelList = (Mapper.Map<List<Stage>, List<IEmployeeApprovalViewModel>>(stageItem.ToList()))
                        ModuleIds = new List<int>(),
                        Status = new List<int>(),
                        SeatingLocation = person.PersonEmployment.First().SeatingLocation,
                        Extension = person.PersonEmployment.First().OfficeExtension,

                    });
                    foreach (var item in stageItem)
                    {
                        viewModel.Where(x => x.Name == (person.FirstName + " " + person.LastName)).First().ModuleIds.Add((int)item.ModuleID);
                    }

                    foreach (var item in stageItem.GroupBy(x => x.ApprovalStatus))
                    {
                        viewModel.Where(x => x.Name == (person.FirstName + " " + person.LastName)).First().Status.Add((int)item.Key);
                    }
                }
                foreach (var multiRecordItem in multiRecordApprovalList.GroupBy(x => x.By))
                {
                    Person person = new Person();
                    person = service.First<Person>(x => x.ID == multiRecordItem.Key);
                    int? ol = person.PersonEmployment.First().OfficeLocation;
                    var officeLocation = service.Top<WorkLocation>(0, x => x.ID == ol).Select(x => x.LocationName).FirstOrDefault();
                    if (viewModel.Where(x => x.Name == (person.FirstName + " " + person.LastName)).Count() <= 0)
                    {
                        viewModel.Add(new EmployeeApproval
                        {
                            ID = (int)multiRecordItem.Key,
                            Designation = person.PersonEmployment.First().Designation.Name,
                            Name = person.FirstName + " " + person.LastName,
                            OfficialEmail = person.PersonEmployment.First().OrganizationEmail,
                            OLText = Convert.ToString(officeLocation), //EnumExtensions.GetEnumDescription((EnumHelpers.Location)person.PersonEmployment.First().OfficeLocation),
                            ImagePath = person.Image,
                            //EmployeeApprovalViewModelList = (Mapper.Map<List<MultiRecordStage>, List<IEmployeeApprovalViewModel>>(multiRecordItem.ToList()))
                            ModuleIds = new List<int>(),
                            Status = new List<int>()
                        });
                        foreach (var item in multiRecordItem.GroupBy(x => x.ModuleID))
                        {
                            viewModel.Where(x => x.Name == (person.FirstName + " " + person.LastName)).First().ModuleIds.Add((int)item.Key);
                        }

                        foreach (var item in multiRecordItem.GroupBy(x => x.ApprovalStatus))
                        {
                            viewModel.Where(x => x.Name == (person.FirstName + " " + person.LastName)).First().Status.Add((int)item.Key);
                        }
                    }
                    else
                    {
                        foreach (var item in multiRecordItem.GroupBy(x => x.ModuleID))
                        {
                            viewModel.Where(x => x.Name == (person.FirstName + " " + person.LastName)).First().ModuleIds.Add((int)item.Key);
                        }

                        foreach (var item in multiRecordItem.GroupBy(x => x.ApprovalStatus))
                        {
                            viewModel.Where(x => x.Name == (person.FirstName + " " + person.LastName)).First().Status.Add((int)item.Key);
                        }
                    }
                }

                return viewModel;
            });
        }

        public async Task<EmployeeApproval> GetApprovalById(int empID)
        {
            return await Task.Run(() =>
            {
                var viewModel = new EmployeeApproval();
                Person person = new Person();
                person = service.First<Person>(x => x.ID == empID);
                int? ol = person.PersonEmployment.First().OfficeLocation;
                var officeLocation = service.Top<WorkLocation>(0, x => x.ID == ol).Select(x => x.LocationName).FirstOrDefault();
                viewModel.ID = empID;
                viewModel.Designation = person.PersonEmployment.First().Designation.Name;
                viewModel.Name = person.FirstName + " " + person.LastName;
                viewModel.OfficialEmail = person.PersonEmployment.First().OrganizationEmail;
                viewModel.OLText = Convert.ToString(officeLocation);//EnumExtensions.GetEnumDescription((EnumHelpers.Location)person.PersonEmployment.First().OfficeLocation);
                viewModel.ImagePath = person.Image;
                viewModel.SeatingLocation = person.PersonEmployment.First().SeatingLocation;
                viewModel.Extension = person.PersonEmployment.First().OfficeExtension;
                var stageApprovalsList = new List<Stage>();
                var multiRecordApprovalList = new List<MultiRecordStage>();
                stageApprovalsList = service.Top<Stage>(0, x => x.By == empID && x.IsDeleted == false && (x.ApprovalStatus == 0 || x.ApprovalStatus == 3)).ToList();
                multiRecordApprovalList = service.Top<MultiRecordStage>(0, x => x.By == empID && x.IsDeleted == false && (x.ApprovalStatus == 0 || x.ApprovalStatus == 3)).ToList();
                var modulesList = multiRecordApprovalList.GroupBy(x => x.ModuleID).Select(x => x.Key).ToList();
                viewModel.EmployeeApprovalViewModelList = new List<IEmployeeApprovalViewModel>();
                Func<ApprovalDetail, ApprovalHistory> getHistory = SetApprovalHistory;
                foreach (var stageItem in stageApprovalsList)
                {
                    if (stageItem.ModuleID == 2)
                    {
                        var oldAddressViewModel = new List<EmployeeAddress>();
                        var newAddressViewModel = new List<EmployeeAddress>();
                        var newEntry = JsonConvert.DeserializeObject<EmployeeAddress>(stageItem.NewEntry, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
                        var oldEntry = JsonConvert.DeserializeObject<EmployeeAddress>(stageItem.PreviousEntry, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
                        oldAddressViewModel.Add(oldEntry);
                        newAddressViewModel.Add(newEntry);
                        stageItem.NewEntry = JsonConvert.SerializeObject(newAddressViewModel, Formatting.None, new IsoDateTimeConverter() { DateTimeFormat = "MM/dd/yyyy" });
                        stageItem.PreviousEntry = JsonConvert.SerializeObject(oldAddressViewModel, Formatting.None, new IsoDateTimeConverter() { DateTimeFormat = "MM/dd/yyyy" });
                    }
                    var mappedItem = Mapper.Map<Stage, EmployeeApprovalViewModel>(stageItem);
                    var requestId = stageApprovalsList.Where(n => n.ModuleID == 1).First().RecordID.Value;
                    mappedItem.approvalHistory = service.Top<ApprovalDetail>(0, x => x.Approval.RequestID == requestId && x.IsDeleted == true).OrderByDescending(x => x.ApprovalDate).Select(m => getHistory(m)).ToList();
                    viewModel.EmployeeApprovalViewModelList.Add(mappedItem);
                }
                foreach (int multiRecordItem in modulesList)
                {
                    int requestId = 0;
                    List<ApprovalHistory> approvalHistory = new System.Collections.Generic.List<ApprovalHistory>();
                    dynamic oldModelObject;

                    switch (multiRecordItem)
                    {
                        case 2:
                            oldModelObject = service.Top<PersonAddress>(0, x => x.Person.ID == empID && x.IsDeleted == false);
                            var oldAddressViewModel = Mapper.Map<IEnumerable<PersonAddress>, List<EmployeeAddress>>(oldModelObject);
                            var newAddressViewModel = new List<EmployeeAddress>(oldAddressViewModel);
                            requestId = multiRecordApprovalList.Where(n => n.ModuleID == 2).First().RecordID.Value;
                            approvalHistory = service.Top<ApprovalDetail>(0, x => x.Approval.RequestID == requestId && x.IsDeleted == true).OrderByDescending(x => x.ApprovalDate).Select(m => getHistory(m)).ToList();
                            viewModel.EmployeeApprovalViewModelList.Add(new EmployeeApprovalViewModel
                            {
                                ModuleID = multiRecordItem,
                                ModuleCode = multiRecordApprovalList.Where(x => x.ModuleID == 2).First().ModuleCode,
                                OldModel = JsonConvert.SerializeObject(oldAddressViewModel),
                                StageID = "mrstg_" + multiRecordApprovalList.Where(x => x.ModuleID == 2).First().ID.ToString(),
                                ApprovalStatus = multiRecordApprovalList.Where(x => x.ModuleID == 2).First().ApprovalStatus,
                                RecordID = multiRecordApprovalList.Where(x => x.ModuleID == 2).First().RecordID.Value,
                                approvalHistory = approvalHistory

                            });
                            viewModel.EmployeeApprovalViewModelList.Where(x => x.ModuleID == multiRecordItem).First().OldModel = JsonConvert.SerializeObject(oldAddressViewModel, Formatting.None, new IsoDateTimeConverter() { DateTimeFormat = "MM/dd/yyyy" });
                            foreach (var item in multiRecordApprovalList.Where(x => x.ModuleID == 2))
                            {
                                var newObject = JsonConvert.DeserializeObject<EmployeeAddress>(item.NewEntry, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
                                if (item.RecordID == 0)
                                {
                                    newAddressViewModel.Add(newObject);
                                }
                                else
                                {
                                    if (item.StatusID == 2)
                                    {
                                        newAddressViewModel.Where(x => x.ID == item.RecordID).FirstOrDefault().CurrentAddress = newObject.CurrentAddress;
                                        newAddressViewModel.Where(x => x.ID == item.RecordID).FirstOrDefault().CurrentAddressCountry = newObject.CurrentAddressCountry;
                                        newAddressViewModel.Where(x => x.ID == item.RecordID).FirstOrDefault().City = newObject.City;
                                        newAddressViewModel.Where(x => x.ID == item.RecordID).FirstOrDefault().State = newObject.State;
                                        newAddressViewModel.Where(x => x.ID == item.RecordID).FirstOrDefault().Pin = newObject.Pin;
                                        newAddressViewModel.Where(x => x.ID == item.RecordID).FirstOrDefault().ApprovalID = newObject.ApprovalID;
                                    }
                                    else
                                    {
                                    }
                                }
                            }
                            viewModel.EmployeeApprovalViewModelList.Where(x => x.ModuleID == multiRecordItem).First().NewModel = JsonConvert.SerializeObject(newAddressViewModel, Formatting.None, new IsoDateTimeConverter() { DateTimeFormat = "MM/dd/yyyy" });
                            break;

                        case 3:
                            oldModelObject = service.Top<PersonQualificationMapping>(0, x => x.Person.ID == empID && x.IsDeleted == false);
                            var oldQualificationViewModel = Mapper.Map<IEnumerable<PersonQualificationMapping>, List<EmployeeQualification>>(oldModelObject);
                            var newQualificationViewModel = new List<EmployeeQualification>(oldQualificationViewModel);
                            requestId = multiRecordApprovalList.Where(n => n.ModuleID == 3).First().RecordID.Value;
                            approvalHistory = service.Top<ApprovalDetail>(0, x => x.Approval.RequestID == requestId && x.IsDeleted == true).OrderByDescending(x => x.ApprovalDate).Select(m => getHistory(m)).ToList();
                            viewModel.EmployeeApprovalViewModelList.Add(new EmployeeApprovalViewModel
                            {
                                ModuleID = multiRecordItem,
                                ModuleCode = multiRecordApprovalList.Where(x => x.ModuleID == 3).First().ModuleCode,
                                OldModel = JsonConvert.SerializeObject(oldQualificationViewModel),
                                StageID = "mrstg_" + multiRecordApprovalList.Where(x => x.ModuleID == 3).First().ID.ToString(),
                                ApprovalStatus = multiRecordApprovalList.Where(x => x.ModuleID == 3).First().ApprovalStatus,
                                RecordID = multiRecordApprovalList.Where(x => x.ModuleID == 3).First().RecordID.Value,
                                approvalHistory = approvalHistory
                            });
                            viewModel.EmployeeApprovalViewModelList.Where(x => x.ModuleID == multiRecordItem).First().OldModel = JsonConvert.SerializeObject(oldQualificationViewModel, Formatting.None, new IsoDateTimeConverter() { DateTimeFormat = "MM/dd/yyyy" });
                            foreach (var item in multiRecordApprovalList.Where(x => x.ModuleID == 3))
                            {
                                var newObject = JsonConvert.DeserializeObject<EmployeeQualification>(item.NewEntry, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
                                if (item.RecordID == 0)
                                {
                                    newQualificationViewModel.Add(newObject);
                                }
                                else
                                {
                                    if (item.StatusID == 2)
                                    {
                                        newQualificationViewModel.Where(x => x.ID == item.RecordID).FirstOrDefault().Grade_Class = newObject.Grade_Class;
                                        newQualificationViewModel.Where(x => x.ID == item.RecordID).FirstOrDefault().Institute = newObject.Institute;
                                        newQualificationViewModel.Where(x => x.ID == item.RecordID).FirstOrDefault().PassingYear = newObject.PassingYear;
                                        newQualificationViewModel.Where(x => x.ID == item.RecordID).FirstOrDefault().QualificationType = newObject.QualificationType;
                                        newQualificationViewModel.Where(x => x.ID == item.RecordID).FirstOrDefault().Specialization = newObject.Specialization;
                                        newQualificationViewModel.Where(x => x.ID == item.RecordID).FirstOrDefault().University = newObject.University;
                                        newQualificationViewModel.Where(x => x.ID == item.RecordID).FirstOrDefault().ApprovalID = newObject.ApprovalID;
                                    }
                                    else
                                    {
                                        newQualificationViewModel.Where(x => x.ID == item.RecordID).FirstOrDefault().Grade_Class = newObject.Grade_Class;
                                        newQualificationViewModel.Where(x => x.ID == item.RecordID).FirstOrDefault().Institute = newObject.Institute;
                                        newQualificationViewModel.Where(x => x.ID == item.RecordID).FirstOrDefault().PassingYear = newObject.PassingYear;
                                        newQualificationViewModel.Where(x => x.ID == item.RecordID).FirstOrDefault().QualificationType = newObject.QualificationType;
                                        newQualificationViewModel.Where(x => x.ID == item.RecordID).FirstOrDefault().Specialization = newObject.Specialization;
                                        newQualificationViewModel.Where(x => x.ID == item.RecordID).FirstOrDefault().University = newObject.University;
                                        newQualificationViewModel.Where(x => x.ID == item.RecordID).FirstOrDefault().ApprovalID = newObject.ApprovalID;
                                        newQualificationViewModel.Where(x => x.ID == item.RecordID).FirstOrDefault().IsDeleted = true;
                                    }
                                }
                            }
                            viewModel.EmployeeApprovalViewModelList.Where(x => x.ModuleID == multiRecordItem).First().NewModel = JsonConvert.SerializeObject(newQualificationViewModel, Formatting.None, new IsoDateTimeConverter() { DateTimeFormat = "MM/dd/yyyy" });
                            break;

                        case 4:
                            oldModelObject = service.Top<PersonPassport>(1, x => x.PersonID == empID && x.RelationWithPPHolder == 1 && x.IsDeleted == false);
                            var oldPassportViewModel = Mapper.Map<IEnumerable<PersonPassport>, List<EmployeePassportViewModel>>(oldModelObject);
                            var newPassportViewModel = new List<EmployeePassportViewModel>(oldPassportViewModel);
                            requestId = multiRecordApprovalList.Where(n => n.ModuleID == 4).First().RecordID.Value;
                            approvalHistory = service.Top<ApprovalDetail>(0, x => x.Approval.RequestID == requestId && x.IsDeleted == true).OrderByDescending(x => x.ApprovalDate).Select(m => getHistory(m)).ToList();
                            viewModel.EmployeeApprovalViewModelList.Add(new EmployeeApprovalViewModel
                            {
                                ModuleID = multiRecordItem,
                                ModuleCode = multiRecordApprovalList.Where(x => x.ModuleID == 4).First().ModuleCode,
                                OldModel = JsonConvert.SerializeObject(oldPassportViewModel),
                                StageID = "mrstg_" + multiRecordApprovalList.Where(x => x.ModuleID == 4).First().ID.ToString(),
                                ApprovalStatus = multiRecordApprovalList.Where(x => x.ModuleID == 4).First().ApprovalStatus,
                                RecordID = multiRecordApprovalList.Where(x => x.ModuleID == 4).First().RecordID.Value,
                                approvalHistory = approvalHistory
                            });
                            viewModel.EmployeeApprovalViewModelList.Where(x => x.ModuleID == multiRecordItem).First().OldModel = JsonConvert.SerializeObject(oldPassportViewModel, Formatting.None, new IsoDateTimeConverter() { DateTimeFormat = "MM/dd/yyyy" });
                            foreach (var item in multiRecordApprovalList.Where(x => x.ModuleID == 4))
                            {
                                var newObject = JsonConvert.DeserializeObject<EmployeePassportViewModel>(item.NewEntry, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
                                if (item.RecordID == 0)
                                {
                                    newPassportViewModel.Add(newObject);
                                }
                                else
                                {
                                    if (item.StatusID == 2)
                                    {
                                        newPassportViewModel.Where(x => x.ID == item.RecordID).FirstOrDefault().NameAsInPassport = newObject.NameAsInPassport;
                                        newPassportViewModel.Where(x => x.ID == item.RecordID).FirstOrDefault().PlaceIssued = newObject.PlaceIssued;
                                        newPassportViewModel.Where(x => x.ID == item.RecordID).FirstOrDefault().BlankPagesLeft = newObject.BlankPagesLeft;
                                        newPassportViewModel.Where(x => x.ID == item.RecordID).FirstOrDefault().DateOfExpiry = newObject.DateOfExpiry;
                                        newPassportViewModel.Where(x => x.ID == item.RecordID).FirstOrDefault().DateOfIssue = newObject.DateOfIssue;
                                        newPassportViewModel.Where(x => x.ID == item.RecordID).FirstOrDefault().PassportNumber = newObject.PassportNumber;
                                    }
                                    else
                                    {
                                        newPassportViewModel.Where(x => x.ID == item.RecordID).FirstOrDefault().NameAsInPassport = newObject.NameAsInPassport;
                                        newPassportViewModel.Where(x => x.ID == item.RecordID).FirstOrDefault().PlaceIssued = newObject.PlaceIssued;
                                        newPassportViewModel.Where(x => x.ID == item.RecordID).FirstOrDefault().BlankPagesLeft = newObject.BlankPagesLeft;
                                        newPassportViewModel.Where(x => x.ID == item.RecordID).FirstOrDefault().DateOfExpiry = newObject.DateOfExpiry;
                                        newPassportViewModel.Where(x => x.ID == item.RecordID).FirstOrDefault().DateOfIssue = newObject.DateOfIssue;
                                        newPassportViewModel.Where(x => x.ID == item.RecordID).FirstOrDefault().PassportNumber = newObject.PassportNumber;
                                        newPassportViewModel.Where(x => x.ID == item.RecordID).FirstOrDefault().IsDeleted = true;
                                    }
                                }
                            }
                            viewModel.EmployeeApprovalViewModelList.Where(x => x.ModuleID == multiRecordItem).First().NewModel = JsonConvert.SerializeObject(newPassportViewModel, Formatting.None, new IsoDateTimeConverter() { DateTimeFormat = "MM/dd/yyyy" });
                            break;

                        case 5:
                            oldModelObject = service.Top<PersonContact>(0, x => x.Person.ID == empID && x.IsDeleted == false);
                            var oldContactViewModel = Mapper.Map<IEnumerable<PersonContact>, List<EmployeeEmergencyContact>>(oldModelObject);
                            var newContactViewModel = new List<EmployeeEmergencyContact>(oldContactViewModel);
                            requestId = multiRecordApprovalList.Where(n => n.ModuleID == 5).First().RecordID.Value;
                            approvalHistory = service.Top<ApprovalDetail>(0, x => x.Approval.RequestID == requestId && x.IsDeleted == true).OrderByDescending(x => x.ApprovalDate).Select(m => getHistory(m)).ToList();
                            viewModel.EmployeeApprovalViewModelList.Add(new EmployeeApprovalViewModel
                            {
                                ModuleID = multiRecordItem,
                                ModuleCode = multiRecordApprovalList.Where(x => x.ModuleID == 5).First().ModuleCode,
                                OldModel = JsonConvert.SerializeObject(oldContactViewModel),
                                StageID = "mrstg_" + multiRecordApprovalList.Where(x => x.ModuleID == 5).First().ID.ToString(),
                                ApprovalStatus = multiRecordApprovalList.Where(x => x.ModuleID == 5).First().ApprovalStatus,
                                RecordID = multiRecordApprovalList.Where(x => x.ModuleID == 5).First().RecordID.Value,
                                approvalHistory = approvalHistory
                            });
                            viewModel.EmployeeApprovalViewModelList.Where(x => x.ModuleID == multiRecordItem).First().OldModel = JsonConvert.SerializeObject(oldContactViewModel, Formatting.None, new IsoDateTimeConverter() { DateTimeFormat = "MM/dd/yyyy" });
                            foreach (var item in multiRecordApprovalList.Where(x => x.ModuleID == 5))
                            {
                                var newObject = JsonConvert.DeserializeObject<EmployeeEmergencyContact>(item.NewEntry, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
                                if (item.RecordID == 0)
                                {
                                    newContactViewModel.Add(newObject);
                                }
                                else
                                {
                                    if (item.StatusID == 2)
                                    {
                                        newContactViewModel.Where(x => x.ID == item.RecordID).FirstOrDefault().ContactPersonName = newObject.ContactPersonName;
                                        newContactViewModel.Where(x => x.ID == item.RecordID).FirstOrDefault().EmergencyContactNo = newObject.EmergencyContactNo;
                                        newContactViewModel.Where(x => x.ID == item.RecordID).FirstOrDefault().EmergencyEmail = newObject.EmergencyEmail;
                                        newContactViewModel.Where(x => x.ID == item.RecordID).FirstOrDefault().Relation = newObject.Relation;
                                        newContactViewModel.Where(x => x.ID == item.RecordID).FirstOrDefault().ContactAddress = newObject.ContactAddress;
                                        newContactViewModel.Where(x => x.ID == item.RecordID).FirstOrDefault().ApprovalID = newObject.ApprovalID;
                                    }
                                    else
                                    {
                                        newContactViewModel.Where(x => x.ID == item.RecordID).FirstOrDefault().ContactPersonName = newObject.ContactPersonName;
                                        newContactViewModel.Where(x => x.ID == item.RecordID).FirstOrDefault().EmergencyContactNo = newObject.EmergencyContactNo;
                                        newContactViewModel.Where(x => x.ID == item.RecordID).FirstOrDefault().EmergencyEmail = newObject.EmergencyEmail;
                                        newContactViewModel.Where(x => x.ID == item.RecordID).FirstOrDefault().Relation = newObject.Relation;
                                        newContactViewModel.Where(x => x.ID == item.RecordID).FirstOrDefault().ContactAddress = newObject.ContactAddress;
                                        newContactViewModel.Where(x => x.ID == item.RecordID).FirstOrDefault().ApprovalID = newObject.ApprovalID;
                                        newContactViewModel.Where(x => x.ID == item.RecordID).FirstOrDefault().IsDeleted = true;
                                    }
                                }
                            }
                            viewModel.EmployeeApprovalViewModelList.Where(x => x.ModuleID == multiRecordItem).First().NewModel = JsonConvert.SerializeObject(newContactViewModel, Formatting.None, new IsoDateTimeConverter() { DateTimeFormat = "MM/dd/yyyy" });
                            break;

                        case 6:
                            oldModelObject = service.Top<PersonCertification>(0, x => x.Person.ID == empID && x.IsDeleted == false);
                            var oldCertificationViewModel = Mapper.Map<IEnumerable<PersonCertification>, List<EmployeeCertification>>(oldModelObject);
                            var newCertificationViewModel = new List<EmployeeCertification>(oldCertificationViewModel);
                            requestId = multiRecordApprovalList.Where(n => n.ModuleID == 6).First().RecordID.Value;
                            approvalHistory = service.Top<ApprovalDetail>(0, x => x.Approval.RequestID == requestId && x.IsDeleted == true).OrderByDescending(x => x.ApprovalDate).Select(m => getHistory(m)).ToList();
                            viewModel.EmployeeApprovalViewModelList.Add(new EmployeeApprovalViewModel
                            {
                                ModuleID = multiRecordItem,
                                ModuleCode = multiRecordApprovalList.Where(x => x.ModuleID == 6).First().ModuleCode,
                                OldModel = JsonConvert.SerializeObject(oldCertificationViewModel),
                                StageID = "mrstg_" + multiRecordApprovalList.Where(x => x.ModuleID == 6).First().ID.ToString(),
                                ApprovalStatus = multiRecordApprovalList.Where(x => x.ModuleID == 6).First().ApprovalStatus,
                                RecordID = multiRecordApprovalList.Where(x => x.ModuleID == 6).First().RecordID.Value,
                                approvalHistory = approvalHistory
                            });
                            viewModel.EmployeeApprovalViewModelList.Where(x => x.ModuleID == multiRecordItem).First().OldModel = JsonConvert.SerializeObject(oldCertificationViewModel, Formatting.None, new IsoDateTimeConverter() { DateTimeFormat = "MM/dd/yyyy" });
                            foreach (var item in multiRecordApprovalList.Where(x => x.ModuleID == 6))
                            {
                                var newObject = JsonConvert.DeserializeObject<EmployeeCertification>(item.NewEntry, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
                                if (item.RecordID == 0)
                                {
                                    newCertificationViewModel.Add(newObject);
                                }
                                else
                                {
                                    if (item.StatusID == 2)
                                    {
                                        newCertificationViewModel.Where(x => x.ID == item.RecordID).FirstOrDefault().CertificationDate = newObject.CertificationDate;
                                        newCertificationViewModel.Where(x => x.ID == item.RecordID).FirstOrDefault().Grade = newObject.Grade;
                                        newCertificationViewModel.Where(x => x.ID == item.RecordID).FirstOrDefault().CertificationNumber = newObject.CertificationNumber;
                                        newCertificationViewModel.Where(x => x.ID == item.RecordID).FirstOrDefault().ApprovalID = newObject.ApprovalID;
                                    }
                                    else
                                    {
                                        newCertificationViewModel.Where(x => x.ID == item.RecordID).FirstOrDefault().CertificationDate = newObject.CertificationDate;
                                        newCertificationViewModel.Where(x => x.ID == item.RecordID).FirstOrDefault().Grade = newObject.Grade;
                                        newCertificationViewModel.Where(x => x.ID == item.RecordID).FirstOrDefault().CertificationNumber = newObject.CertificationNumber;
                                        newCertificationViewModel.Where(x => x.ID == item.RecordID).FirstOrDefault().ApprovalID = newObject.ApprovalID;
                                        newCertificationViewModel.Where(x => x.ID == item.RecordID).FirstOrDefault().IsDeleted = true;
                                    }
                                }
                            }
                            viewModel.EmployeeApprovalViewModelList.Where(x => x.ModuleID == multiRecordItem).First().NewModel = JsonConvert.SerializeObject(newCertificationViewModel, Formatting.None, new IsoDateTimeConverter() { DateTimeFormat = "MM/dd/yyyy" });
                            break;

                        case 7:
                            oldModelObject = service.Top<PersonEmploymentHistory>(0, x => x.Person.ID == empID && x.IsDeleted == false);
                            var oldEmpHistViewModel = Mapper.Map<IEnumerable<PersonEmploymentHistory>, List<EmployeeEmploymentHistory>>(oldModelObject);
                            var newEmpHistViewModel = new List<EmployeeEmploymentHistory>(oldEmpHistViewModel);
                            requestId = multiRecordApprovalList.Where(n => n.ModuleID == 7).First().RecordID.Value;
                            approvalHistory = service.Top<ApprovalDetail>(0, x => x.Approval.RequestID == requestId && x.IsDeleted == true).OrderByDescending(x => x.ApprovalDate).Select(m => getHistory(m)).ToList();
                            viewModel.EmployeeApprovalViewModelList.Add(new EmployeeApprovalViewModel
                            {
                                ModuleID = multiRecordItem,
                                ModuleCode = multiRecordApprovalList.Where(x => x.ModuleID == 7).First().ModuleCode,
                                OldModel = JsonConvert.SerializeObject(oldEmpHistViewModel),
                                StageID = "mrstg_" + multiRecordApprovalList.Where(x => x.ModuleID == 7).First().ID.ToString(),
                                ApprovalStatus = multiRecordApprovalList.Where(x => x.ModuleID == 7).First().ApprovalStatus,
                                RecordID = multiRecordApprovalList.Where(x => x.ModuleID == 7).First().RecordID.Value,
                                approvalHistory = approvalHistory
                            });
                            viewModel.EmployeeApprovalViewModelList.Where(x => x.ModuleID == multiRecordItem).First().OldModel = JsonConvert.SerializeObject(oldEmpHistViewModel, Formatting.None, new IsoDateTimeConverter() { DateTimeFormat = "MM/dd/yyyy" });
                            foreach (var item in multiRecordApprovalList.Where(x => x.ModuleID == 7))
                            {
                                var newObject = JsonConvert.DeserializeObject<EmployeeEmploymentHistory>(item.NewEntry, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
                                if (item.RecordID == 0)
                                {
                                    newEmpHistViewModel.Add(newObject);
                                }
                                else
                                {
                                    if (item.StatusID == 2)
                                    {
                                        newEmpHistViewModel.Where(x => x.ID == item.RecordID).FirstOrDefault().OrganisationName = newObject.OrganisationName;
                                        newEmpHistViewModel.Where(x => x.ID == item.RecordID).FirstOrDefault().Location = newObject.Location;
                                        newEmpHistViewModel.Where(x => x.ID == item.RecordID).FirstOrDefault().LastDesignation = newObject.LastDesignation;
                                        newEmpHistViewModel.Where(x => x.ID == item.RecordID).FirstOrDefault().JoiningDate = newObject.JoiningDate;
                                        newEmpHistViewModel.Where(x => x.ID == item.RecordID).FirstOrDefault().WorkedTill = newObject.WorkedTill;
                                    }
                                    else
                                    {
                                        newEmpHistViewModel.Where(x => x.ID == item.RecordID).FirstOrDefault().OrganisationName = newObject.OrganisationName;
                                        newEmpHistViewModel.Where(x => x.ID == item.RecordID).FirstOrDefault().Location = newObject.Location;
                                        newEmpHistViewModel.Where(x => x.ID == item.RecordID).FirstOrDefault().LastDesignation = newObject.LastDesignation;
                                        newEmpHistViewModel.Where(x => x.ID == item.RecordID).FirstOrDefault().JoiningDate = newObject.JoiningDate;
                                        newEmpHistViewModel.Where(x => x.ID == item.RecordID).FirstOrDefault().WorkedTill = newObject.WorkedTill;
                                        newEmpHistViewModel.Where(x => x.ID == item.RecordID).FirstOrDefault().IsDeleted = true;
                                    }
                                }
                            }

                            viewModel.EmployeeApprovalViewModelList.Where(x => x.ModuleID == multiRecordItem).First().NewModel = JsonConvert.SerializeObject(newEmpHistViewModel, Formatting.None, new IsoDateTimeConverter() { DateTimeFormat = "MM/dd/yyyy" });
                            break;

                        case 8:
                            oldModelObject = service.Top<PersonVisa>(0, x => x.Person.ID == empID && x.IsDeleted == false);
                            var oldVisaViewModel = Mapper.Map<IEnumerable<PersonVisa>, List<EmployeeVisa>>(oldModelObject);
                            var newVisaViewModel = new List<EmployeeVisa>(oldVisaViewModel);
                            requestId = multiRecordApprovalList.Where(n => n.ModuleID == 8).First().RecordID.Value;
                            approvalHistory = service.Top<ApprovalDetail>(0, x => x.Approval.RequestID == requestId && x.IsDeleted == true).OrderByDescending(x => x.ApprovalDate).Select(m => getHistory(m)).ToList();
                            viewModel.EmployeeApprovalViewModelList.Add(new EmployeeApprovalViewModel
                            {
                                ModuleID = multiRecordItem,
                                ModuleCode = multiRecordApprovalList.Where(x => x.ModuleID == 8).First().ModuleCode,
                                OldModel = JsonConvert.SerializeObject(oldVisaViewModel),
                                StageID = "mrstg_" + multiRecordApprovalList.Where(x => x.ModuleID == 8).First().ID.ToString(),
                                ApprovalStatus = multiRecordApprovalList.Where(x => x.ModuleID == 8).First().ApprovalStatus,
                                RecordID = multiRecordApprovalList.Where(x => x.ModuleID == 8).First().RecordID.Value,
                                approvalHistory = approvalHistory
                            });
                            viewModel.EmployeeApprovalViewModelList.Where(x => x.ModuleID == multiRecordItem).First().OldModel = JsonConvert.SerializeObject(oldVisaViewModel, Formatting.None, new IsoDateTimeConverter() { DateTimeFormat = "MM/dd/yyyy" });
                            foreach (var item in multiRecordApprovalList.Where(x => x.ModuleID == 8))
                            {
                                var newObject = JsonConvert.DeserializeObject<EmployeeVisa>(item.NewEntry, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
                                if (item.RecordID == 0)
                                {
                                    newVisaViewModel.Add(newObject);
                                }
                                else
                                {
                                    if (item.StatusID == 2)
                                    {
                                        newVisaViewModel.Where(x => x.ID == item.RecordID).FirstOrDefault().ValidTill = newObject.ValidTill;
                                    }
                                    else
                                    {
                                        newVisaViewModel.Where(x => x.ID == item.RecordID).FirstOrDefault().ValidTill = newObject.ValidTill;
                                        newVisaViewModel.Where(x => x.ID == item.RecordID).FirstOrDefault().IsDeleted = true;
                                    }
                                }
                            }
                            viewModel.EmployeeApprovalViewModelList.Where(x => x.ModuleID == multiRecordItem).First().NewModel = JsonConvert.SerializeObject(newVisaViewModel, Formatting.None, new IsoDateTimeConverter() { DateTimeFormat = "MM/dd/yyyy" });
                            break;

                        case 10:
                            oldModelObject = service.Top<PersonMedicalHistory>(0, x => x.Person.ID == empID && x.IsDeleted == false);
                            var oldMedHistViewModel = Mapper.Map<IEnumerable<PersonMedicalHistory>, List<EmployeeMedicalHistory>>(oldModelObject);
                            var newMedHistViewModel = new List<EmployeeMedicalHistory>(oldMedHistViewModel);
                            requestId = multiRecordApprovalList.Where(n => n.ModuleID == 10).First().RecordID.Value;
                            approvalHistory = service.Top<ApprovalDetail>(0, x => x.Approval.RequestID == requestId && x.IsDeleted == true).OrderByDescending(x => x.ApprovalDate).Select(m => getHistory(m)).ToList();
                            viewModel.EmployeeApprovalViewModelList.Add(new EmployeeApprovalViewModel
                            {
                                ModuleID = multiRecordItem,
                                ModuleCode = multiRecordApprovalList.Where(x => x.ModuleID == 10).First().ModuleCode,
                                OldModel = JsonConvert.SerializeObject(oldMedHistViewModel),
                                StageID = "mrstg_" + multiRecordApprovalList.Where(x => x.ModuleID == 10).First().ID.ToString(),
                                ApprovalStatus = multiRecordApprovalList.Where(x => x.ModuleID == 10).First().ApprovalStatus,
                                RecordID = multiRecordApprovalList.Where(x => x.ModuleID == 10).First().RecordID.Value,
                                approvalHistory = approvalHistory
                            });
                            viewModel.EmployeeApprovalViewModelList.Where(x => x.ModuleID == multiRecordItem).First().OldModel = JsonConvert.SerializeObject(oldMedHistViewModel, Formatting.None, new IsoDateTimeConverter() { DateTimeFormat = "MM/dd/yyyy" });
                            foreach (var item in multiRecordApprovalList.Where(x => x.ModuleID == 10))
                            {
                                var newObject = JsonConvert.DeserializeObject<EmployeeMedicalHistory>(item.NewEntry, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
                                if (item.RecordID == 0)
                                {
                                    newMedHistViewModel.Add(newObject);
                                }
                                else
                                {
                                    if (item.StatusID == 2)
                                    {
                                        newMedHistViewModel.Where(x => x.ID == item.RecordID).FirstOrDefault().Description = newObject.Description;
                                        newMedHistViewModel.Where(x => x.ID == item.RecordID).FirstOrDefault().Year = newObject.Year;
                                        newMedHistViewModel.Where(x => x.ID == item.RecordID).FirstOrDefault().ApprovalID = newObject.ApprovalID;
                                    }
                                    else
                                    {
                                        newMedHistViewModel.Where(x => x.ID == item.RecordID).FirstOrDefault().Description = newObject.Description;
                                        newMedHistViewModel.Where(x => x.ID == item.RecordID).FirstOrDefault().Year = newObject.Year;
                                        newMedHistViewModel.Where(x => x.ID == item.RecordID).FirstOrDefault().ApprovalID = newObject.ApprovalID;
                                        newMedHistViewModel.Where(x => x.ID == item.RecordID).FirstOrDefault().IsDeleted = true;
                                    }
                                }
                            }
                            viewModel.EmployeeApprovalViewModelList.Where(x => x.ModuleID == multiRecordItem).First().NewModel = JsonConvert.SerializeObject(newMedHistViewModel, Formatting.None, new IsoDateTimeConverter() { DateTimeFormat = "MM/dd/yyyy" });
                            break;

                        case 12:
                            oldModelObject = service.Top<PersonEmployment>(0, x => x.Person.ID == empID && x.IsDeleted == false);
                            break;

                        case 16:
                            oldModelObject = service.Top<PersonSkillMapping>(0, x => x.Person.ID == empID && x.IsDeleted == false);
                            var oldSkillsViewModel = Mapper.Map<IEnumerable<PersonSkillMapping>, List<EmployeeSkill>>(oldModelObject);
                            var newSkillsViewModel = new List<EmployeeSkill>(oldSkillsViewModel);
                            requestId = multiRecordApprovalList.Where(n => n.ModuleID == 16).First().RecordID.Value;
                            approvalHistory = service.Top<ApprovalDetail>(0, x => x.Approval.RequestID == requestId && x.IsDeleted == true).OrderByDescending(x => x.ApprovalDate).Select(m => getHistory(m)).ToList();
                            viewModel.EmployeeApprovalViewModelList.Add(new EmployeeApprovalViewModel
                            {
                                ModuleID = multiRecordItem,
                                ModuleCode = multiRecordApprovalList.Where(x => x.ModuleID == 16).First().ModuleCode,
                                OldModel = JsonConvert.SerializeObject(oldSkillsViewModel),
                                StageID = "mrstg_" + multiRecordApprovalList.Where(x => x.ModuleID == 16).First().ID.ToString(),
                                ApprovalStatus = multiRecordApprovalList.Where(x => x.ModuleID == 16).First().ApprovalStatus,
                                RecordID = multiRecordApprovalList.Where(x => x.ModuleID == 16).First().RecordID.Value,
                                approvalHistory = approvalHistory
                            });
                            viewModel.EmployeeApprovalViewModelList.Where(x => x.ModuleID == multiRecordItem).First().OldModel = JsonConvert.SerializeObject(oldSkillsViewModel, Formatting.None, new IsoDateTimeConverter() { DateTimeFormat = "MM/dd/yyyy" });
                            foreach (var item in multiRecordApprovalList.Where(x => x.ModuleID == 16))
                            {
                                var newObject = JsonConvert.DeserializeObject<EmployeeSkill>(item.NewEntry, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
                                if (item.RecordID == 0)
                                {
                                    newSkillsViewModel.Add(newObject);
                                }
                                else
                                {
                                    if (item.StatusID == 2)
                                    {
                                        newSkillsViewModel.Where(x => x.ID == item.RecordID).FirstOrDefault().ExperienceMonths = newObject.ExperienceMonths;
                                        newSkillsViewModel.Where(x => x.ID == item.RecordID).FirstOrDefault().ExperienceYears = newObject.ExperienceYears;
                                        newSkillsViewModel.Where(x => x.ID == item.RecordID).FirstOrDefault().HasCoreCompetency = newObject.HasCoreCompetency;
                                        newSkillsViewModel.Where(x => x.ID == item.RecordID).FirstOrDefault().SkillRating = newObject.SkillRating;
                                    }
                                    else
                                    {
                                        newSkillsViewModel.Where(x => x.ID == item.RecordID).FirstOrDefault().ExperienceMonths = newObject.ExperienceMonths;
                                        newSkillsViewModel.Where(x => x.ID == item.RecordID).FirstOrDefault().ExperienceYears = newObject.ExperienceYears;
                                        newSkillsViewModel.Where(x => x.ID == item.RecordID).FirstOrDefault().HasCoreCompetency = newObject.HasCoreCompetency;
                                        newSkillsViewModel.Where(x => x.ID == item.RecordID).FirstOrDefault().SkillRating = newObject.SkillRating;
                                        newSkillsViewModel.Where(x => x.ID == item.RecordID).FirstOrDefault().IsDeleted = true;
                                    }
                                }
                            }
                            viewModel.EmployeeApprovalViewModelList.Where(x => x.ModuleID == multiRecordItem).First().NewModel = JsonConvert.SerializeObject(newSkillsViewModel, Formatting.None, new IsoDateTimeConverter() { DateTimeFormat = "MM/dd/yyyy" });
                            break;

                        case 17:
                            oldModelObject = service.Top<PersonDependent>(0, x => x.Person.ID == empID && x.IsDeleted == false);
                            var oldDependentViewModel = Mapper.Map<IEnumerable<PersonDependent>, List<EmployeeDependent>>(oldModelObject);
                            var newDependentViewModel = new List<EmployeeDependent>(oldDependentViewModel);
                            requestId = multiRecordApprovalList.Where(n => n.ModuleID == 17).First().RecordID.Value;
                            approvalHistory = service.Top<ApprovalDetail>(0, x => x.Approval.RequestID == requestId && x.IsDeleted == true).OrderByDescending(x => x.ApprovalDate).Select(m => getHistory(m)).ToList();
                            viewModel.EmployeeApprovalViewModelList.Add(new EmployeeApprovalViewModel
                            {
                                ModuleID = multiRecordItem,
                                ModuleCode = multiRecordApprovalList.Where(x => x.ModuleID == 17).First().ModuleCode,
                                OldModel = JsonConvert.SerializeObject(oldDependentViewModel),
                                StageID = "mrstg_" + multiRecordApprovalList.Where(x => x.ModuleID == 17).First().ID.ToString(),
                                ApprovalStatus = multiRecordApprovalList.Where(x => x.ModuleID == 17).First().ApprovalStatus,
                                RecordID = multiRecordApprovalList.Where(x => x.ModuleID == 17).First().RecordID.Value,
                                approvalHistory = approvalHistory
                            });
                            viewModel.EmployeeApprovalViewModelList.Where(x => x.ModuleID == multiRecordItem).First().OldModel = JsonConvert.SerializeObject(oldDependentViewModel, Formatting.None, new IsoDateTimeConverter() { DateTimeFormat = "MM/dd/yyyy" });
                            foreach (var item in multiRecordApprovalList.Where(x => x.ModuleID == 17))
                            {
                                var newObject = JsonConvert.DeserializeObject<EmployeeDependent>(item.NewEntry, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
                                if (item.RecordID == 0)
                                {
                                    newDependentViewModel.Add(newObject);
                                }
                                else
                                {
                                    if (item.StatusID == 2)
                                    {
                                        newDependentViewModel.Where(x => x.ID == item.RecordID).FirstOrDefault().DependentName = newObject.DependentName;
                                        newDependentViewModel.Where(x => x.ID == item.RecordID).FirstOrDefault().DateOfBirthOfDependent = newObject.DateOfBirthOfDependent;
                                        newDependentViewModel.Where(x => x.ID == item.RecordID).FirstOrDefault().RelationWithDependent = newObject.RelationWithDependent;
                                    }
                                    else
                                    {
                                        newDependentViewModel.Where(x => x.ID == item.RecordID).FirstOrDefault().DependentName = newObject.DependentName;
                                        newDependentViewModel.Where(x => x.ID == item.RecordID).FirstOrDefault().DateOfBirthOfDependent = newObject.DateOfBirthOfDependent;
                                        newDependentViewModel.Where(x => x.ID == item.RecordID).FirstOrDefault().RelationWithDependent = newObject.RelationWithDependent;
                                        newDependentViewModel.Where(x => x.ID == item.RecordID).FirstOrDefault().IsDeleted = true;
                                    }
                                }
                            }
                            viewModel.EmployeeApprovalViewModelList.Where(x => x.ModuleID == multiRecordItem).First().NewModel = JsonConvert.SerializeObject(newDependentViewModel, Formatting.None, new IsoDateTimeConverter() { DateTimeFormat = "MM/dd/yyyy" });
                            break;

                        case 18:
                            oldModelObject = service.Top<PersonDeclaration>(0, x => x.Person.ID == empID && x.IsDeleted == false);
                            var oldDeclarationViewModel = Mapper.Map<IEnumerable<PersonDeclaration>, List<EmployeeDeclaration>>(oldModelObject);
                            var newDeclarationViewModel = new List<EmployeeDeclaration>(oldDeclarationViewModel);
                            requestId = multiRecordApprovalList.Where(n => n.ModuleID == 18).First().RecordID.Value;
                            approvalHistory = service.Top<ApprovalDetail>(0, x => x.Approval.RequestID == requestId && x.IsDeleted == true).OrderByDescending(x => x.ApprovalDate).Select(m => getHistory(m)).ToList();
                            viewModel.EmployeeApprovalViewModelList.Add(new EmployeeApprovalViewModel
                            {
                                ModuleID = multiRecordItem,
                                ModuleCode = multiRecordApprovalList.Where(x => x.ModuleID == 18).First().ModuleCode,
                                OldModel = JsonConvert.SerializeObject(oldDeclarationViewModel),
                                StageID = "mrstg_" + multiRecordApprovalList.Where(x => x.ModuleID == 18).First().ID.ToString(),
                                ApprovalStatus = multiRecordApprovalList.Where(x => x.ModuleID == 18).First().ApprovalStatus,
                                RecordID = multiRecordApprovalList.Where(x => x.ModuleID == 18).First().RecordID.Value,
                                approvalHistory = approvalHistory
                            });
                            viewModel.EmployeeApprovalViewModelList.Where(x => x.ModuleID == multiRecordItem).First().OldModel = JsonConvert.SerializeObject(oldDeclarationViewModel, Formatting.None, new IsoDateTimeConverter() { DateTimeFormat = "MM/dd/yyyy" });
                            foreach (var item in multiRecordApprovalList.Where(x => x.ModuleID == 18))
                            {
                                var newObject = JsonConvert.DeserializeObject<EmployeeDeclaration>(item.NewEntry, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
                                if (item.RecordID == 0)
                                {
                                    newDeclarationViewModel.Add(newObject);
                                }
                                else
                                {
                                    if (item.StatusID == 2)
                                    {
                                        newDeclarationViewModel.Where(x => x.ID == item.RecordID).FirstOrDefault().DeclaredPerson = newObject.DeclaredPerson;
                                        newDeclarationViewModel.Where(x => x.ID == item.RecordID).FirstOrDefault().BirthDate = newObject.BirthDate;
                                        newDeclarationViewModel.Where(x => x.ID == item.RecordID).FirstOrDefault().RelationType = newObject.RelationType;
                                        newDeclarationViewModel.Where(x => x.ID == item.RecordID).FirstOrDefault().V2PersonID = newObject.V2PersonID;
                                    }
                                    else
                                    {
                                        newDeclarationViewModel.Where(x => x.ID == item.RecordID).FirstOrDefault().DeclaredPerson = newObject.DeclaredPerson;
                                        newDeclarationViewModel.Where(x => x.ID == item.RecordID).FirstOrDefault().BirthDate = newObject.BirthDate;
                                        newDeclarationViewModel.Where(x => x.ID == item.RecordID).FirstOrDefault().RelationType = newObject.RelationType;
                                        newDeclarationViewModel.Where(x => x.ID == item.RecordID).FirstOrDefault().V2PersonID = newObject.V2PersonID;
                                        newDeclarationViewModel.Where(x => x.ID == item.RecordID).FirstOrDefault().IsDeleted = true;
                                    }
                                }
                            }
                            viewModel.EmployeeApprovalViewModelList.Where(x => x.ModuleID == multiRecordItem).First().NewModel = JsonConvert.SerializeObject(newDeclarationViewModel, Formatting.None, new IsoDateTimeConverter() { DateTimeFormat = "MM/dd/yyyy" });
                            break;

                        default:
                            break;
                    }
                }
                return viewModel;
            });
        }

        public async Task<EmployeeApprovalViewModel> ApproveEmployeeDetails(string currentUserEmail, string stageID, int moduleID, int empID, int statusID, string comments)
        {
            return await Task.Run(async () =>
            {
                EmployeeApprovalViewModel viewModel = new EmployeeApprovalViewModel();
                int id = Convert.ToInt32(stageID.Substring(stageID.IndexOf('_') + 1));

                if (stageID.Contains("mrstg_"))
                {
                    var oldStageModel = service.Top<MultiRecordStage>(0, x => x.By == empID && x.ModuleID == moduleID && (x.ApprovalStatus == 0 || x.ApprovalStatus == 3));
                    await ApproveMultiRecordStage(currentUserEmail, moduleID, empID, oldStageModel, statusID, comments);
                }
                else
                {
                    var oldStageModel = service.First<Stage>(x => x.ID == id && x.ModuleID == moduleID && (x.ApprovalStatus == 0 || x.ApprovalStatus == 3));
                    await ApproveStageItem(currentUserEmail, moduleID, empID, oldStageModel, statusID, comments);
                }

                return viewModel;
            });
        }

        public async Task<ChangeSet<EmployeePersonalDetails>> UpdateEmployeePersonalAsync(int id, ChangeSet<EmployeePersonalDetails> model)
        {
            return await Task.Run(() =>
            {
                return UpdateEmployeePersonal(id, model);
            });
        }

        public async Task<ChangeSet<EmployeeAddress>> UpdateEmployeeAddressAsync(int id, ChangeSet<EmployeeAddress> model)
        {
            return await Task.Run(() =>
            {
                return UpdateEmployeeAddress(id, model);
            });
        }

        public async Task<ChangeSet<EmployeePassportViewModel>> ManageEmployeePassportAsync(int id, ChangeSet<EmployeePassportViewModel> model, bool isDeleted)
        {
            return await Task.Run(() =>
            {
                return ManageEmployeePassport(id, model, isDeleted);
            });
        }

        public async Task<ChangeSet<EmployeeCertification>> ManageEmployeeCertificationsAsync(int id, ChangeSet<EmployeeCertification> model, bool isDeleted)
        {
            return await Task.Run(() =>
            {
                return ManageEmployeeCertifications(id, model, isDeleted);
            });
        }

        public async Task<ChangeSet<EmployeeEmploymentHistory>> ManageEmployeeEmploymentHistoryAsync(int id, ChangeSet<EmployeeEmploymentHistory> model, bool isDeleted)
        {
            return await Task.Run(() =>
            {
                return ManageEmployeeEmploymentHistory(id, model, isDeleted);
            });
        }

        public async Task<ChangeSet<EmployeeVisa>> ManageEmployeeVisaAsync(int id, ChangeSet<EmployeeVisa> model, bool isDeleted)
        {
            return await Task.Run(() =>
            {
                return ManageEmployeeVisa(id, model, isDeleted);
            });
        }

        public async Task<ChangeSet<EmployeeQualification>> ManageEmployeeQualificationsAsync(int id, ChangeSet<EmployeeQualification> model, bool isDeleted)
        {
            return await Task.Run(() =>
            {
                return ManageEmployeeQualifications(id, model, isDeleted);
            });
        }

        public async Task<ChangeSet<EmployeeEmergencyContact>> ManageEmployeeEmergencyContactsAsync(int id, ChangeSet<EmployeeEmergencyContact> model, bool isDeleted)
        {
            return await Task.Run(() =>
            {
                return ManageEmployeeEmergencyContacts(id, model, isDeleted);
            });
        }

        public async Task<ChangeSet<EmployeeMedicalHistory>> ManageEmployeeMedicalHistoryAsync(int id, ChangeSet<EmployeeMedicalHistory> model, bool isDeleted)
        {
            return await Task.Run(() =>
            {
                return ManageEmployeeMedicalHistory(id, model, isDeleted);
            });
        }

        public async Task<ChangeSet<EmployeeDependent>> ManageEmployeeDependentAsync(int id, ChangeSet<EmployeeDependent> model, bool isDeleted)
        {
            return await Task.Run(() =>
            {
                return ManageEmployeeDependent(id, model, isDeleted);
            });
        }

        public async Task<ChangeSet<EmployeeDeclaration>> ManageEmployeeDeclarationAsync(int id, ChangeSet<EmployeeDeclaration> model, bool isDeleted)
        {
            return await Task.Run(() =>
            {
                return ManageEmployeeDeclaration(id, model, isDeleted);
            });
        }

        public async Task<ChangeSet<EmployeeSkill>> ManageEmployeeSkillsAsync(int id, ChangeSet<EmployeeSkill> model, bool isDeleted)
        {
            return await Task.Run(() =>
            {
                return ManageEmployeeSkills(id, model, isDeleted);
            });
        }

        public async Task<EmployeeCompetency> ManageEmployeeCompetencyAsync(int id, EmployeeCompetency model)
        {
            return await Task.Run(() =>
            {
                if (model.SkillDescription != null) // Check if SkillDescription is not null
                {
                    return ManageEmployeeSkillsDescription(id, model);
                }
                else
                {
                    return ManageEmployeeCompetency(id, model);
                }
            });
        }

        //public async Task<ChangeSet<PersonBGMapping>> ManageEmployeeBGCAsync(int id, ChangeSet<PersonBGMapping> model)
        //{
        //    return await Task.Run(() =>
        //    {
        //        return ManageEmployeeBGC(id, model);
        //    });
        //}

        public async Task<IEnumerable<EmployeeMyTeam>> GetMyTeamList(int id)
        {
            return await Task.Run(() =>
            {
                return TeamListFilter(id);
            });
        }

        public async Task<IEnumerable<EmployeeMyTeam>> GetAllEmpList(int id)
        {
            return await Task.Run(() =>
            {
                return GetAllEmp(id);
            });
        }

        public async Task<ChangeSet<EmployeeSittingLocation>> UpdateEmployeeSittingLocationDetail(int id, ChangeSet<EmployeeSittingLocation> model)
        {
            return await Task.Run(() =>
            {
                var oldPersonEmployment = new PersonEmployment();

                oldPersonEmployment = service.First<PersonEmployment>(x => x.Person.ID == id);

                var newPersonEmployment = oldPersonEmployment;
                newPersonEmployment.OfficeExtension = model.NewModel.Extension;
                newPersonEmployment.SeatingLocation = model.NewModel.SeatingLocation;
                var isUpdated = service.Update<PersonEmployment>(newPersonEmployment, oldPersonEmployment);
                if (isUpdated)
                    service.Finalize(true);
                return model;
            });
        }

        #endregion Public Contracts

        #region Helpers

        private IEnumerable<TViewModel> ExecuteSearch<TQueryEntity, TOutputEntity, TViewModel>(
            string query,
            Func<string, KeyValuePair<string, Type>, string, Expression<Func<TQueryEntity, bool>>> predicateBodyBuilder,
            Func<IEnumerable<TQueryEntity>, IEnumerable<TOutputEntity>> expression)
            where TQueryEntity : class
            where TOutputEntity : class
            where TViewModel : class
        {
            Expression<Func<TQueryEntity, bool>> predicateBody = null;
            var queryType = SearchParameterIdentifier.IdentifyEmployeeSearchOnPersonQuery(query, DesignationList().ToList());
            predicateBody = predicateBodyBuilder(query, queryType.Value, "Equals");
            var employee = service.Top<TQueryEntity>(100, predicateBody);
            if (employee != null)
            {
                var expressionResult = expression != null ? expression(employee) : employee as IEnumerable<TOutputEntity>;
                return Mapper.Map<IEnumerable<TOutputEntity>, IEnumerable<TViewModel>>(expressionResult);
            }
            return null;
        }

        private IEnumerable<Designation> DesignationList()
        {
            IEnumerable<Designation> aaa = new List<Designation>();

            return aaa;
        }
        public async Task<Boolean> ApproveBulkEmployeeDetails(string currentUserEmail, int statusID, string comments, params int[] empIDs)
        {         
            foreach (var empID in empIDs)
            {
                var employeeApproval = await GetApprovalById(empID);
                foreach (EmployeeApprovalViewModel employeeApprovalViewModel in employeeApproval.EmployeeApprovalViewModelList)
                {
                    var result = await ApproveEmployeeDetails(currentUserEmail, employeeApprovalViewModel.StageID, employeeApprovalViewModel.ModuleID, empID, statusID, comments);
                }
            }
        return true;            
        }
        private ChangeSet<T> GetChangeSetForApproval<T>(Stage model)
        {
            ChangeSet<T> empPersonalDetails = new ChangeSet<T>
            {
                ModuleCode = model.ModuleCode,
                ModuleId = (int)model.ModuleID,
                NewModel = JsonConvert.DeserializeObject<T>(model.NewEntry, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() }),
                OldModel = JsonConvert.DeserializeObject<T>(model.PreviousEntry, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() }),
                SendForApproval = false
            };

            return empPersonalDetails;
        }

        private ChangeSet<T> GetMultiRecordChangeSetForApproval<T>(MultiRecordStage model)
        {
            ChangeSet<T> empPersonalDetails = new ChangeSet<T>
            {
                ModuleCode = model.ModuleCode,
                ModuleId = (int)model.ModuleID,
                NewModel = JsonConvert.DeserializeObject<T>(model.NewEntry, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() }),
                OldModel = JsonConvert.DeserializeObject<T>(model.PreviousEntry, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() }),
                SendForApproval = false
            };

            return empPersonalDetails;
        }

        private async Task<int> CallStrategy<T>(IBasicOperationsService service, IEmailService emailService, ISaveToStageService saveService, int empID, T model, int statusID, Func<int, ChangeSet<T>, bool, ChangeSet<T>> updateWith, Stage stage, string comment, string currentUserEmail, string moduleCode) where T : new()
        {
            var person = service.First<Person>(x => x.ID == empID);
            string empName = person.FirstName + " " + person.LastName;
            ApprovalUser approvalUser = new ApprovalUser
            {
                empId = empID,
                empName = person.FirstName + " " + person.LastName,
                empEmail = person.PersonEmployment.First().OrganizationEmail,
                empImage = person.Image,
                currentUserEmail = currentUserEmail,
                cardName = moduleCode
            };
            UpdateApprovalStrategy<T> strategy = new SingleCardApprovalStrategy<T>(service, emailService, _stageService, approvalUser, default(T), statusID, updateWith);
            return await strategy.ProcessSingle(stage, comment);
        }

        private async Task<int> CallMultiStrategy<T>(IEnumerable<MultiRecordStage> oldStageModel, IBasicOperationsService service, IEmailService emailService, ISaveToStageService saveService, int empID, T model, int statusID, Func<int, ChangeSet<T>, bool, ChangeSet<T>> updateWith, string comment, string currentUserEmail, string moduleCode) where T : new()
        {
            var person = service.First<Person>(x => x.ID == empID);
            string empName = person.FirstName + " " + person.LastName;

            ApprovalUser approvalUser = new ApprovalUser
            {
                empId = empID,
                empName = person.FirstName + " " + person.LastName,
                empEmail = person.PersonEmployment.First().OrganizationEmail,
                empImage = person.Image,
                currentUserEmail = currentUserEmail,
                cardName = moduleCode
            };
            foreach (var item in oldStageModel.ToList())
            {
                UpdateApprovalStrategy<T> strategy = new SingleCardApprovalStrategy<T>(service, emailService, _stageService, approvalUser, default(T), statusID, updateWith);
                await strategy.ProcessMultiple(item, comment, false);
            }
            return 1;
        }

        private async Task<int> ApproveMultiRecordStage(string currentUserEmail, int moduleID, int empID, IEnumerable<MultiRecordStage> oldStageModel, int statusID, string comments)
        {
            int returnVal = 0;

            switch (moduleID)
            {
                case 2:
                    returnVal = await CallMultiStrategy<EmployeeAddress>(oldStageModel, service, emailService, _stageService, empID, null, statusID, UpdateEmployeeAddress, comments, currentUserEmail, "Contact Detail");
                    break;

                case 3:
                    returnVal = await CallMultiStrategy<EmployeeQualification>(oldStageModel, service, emailService, _stageService, empID, null, statusID, ManageEmployeeQualifications, comments, currentUserEmail, "Education Detail");
                    break;

                case 4:
                    returnVal = await CallMultiStrategy<EmployeePassportViewModel>(oldStageModel, service, emailService, _stageService, empID, null, statusID, ManageEmployeePassport, comments, currentUserEmail, "Passport Detail");
                    break;

                case 5:
                    returnVal = await CallMultiStrategy<EmployeeEmergencyContact>(oldStageModel, service, emailService, _stageService, empID, null, statusID, ManageEmployeeEmergencyContacts, comments, currentUserEmail, "Emergency Detail");
                    break;

                case 6:
                    returnVal = await CallMultiStrategy<EmployeeCertification>(oldStageModel, service, emailService, _stageService, empID, null, statusID, ManageEmployeeCertifications, comments, currentUserEmail, "Certification Detail");
                    break;

                case 7:
                    returnVal = await CallMultiStrategy<EmployeeEmploymentHistory>(oldStageModel, service, emailService, _stageService, empID, null, statusID, ManageEmployeeEmploymentHistory, comments, currentUserEmail, "Experience Detail");
                    break;

                case 8:
                    returnVal = await CallMultiStrategy<EmployeeVisa>(oldStageModel, service, emailService, _stageService, empID, null, statusID, ManageEmployeeVisa, comments, currentUserEmail, "Visa Detail");
                    break;

                case 10:
                    returnVal = await CallMultiStrategy<EmployeeMedicalHistory>(oldStageModel, service, emailService, _stageService, empID, null, statusID, ManageEmployeeMedicalHistory, comments, currentUserEmail, "Medical History");
                    break;

                case 16:
                    returnVal = await CallMultiStrategy<EmployeeSkill>(oldStageModel, service, emailService, _stageService, empID, null, statusID, ManageEmployeeSkills, comments, currentUserEmail, "Skills");
                    break;

                case 17:
                    returnVal = await CallMultiStrategy<EmployeeDependent>(oldStageModel, service, emailService, _stageService, empID, null, statusID, ManageEmployeeDependent, comments, currentUserEmail, "Dependent");
                    break;

                case 18:
                    returnVal = await CallMultiStrategy<EmployeeDeclaration>(oldStageModel, service, emailService, _stageService, empID, null, statusID, ManageEmployeeDeclaration, comments, currentUserEmail, "Declaration");
                    break;

                default:
                    break;
            }
            return returnVal;
        }

        private async Task<int> ApproveStageItem(string currentUserEmail, int moduleID, int empID, Stage oldStageModel, int statusID, string comments)
        {
            int returnVal = 0;

            switch (moduleID)
            {
                case 1:
                    returnVal = await CallStrategy<EmployeePersonalDetails>(service, emailService, _stageService, empID, null, statusID, UpdateEmployeePersonal, oldStageModel, comments, currentUserEmail, "Personal Detail");
                    break;

                case 2:
                    returnVal = await CallStrategy<EmployeeAddress>(service, emailService, _stageService, empID, null, statusID, UpdateEmployeeAddress, oldStageModel, comments, currentUserEmail, "Contact Detail");
                    break;

                default:
                    break;
            }
            return returnVal;
        }

        private int HookApproval(int userId, int recordID, int componentID)
        {
            var strategy = ApprovalStrategyFactory.GetStrategy(ApprovalStrategy.HrOnly, userId);
            strategy.opsService = this.service;
            ApprovalService service = new ApprovalService(this.service);
            service.SendForApproval(userId, 2, recordID, strategy.FetchApprovers(), componentID);
            return strategy.FetchApprovers().First();
        }

        private async Task<int> UpdateHookedApproval(int userId, int componentID, int statusID, string statusComment)
        {
            var strategy = ApprovalStrategyFactory.GetStrategy(ApprovalStrategy.HrOnly, userId);
            strategy.opsService = this.service;
            ApprovalService service = new ApprovalService(this.service);
            return await service.UpdateApproval(userId, 2, 2, statusID, statusComment, componentID);
        }

        private ApprovalHistory SetApprovalHistory(ApprovalDetail detail)
        {
            return new ApprovalHistory { comments = detail.StatusComment, dateTime = detail.ApprovalDate.HasValue ? detail.ApprovalDate.Value.ToStandardDate() : "" };
        }

        private IEnumerable<EmployeeMyTeam> TeamListFilter(int id)
        {
            IEnumerable<EmployeeMyTeam> myTeamList = new List<EmployeeMyTeam>();
            var lstStatus = service.All<EmploymentStatus>();
            var teamList = new List<EmployeeMyTeam>();
            var personReporting = service.Top<PersonReporting>(0, x => x.ReportingTo == id).ToList();
            foreach (var item in personReporting.ToList())
            {
                var person = service.First<Person>(x => x.ID == item.PersonID && x.Active == true);
                if (person != null)
                {
                    EmployeeMyTeam myTeam = new EmployeeMyTeam();
                    Dictionary<string, dynamic> empStatus = new Dictionary<string, dynamic>();
                    empStatus.Add("status", lstStatus.Where(x => x.Id == person.PersonEmployment.First().EmploymentStatus.Value).First().Description);
                    empStatus.Add("ConfirmationDate", person.PersonEmployment.Where(x => x.ConfirmationDate != null).Select(x => x.ConfirmationDate.Value).FirstOrDefault());
                    myTeam.ReportingTo = item.ReportingTo;
                    myTeam.EmployeeTeamProfile = Mapper.Map<Person, EmployeeBasicProfile>(person);
                    myTeam.EmployementStatus = empStatus;
                    teamList.Add(myTeam);
                }
            }
            myTeamList = teamList.OrderBy(d => d.EmployeeTeamProfile.FirstName);
            return myTeamList;
        }

        private IEnumerable<EmployeeMyTeam> GetAllEmp(int id)
        {
            IEnumerable<EmployeeMyTeam> empList = new List<EmployeeMyTeam>();
            var lstStatus = service.All<EmploymentStatus>();
            var teamList = new List<EmployeeMyTeam>();
            var personList = service.All<PersonReporting>().ToList();
            foreach (var item in personList.ToList())
            {
                var person = service.First<Person>(x => x.ID == item.PersonID && x.Active == true);
                if (person != null)
                {
                    EmployeeMyTeam allEmpList = new EmployeeMyTeam();
                    Dictionary<string, dynamic> empStatus = new Dictionary<string, dynamic>();
                    empStatus.Add("status", lstStatus.Where(x => x.Id == person.PersonEmployment.First().EmploymentStatus.Value).First().Description);
                    empStatus.Add("ConfirmationDate", person.PersonEmployment.Where(x => x.ConfirmationDate != null).Select(x => x.ConfirmationDate.Value).FirstOrDefault());
                    allEmpList.ReportingTo = item.ReportingTo;
                    allEmpList.EmployeeTeamProfile = Mapper.Map<Person, EmployeeBasicProfile>(person);
                    allEmpList.EmployementStatus = empStatus;
                    teamList.Add(allEmpList);
                }
            }
            empList = teamList.OrderBy(d => d.EmployeeTeamProfile.FirstName);
            return empList;
        }

        private IEnumerable<EmployeeMyTeam> GetMyTeamHistory(int id)
        {

            IEnumerable<EmployeeMyTeam> myTeamList = new List<EmployeeMyTeam>();
            var lstStatus = service.All<EmploymentStatus>();
            var teamList = new List<EmployeeMyTeam>();
            var personReporting = service.Top<PersonReporting>(0, x => x.ReportingTo == id).ToList();
            foreach (var item in personReporting.ToList())
            {
                var person = service.First<Person>(x => x.ID == item.PersonID && x.Active == false);
                if (person != null)
                {
                    EmployeeMyTeam myTeam = new EmployeeMyTeam();
                    Dictionary<string, dynamic> empStatus = new Dictionary<string, dynamic>();
                    empStatus.Add("status", lstStatus.Where(x => x.Id == person.PersonEmployment.First().EmploymentStatus.Value).First().Description);
                    empStatus.Add("ConfirmationDate", person.PersonEmployment.Where(x => x.ConfirmationDate != null).Select(x => x.ConfirmationDate.Value).FirstOrDefault());
                    empStatus.Add("ExitDate", person.PersonEmployment.Where(x => x.ExitDate != null).Select(x => x.ExitDate.Value).FirstOrDefault());
                    myTeam.ReportingTo = item.ReportingTo;
                    myTeam.EmployeeTeamProfile = Mapper.Map<Person, EmployeeBasicProfile>(person);
                    myTeam.EmployementStatus = empStatus;

                    teamList.Add(myTeam);
                }
            }
            myTeamList = teamList;

            return myTeamList;

        }

        #endregion Helpers

        public async Task<IEnumerable<EmployeeMyTeam>> GetMyTeamListDeep(int id)//RS: need to remove ID with current user, this will happen when all code will be in sync
        {
            return await Task.Run(() =>
            {
                List<EmployeeMyTeam> myTeam = TeamListFilter(id).ToList();
                return myTeam;
            });
        }

        public async Task<IEnumerable<EmployeeMyTeam>> GetMyTeamHistoryList(int id)//RS: need to remove ID with current user, this will happen when all code will be in sync
        {
            return await Task.Run(() =>
            {
                List<EmployeeMyTeam> myTeam = GetMyTeamHistory(id).ToList();
                return myTeam;
            });
        }

        //To send request in Approval and ApprovalDetail tables when HR edit other employee data through Employee Search module
        private int HREditingApproval(int userId, int recordID, int componentID, int currUserID)
        {
            var strategy = ApprovalStrategyFactory.GetStrategy(ApprovalStrategy.HrOnly, userId);
            strategy.opsService = this.service;
            ApprovalService service = new ApprovalService(this.service);
            service.SendForHRApproval(userId, 2, recordID, currUserID, componentID);
            return 1;
        }
        public async Task<IEnumerable<EmployeeDesignation>> GetDesignation()
        {
            return await Task.Run(() =>
            {
                return service.All<Designation>()
                                .Select(t => new EmployeeDesignation()
                                {
                                    ID = t.ID,
                                    Text = t.Name,
                                }).OrderBy(t => t.Text); //Added sorting based on VR202132330 on 8th Sep 2021
            });
        }

        public IEnumerable<EmployeeWorkLocation> GetEmpWorkLocation()
        {
            //return await Task.Run(() =>
            //{
            IEnumerable<EmployeeWorkLocation> empWorkLocationList = new List<EmployeeWorkLocation>();
            var designationList = service.All<WorkLocation>().Where(x => x.ParentLocation != null && x.IsDeleted == false);
            var _tempList = new List<EmployeeWorkLocation>();

            foreach (var item in designationList)
            {
                EmployeeWorkLocation _EmployeeWorkLocation = new EmployeeWorkLocation();
                _EmployeeWorkLocation.ID = item.ID;
                _EmployeeWorkLocation.Text = item.LocationName;

                _tempList.Add(_EmployeeWorkLocation);
            }

            empWorkLocationList = _tempList;
            return empWorkLocationList;
            // });
        }

        public async Task<IEnumerable<EmployeeWorkLocation>> GetEmpOfficeLocation()
        {
            return await Task.Run(() =>
            {
                IEnumerable<EmployeeWorkLocation> empOfficeLocationList = new List<EmployeeWorkLocation>();
                var officeLocationList = service.All<WorkLocation>().Where(x => x.ParentLocation == null && x.IsDeleted == false);
                var _tempList = new List<EmployeeWorkLocation>();

                foreach (var item in officeLocationList)
                {
                    EmployeeWorkLocation _empOfficeLocation = new EmployeeWorkLocation();
                    _empOfficeLocation.ID = item.ID;
                    _empOfficeLocation.Text = item.LocationName;

                    _tempList.Add(_empOfficeLocation);
                }

                empOfficeLocationList = _tempList;
                return empOfficeLocationList;
            });
        }

        public async Task<IEnumerable<EmploymentStatusList>> GetEmploymentStatus()
        {
            return await Task.Run(() =>
            {
                IEnumerable<EmploymentStatusList> employmentStatusList = new List<EmploymentStatusList>();
                var statusList = service.Top<EmploymentStatus>(0, x => x.IsDeleted == false);
                var _tempList = new List<EmploymentStatusList>();

                foreach (var item in statusList)
                {
                    EmploymentStatusList _EmploymentStatus = new EmploymentStatusList();
                    _EmploymentStatus.ID = item.Id;
                    _EmploymentStatus.Text = item.Description;

                    _tempList.Add(_EmploymentStatus);
                }

                employmentStatusList = _tempList;
                return employmentStatusList;
            });
        }

        public string GetEmployeeName(int id)
        {
            var emp = service.Top<Person>(0, x => x.ID == id).FirstOrDefault();
            return $"{emp.FirstName}  {emp.LastName}";
        }

        public string GetCommaSeparatedEmployeeNames(string employeeIds)
        {
            List<string> lstEmployeeName = new List<string>();
            if (employeeIds != null)
            {
                foreach (string employeeId in employeeIds.Split(','))
                {
                    if (employeeId != null)
                    {
                        int empId = int.Parse(employeeId);
                        var emp = service.Top<Person>(0, x => x.ID == empId).FirstOrDefault();
                        if (emp != null)
                            lstEmployeeName.Add(emp.FirstName + " " + emp.LastName);
                    }
                }
            }
            return string.Join(", ", lstEmployeeName);
        }

        public Task<Pheonix.Models.ViewModels.VPBUApproverHandlerModel> checkIsBUApprover(Pheonix.Models.ViewModels.VPBUApproverHandlerModel vPBUApproverVM)
        {
            if (vPBUApproverVM != null)
            {
                List<long> businessUnitList = new List<long>();
                List<VCFApprover> vPBUApproverBUList = service.Top<VCFApprover>(0, v => v.IsDeleted == 0 && v.ReviewerId == vPBUApproverVM.userId).OrderBy(v => v.DeliveryUnitID).ToList();
                if (vPBUApproverBUList != null && vPBUApproverBUList.Count() > 0)
                {
                    vPBUApproverVM.isBUApprover = true;
                    foreach (var vPBUApproverRowObj in vPBUApproverBUList)
                    {
                        businessUnitList.Add(vPBUApproverRowObj.DeliveryUnitID);
                    }
                    vPBUApproverVM.assignedBUList = businessUnitList;
                    service.Finalize(true);
                }
            }
            return Task.Run(() => { return vPBUApproverVM; });
        }
     
        public EmployeePreviousExperience AddEmployeeExperience(EmployeePreviousExperience model)
        { 
            try
            {
                var oldPersonEmployment = service.First<PersonPastExperience>(x => x.PersonID == model.PersonID);
                if (oldPersonEmployment == null)
                {
                    var newPersonEmployment = new PersonPastExperience
                    {
                        PersonID = model.PersonID,
                        ExperienceYears = model.year,
                        ExperienceMonths = model.month
                    };
                    var isUpdated = service.Create<PersonPastExperience>(newPersonEmployment, x => x.PersonID == model.PersonID);
                    if (isUpdated)
                    {
                        service.Finalize(true);
                    }
                }
                else
                {                   
                    oldPersonEmployment.ExperienceYears = model.year;
                    oldPersonEmployment.ExperienceMonths = model.month;
                    var empExpUpdated = service.Update<PersonPastExperience>(oldPersonEmployment);
                    if (empExpUpdated)
                    {              
                        service.Finalize(true);
                    }                
                }               
                return model;
            }           
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public EmployeePreviousExperience GetEmployeePastExperience(int id)
        {
            try
            {
                var employeeExperienceList = service.First<PersonPastExperience>(x => x.PersonID == id);

                if (employeeExperienceList == null)
                {
                    return null;
                }
              
                var employeePreviousExperience = new EmployeePreviousExperience
                {
                    PersonID = employeeExperienceList.PersonID,
                    year = employeeExperienceList.ExperienceYears ?? 0,
                    month = employeeExperienceList.ExperienceMonths ?? 0
                };
                return employeePreviousExperience;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<IEnumerable<CorrectionEmployee>> GetAllActiveEmpList()
        {
            return await Task.Run(() =>
            {
                return GetAllActiveEmp();
            });
        }
        private List<CorrectionEmployee> GetAllActiveEmp()
        {
            var _empList = new List<CorrectionEmployee>();
            try
            {
                using (PhoenixEntities context = new PhoenixEntities())
                {
                    var empList = context.GetAllActiveEmpList().ToList();
                    foreach (var item in empList)
                    {
                        CorrectionEmployee _empListViewModel = new CorrectionEmployee();
                        _empListViewModel.id = item.ID;
                        _empListViewModel.firstName = item.FirstName;
                        _empListViewModel.middleName = item.MiddleName;
                        _empListViewModel.lastName = item.LastName;
                        _empListViewModel.reportingTo = item.ReportingTo;
                        _empList.Add(_empListViewModel);                        
                    }
                }
            }
            catch (Exception ex)
            {
                throw new HttpException(500, ex.ToString());
            }
            return _empList;
        }

        private int GetApprovalID(int employeeID, int moduleId)
        {
            int approvalID = service.Top<Pheonix.DBContext.Approval>(0, x => x.RequestBy == employeeID && x.RequestType == 2 && x.Status == 0 && x.Component == moduleId && x.IsDeleted == false).OrderByDescending(x => x.ID).Select(x => x.ID).FirstOrDefault();
            return approvalID;
        }

        public void UpdateNewEntryJSONStringInDB<T>(ChangeSet<T> model, int employeeId,int moduleId)
        {
            var newModel = JsonConvert.SerializeObject(model.NewModel, Formatting.None, new IsoDateTimeConverter() { DateTimeFormat = "MM/dd/yyyy" });
            UpdateApprovalID(employeeId, newModel, moduleId);
        }

        private void UpdateApprovalID(int employeeId, string newModel, int moduleId)
        {
            bool isUpdated = false;
            using (PhoenixEntities dbContext = new PhoenixEntities())
            {
                using (var transaction = dbContext.Database.BeginTransaction())
                {
                    try
                    {
                        if (moduleId == 1)
                        {
                            int stageID = service.Top<Stage>(0, x => x.By == employeeId && x.ApprovalStatus == 0 && x.IsDeleted == false).OrderByDescending(x => x.ID).Select(x => x.ID).FirstOrDefault();
                            Stage s = (from x in dbContext.Stage
                                       where x.ID == stageID 
                                       select x).FirstOrDefault();
                            if (s != null)
                            {
                                s.NewEntry = newModel; 
                                dbContext.Entry(s).State = EntityState.Modified;
                                dbContext.SaveChanges();
                            }
                            isUpdated = true;
                        }
                        else
                        {
                            int multiStageID = service.Top<MultiRecordStage>(0, x => x.By == employeeId && x.ApprovalStatus == 0 && x.ModuleID == moduleId && x.IsDeleted == false).OrderByDescending(x => x.ID).Select(x => x.ID).FirstOrDefault();
                            MultiRecordStage m = (from x in dbContext.MultiRecordStage
                                       where x.ID == multiStageID
                                       select x).FirstOrDefault();
                            if (m != null)
                            {
                                m.NewEntry = newModel;
                                dbContext.Entry(m).State = EntityState.Modified;
                                dbContext.SaveChanges();
                            }
                            isUpdated = true;
                        }

                        if (isUpdated)
                        {
                            transaction.Commit();
                        }
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }              
            }
        }

        public async Task<IEnumerable<EmployeeProfileAttachmentDetails>> GetAttachmentRecordByApprovalId(int approvalId)
        {
            try
            {
                return await Task.Run(() =>
                {
                    List<EmployeeProfileAttachmentDetails> empAttachmentList = new List<EmployeeProfileAttachmentDetails>();
                    var employeeProfileAttachmentRecords = service.All<EmployeeProfileAttachment>(x => x.ApprovalID == approvalId && x.IsDeleted == false);

                    foreach (var item in employeeProfileAttachmentRecords)
                    {
                        var epad = new EmployeeProfileAttachmentDetails()
                        {
                            ID = item.ID,
                            ApprovalID = item.ApprovalID,
                            FieldName = item.FieldName,
                            FileName = item.FileName,
                            UniqueFileName = item.UniqueFileName
                        };

                        empAttachmentList.Add(epad);
                    }

                    return empAttachmentList;
                });
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }
    }
}

