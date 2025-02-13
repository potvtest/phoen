using Pheonix.Core.v1.Services.Business;
using Pheonix.DBContext;
using Pheonix.Models;
using Pheonix.Models.VM;
using Pheonix.Models.VM.Classes.Employee;
using Pheonix.Web.Extensions;
using Pheonix.Web.Filters;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;

namespace Pheonix.Web.Controllers.v1
{

    [RoutePrefix("v1/employee")]
    [Authorize]
    public class EmployeeController : ApiController
    {
        private IEmployeeService _service;

        public EmployeeController(IEmployeeService service)
        {
            _service = service;
        }

        // Added for testing purpose.
        [HttpGet, Route("test1")]
        public string test1()
        {
            if (RequestValidator.IsValid(RequestContext.GetClaimInt(ClaimTypes.Role)))
            {
                return "dsdsd";
            }
            return "You are not authorized.";
        }

        // Added for testing purpose.
        [HttpGet, Route("test")]
        public string test()
        {
            return RequestContext.GetClaim(ClaimTypes.Email);
        }

        [HttpGet, Route("all")]
        public async Task<IEnumerable<PersonViewModel>> All()
        {
            return await _service.List<PersonViewModel>(null);
        }

        [HttpGet, Route("profile/{id:int}")]
        public async Task<EmployeeProfileViewModel> All(int id)
        {
            if (RequestValidator.IsValid(RequestContext.GetClaimCsvToArray<int>(ClaimTypes.Role)))
            {
                var profile = await _service.GetProfile(id, false);
                return profile;
            }
            return null;
        }

        [HttpGet, Route("my/profile")]
        public async Task<EmployeeProfileViewModel> MyProfile()
        {
            return await _service.GetProfile(RequestContext.GetClaimInt(ClaimTypes.PrimarySid), true);
        }

        [HttpGet, Route("get-card-status")]
        public async Task<IEnumerable<int>> GetCardStatus(bool isMyRecord = false)
        {
            return await _service.GetCardStatus(RequestContext.GetClaimInt(ClaimTypes.PrimarySid), isMyRecord);
        }

        [HttpPost, Route("personal/personalupdate/{id:int}")]
        public async Task<ChangeSet<EmployeePersonalDetails>> UpdateEmployeePersonal(int id, ChangeSet<EmployeePersonalDetails> model)
        {
            model.NewModel.SearchUserID = id;
            var personalDetail = await _service.UpdateEmployeePersonalAsync(RequestContext.GetClaimInt(ClaimTypes.PrimarySid), model);
            return personalDetail;
        }

        [HttpPost, Route("personal/personalupdate/addattachmentdetails")]
        public async Task<IHttpActionResult> AddEmployeeProfileAttachmentDetails(IEnumerable<EmployeeProfileAttachmentDetails> model)
        {
            try
            {
                var successMessage = "Employee profile attachment details added successfully !!";
                var isSuccess = await _service.AddEmployeeProfileAttachmentDetails(model);
                return Ok(new
                {
                    isSuccess,
                    message = successMessage
                });
            }
            catch (Exception)
            {
                return Content(HttpStatusCode.InternalServerError, "Error while adding Employee profile attachment details");
            }
        }

        [HttpPost, Route("personal/addressupdate/{id:int}")]
        public async Task<ChangeSet<EmployeeAddress>> UpdateEmployeeAddress(int id, ChangeSet<EmployeeAddress> model)
        {
            model.NewModel.SearchUserID = id;
            var personAddress = await _service.UpdateEmployeeAddressAsync(RequestContext.GetClaimInt(ClaimTypes.PrimarySid), model);
            return personAddress;
        }

        [HttpPost, Route("personal/passportmanage/{id:int}")]
        public async Task<ChangeSet<EmployeePassportViewModel>> ManageEmployeePassport(int id, ChangeSet<EmployeePassportViewModel> model, bool isDeleted = false)
        {
            if (model.NewModel.PassportNumber == null || model.NewModel.PassportNumber == "")
                return null;
            model.NewModel.SearchUserID = id;
            var personPassport = await _service.ManageEmployeePassportAsync(RequestContext.GetClaimInt(ClaimTypes.PrimarySid), model, isDeleted);
            return personPassport;
        }

        [HttpGet, Route("get-all-employess/{EmpListFor}")]
        public async Task<IEnumerable<PersonViewModel>> GetAllEmployess(string EmpListFor)
        {
            return await _service.GetAllEmployess(EmpListFor);
        }

        [HttpGet, Route("get-employee-filter-list/{filterListFor:int}")]
        public async Task<IEnumerable<EmployeeSearchFilters>> GetEmployeeFilterList(int filterListFor)
        {
            return await _service.GetEmployeeFilterList(filterListFor);
        }

        [HttpGet, Route("Search/{*query}")]  // -- not in use
        public async Task<IEnumerable<EmployeeBasicProfile>> GetEmployeeForAdmin(string query, int location = 0, bool showInActive = false)
        {
            if (RequestValidator.IsValid(RequestContext.GetClaimCsvToArray<int>(ClaimTypes.Role)))
            {
                return await _service.SearchEmployee(query.Trim(), showInActive);
            }
            return null;
        }

        [HttpGet, Route("Search-for-admin/{*query}")]   
        public async Task<AdminSearchResult> GetEmployee(string query, int location = 0, bool showInActive = false)
        {
            AdminSearchResult adminsearchresult = new AdminSearchResult();

            if (RequestValidator.IsValid(RequestContext.GetClaimCsvToArray<int>(ClaimTypes.Role)))
            {
                adminsearchresult.BasicProfile = await _service.SearchEmployeeForAdminTasks(query.Trim(), showInActive);
            }

            if (RequestValidator.IsValid(RequestContext.GetClaimCsvToArray<int>(ClaimTypes.Role)))
            {
                adminsearchresult.EmployeeAdminHistory = await _service.GetEmployeeAdminHistory(query.Trim(), location, showInActive);
            }
            return adminsearchresult;
            //return null;
        }

        //[System.Web.Mvc.ValidateInput(false)]
        [HttpPost, Route("search-employess-based-on-filter")]
        public async Task<IEnumerable<EmployeeBasicProfile>> SearchEmployessBasedOnFilter(EmployeeSearchCriteria objEmployeeSearchCriteria)
        {
            if (RequestValidator.IsValid(RequestContext.GetClaimCsvToArray<int>(ClaimTypes.Role)))
            {
                return await _service.SearchEmployessBasedOnFilter(objEmployeeSearchCriteria);
            }
            return null;
        }

        [HttpPost, Route("personal/certificationmanage/{id:int}")]
        public async Task<ChangeSet<EmployeeCertification>> ManageEmployeeCertifications(int id, ChangeSet<EmployeeCertification> model, bool isDeleted = false)
        {
            model.NewModel.SearchUserID = id;
            var personalCertifications = await _service.ManageEmployeeCertificationsAsync(RequestContext.GetClaimInt(ClaimTypes.PrimarySid), model, isDeleted);
            return personalCertifications;
        }

        [HttpPost, Route("professional/emphistorymanage/{id:int}")]
        public async Task<ChangeSet<EmployeeEmploymentHistory>> ManageEmployeeEmploymentHistory(int id, ChangeSet<EmployeeEmploymentHistory> model, bool isDeleted = false)
        {
            model.NewModel.SearchUserID = id;
            var personalEmpHistories = await _service.ManageEmployeeEmploymentHistoryAsync(RequestContext.GetClaimInt(ClaimTypes.PrimarySid), model, isDeleted);
            return personalEmpHistories;
        }

        [HttpPost, Route("professional/UpdateEmployeePastExperience")]
        public EmployeePreviousExperience AddEmployeeExperience(EmployeePreviousExperience model)
        {
            var addEmployee = _service.AddEmployeeExperience(model);
            return addEmployee;
        }

        [HttpGet, Route("professional/GetEmployeePastExperience/{id:int}")]
        public EmployeePreviousExperience GetEmployeePastExperience(int id)
        {
            var getEmployeeExperience = _service.GetEmployeePastExperience(id);
            return getEmployeeExperience;
        }

        [HttpPost, Route("personal/visamanage/{id:int}")]
        public async Task<ChangeSet<EmployeeVisa>> ManageEmployeeVisa(int id, ChangeSet<EmployeeVisa> model, bool isDeleted = false)
        {
            model.NewModel.SearchUserID = id;
            var personalVisas = await _service.ManageEmployeeVisaAsync(RequestContext.GetClaimInt(ClaimTypes.PrimarySid), model, isDeleted);
            return personalVisas;
        }

        [HttpPost, Route("personal/qualificationmanage/{id:int}")]
        public async Task<ChangeSet<EmployeeQualification>> ManageEmployeeQualifications(int id, ChangeSet<EmployeeQualification> model, bool isDeleted = false)
        {
            model.NewModel.SearchUserID = id;
            var personalQualifications = await _service.ManageEmployeeQualificationsAsync(RequestContext.GetClaimInt(ClaimTypes.PrimarySid), model, isDeleted);
            return personalQualifications;
        }

        [HttpPost, Route("personal/emergencymanage/{id:int}")]
        public async Task<ChangeSet<EmployeeEmergencyContact>> ManageEmployeeEmergencyContacts(int id, ChangeSet<EmployeeEmergencyContact> model, bool isDeleted = false)
        {
            model.NewModel.SearchUserID = id;
            var personalEmergencyContacts = await _service.ManageEmployeeEmergencyContactsAsync(RequestContext.GetClaimInt(ClaimTypes.PrimarySid), model, isDeleted);
            return personalEmergencyContacts;
        }

        [HttpPost, Route("personal/medicalhistorymanage/{id:int}")]
        public async Task<ChangeSet<EmployeeMedicalHistory>> ManageEmployeeMedicalHistory(int id, ChangeSet<EmployeeMedicalHistory> model, bool isDeleted = false)
        {
            model.NewModel.SearchUserID = id;
            var personalMedicalHistories = await _service.ManageEmployeeMedicalHistoryAsync(RequestContext.GetClaimInt(ClaimTypes.PrimarySid), model, isDeleted);
            return personalMedicalHistories;
        }

        [HttpPost, Route("personal/dependentmanage/{id:int}")]
        public async Task<ChangeSet<EmployeeDependent>> ManageEmployeeDependent(int id, ChangeSet<EmployeeDependent> model, bool isDeleted = false)
        {
            model.NewModel.SearchUserID = id;
            var personalDependents = await _service.ManageEmployeeDependentAsync(RequestContext.GetClaimInt(ClaimTypes.PrimarySid), model, isDeleted);
            return personalDependents;
        }

        [HttpPost, Route("professional/declarationmanage/{id:int}")]
        public async Task<ChangeSet<EmployeeDeclaration>> ManageEmployeeDeclaration(int id, ChangeSet<EmployeeDeclaration> model, bool isDeleted = false)
        {
            model.NewModel.SearchUserID = id;
            var personalDeclarations = await _service.ManageEmployeeDeclarationAsync(RequestContext.GetClaimInt(ClaimTypes.PrimarySid), model, isDeleted);
            return personalDeclarations;
        }

        [HttpPost, Route("professional/skillsmanage/{id:int}")]
        public async Task<ChangeSet<EmployeeSkill>> ManageEmployeeSkills(int id, ChangeSet<EmployeeSkill> model, bool isDeleted = false)
        {
            model.NewModel.SearchUserID = id;
            var professionalSkills = await _service.ManageEmployeeSkillsAsync(RequestContext.GetClaimInt(ClaimTypes.PrimarySid), model, isDeleted);
            return professionalSkills;
        }

        [HttpPost, Route("professional/competencymanage")]
        public async Task<EmployeeCompetency> ManageEmployeeCompetency(EmployeeCompetency model)
        {
            var competency = await _service.ManageEmployeeCompetencyAsync(model.PersonID ?? 0, model);
            return competency;
        }

        //[HttpPost, Route("professional/bgcmanage/{id:int}")]
        //public async Task<ChangeSet<PersonBGMapping>> ManageEmployeeBGC(int id, ChangeSet<PersonBGMapping> model, bool isDeleted = false)
        //{
        //    //model.NewModel.SearchUserID = id;
        //    var professionalSkills = await _service.ManageEmployeeBGCAsync(RequestContext.GetClaimInt(ClaimTypes.PrimarySid), model);
        //    return professionalSkills;
        //}

        [HttpPost, Route("professional/joiningupdate/{id:int}")]
        public async Task<ChangeSet<EmployeeProfileViewModel>> UpdateEmployeeJoiningDetail(int id, ChangeSet<EmployeeProfileViewModel> model)
        {
            model.NewModel.SearchUserID = id;
            var professionalSkills = await _service.UpdateEmployeeJoiningDetail(RequestContext.GetClaimInt(ClaimTypes.PrimarySid), model);
            return professionalSkills;
        }

        [HttpPost, Route("professional/orgDetailsupdate/{id:int}")]
        public async Task<ChangeSet<EmployeeOrganizaionDetails>> UpdateEmployeeOrganizationDetail(int id, ChangeSet<EmployeeOrganizaionDetails> model)
        {
            var professionalOrg = await _service.UpdateEmployeeOrgDetail(RequestContext.GetClaimInt(ClaimTypes.PrimarySid), id, model);
            return professionalOrg;
        }
        [HttpGet, Route("dropdowns/{moduleName}")]
        public async Task<Dictionary<string, List<DropdownItems>>> GetDropdowns(string moduleName)
        {
            string listQuery = ConfigurationManager.AppSettings[moduleName].ToString();

            var dropdowns = await _service.GetDropdowns(listQuery);
            return dropdowns;
        }

        [HttpGet, Route("my-team-list")]
        public async Task<IEnumerable<EmployeeMyTeam>> GetMyTeamList()
        {
            var teamList = await _service.GetMyTeamList(RequestContext.GetClaimInt(ClaimTypes.PrimarySid));
            return teamList;
        }

        [HttpGet, Route("all-emp-list")]
        public async Task<IEnumerable<EmployeeMyTeam>> GetAllEmpList()
        {
            var empList = await _service.GetAllEmpList(RequestContext.GetClaimInt(ClaimTypes.PrimarySid));
            return empList;
        }

        [HttpPost, Route("professional/sittingLocationupdate/{id:int}")]
        public async Task<ChangeSet<EmployeeSittingLocation>> UpdateEmployeeSittingLocationDetail(int id, ChangeSet<EmployeeSittingLocation> model)
        {
            var sittingLocationDetail = await _service.UpdateEmployeeSittingLocationDetail(id, model);
            return sittingLocationDetail;
        }

        [HttpGet, Route("my-team-detail")]
        public async Task<IEnumerable<EmployeeMyTeam>> GetMyTeamDetail()
        {
            var teamList = await _service.GetMyTeamListDeep(RequestContext.GetClaimInt(ClaimTypes.PrimarySid));
            //var teamList = await _service.GetMyTeamListDeep(id);
            return teamList;
        }

        [HttpGet, Route("getempdesignation")]
        public async Task<IEnumerable<EmployeeDesignation>> GetDesignation()
        {
            return await _service.GetDesignation();
        }

        [HttpGet, Route("getempworklocation")]
        public IEnumerable<EmployeeWorkLocation> GetEmpWorkLocation()
        {
            return  _service.GetEmpWorkLocation();
        }

        [HttpGet, Route("getempofficelocation")]
        public async Task<IEnumerable<EmployeeWorkLocation>> GetEmpOfficeLocation()
        {
            return await _service.GetEmpOfficeLocation();
        }

        [HttpGet, Route("getemploymentstatus")]
        public async Task<IEnumerable<EmploymentStatusList>> GetEmploymentStatus()
        {
            return await _service.GetEmploymentStatus();
        }

        [HttpGet, Route("my-team-history")]
        public async Task<IEnumerable<EmployeeMyTeam>> GetMyTeamHistory()
        {
            var teamList = await _service.GetMyTeamHistoryList(RequestContext.GetClaimInt(ClaimTypes.PrimarySid));
            //var teamList = await _service.GetMyTeamListDeep(id);
            return teamList;
        }

        [HttpGet, Route("all-emp-list-filtered")]
        public async Task<IEnumerable<CorrectionEmployee>> GetAllActiveEmpList()
        {
            var empList = await _service.GetAllActiveEmpList();
            return empList;
        }
    }
}