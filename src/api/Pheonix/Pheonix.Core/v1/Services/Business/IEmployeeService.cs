using Pheonix.DBContext;
using Pheonix.Models;
//using Pheonix.Models.ViewModels;
using Pheonix.Models.VM;
using Pheonix.Models.VM.Classes.Employee;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pheonix.Core.v1.Services.Business
{
    public interface IEmployeeService : IDataService
    {
        Task<EmployeeProfileViewModel> GetProfile(int id, bool isMyRecord);

        Task<IEnumerable<int>> GetCardStatus(int id, bool isMyRecord);

        Task<IEnumerable<PersonViewModel>> GetAllEmployess(string query);

        Task<IEnumerable<EmployeeSearchFilters>> GetEmployeeFilterList(int filterListFor);

        Task<IEnumerable<EmployeeBasicProfile>> SearchEmployee(string query, bool showInActive);

        Task<IEnumerable<EmployeeBasicProfile>> SearchEmployessBasedOnFilter(EmployeeSearchCriteria objEmployeeSearchCriteria);

        Task<ChangeSet<EmployeePersonalDetails>> UpdateEmployeePersonalAsync(int id, ChangeSet<EmployeePersonalDetails> model);

        ChangeSet<EmployeePersonalDetails> UpdateEmployeePersonal(int id, ChangeSet<EmployeePersonalDetails> model, bool isMulti=false);

        Task<bool> AddEmployeeProfileAttachmentDetails(IEnumerable<EmployeeProfileAttachmentDetails> model);

        Task<ChangeSet<EmployeeAddress>> UpdateEmployeeAddressAsync(int id, ChangeSet<EmployeeAddress> model);

        ChangeSet<EmployeeAddress> UpdateEmployeeAddress(int id, ChangeSet<EmployeeAddress> model, bool isMulti = false);

        Task<ChangeSet<EmployeePassportViewModel>> ManageEmployeePassportAsync(int id, ChangeSet<EmployeePassportViewModel> model, bool isDeleted);

        ChangeSet<EmployeePassportViewModel> ManageEmployeePassport(int id, ChangeSet<EmployeePassportViewModel> model, bool isDeleted);

        Task<ChangeSet<EmployeeCertification>> ManageEmployeeCertificationsAsync(int id, ChangeSet<EmployeeCertification> model, bool isDeleted);

        ChangeSet<EmployeeCertification> ManageEmployeeCertifications(int id, ChangeSet<EmployeeCertification> model, bool isDeleted);

        Task<ChangeSet<EmployeeEmploymentHistory>> ManageEmployeeEmploymentHistoryAsync(int id, ChangeSet<EmployeeEmploymentHistory> model, bool isDeleted);

        ChangeSet<EmployeeEmploymentHistory> ManageEmployeeEmploymentHistory(int id, ChangeSet<EmployeeEmploymentHistory> model, bool isDeleted);

        Task<ChangeSet<EmployeeVisa>> ManageEmployeeVisaAsync(int id, ChangeSet<EmployeeVisa> model, bool isDeleted);

        ChangeSet<EmployeeVisa> ManageEmployeeVisa(int id, ChangeSet<EmployeeVisa> model, bool isDeleted);

        Task<ChangeSet<EmployeeQualification>> ManageEmployeeQualificationsAsync(int id, ChangeSet<EmployeeQualification> model, bool isDeleted);

        ChangeSet<EmployeeQualification> ManageEmployeeQualifications(int id, ChangeSet<EmployeeQualification> model, bool isDeleted);

        Task<ChangeSet<EmployeeEmergencyContact>> ManageEmployeeEmergencyContactsAsync(int id, ChangeSet<EmployeeEmergencyContact> model, bool isDeleted);

        ChangeSet<EmployeeEmergencyContact> ManageEmployeeEmergencyContacts(int id, ChangeSet<EmployeeEmergencyContact> model, bool isDeleted);

        Task<ChangeSet<EmployeeMedicalHistory>> ManageEmployeeMedicalHistoryAsync(int id, ChangeSet<EmployeeMedicalHistory> model, bool isDeleted);

        ChangeSet<EmployeeMedicalHistory> ManageEmployeeMedicalHistory(int id, ChangeSet<EmployeeMedicalHistory> model, bool isDeleted);

        Task<ChangeSet<EmployeeDependent>> ManageEmployeeDependentAsync(int id, ChangeSet<EmployeeDependent> model, bool isDeleted);

        ChangeSet<EmployeeDependent> ManageEmployeeDependent(int id, ChangeSet<EmployeeDependent> model, bool isDeleted);

        Task<ChangeSet<EmployeeDeclaration>> ManageEmployeeDeclarationAsync(int id, ChangeSet<EmployeeDeclaration> model, bool isDeleted);

        ChangeSet<EmployeeDeclaration> ManageEmployeeDeclaration(int id, ChangeSet<EmployeeDeclaration> model, bool isDeleted);

        Task<ChangeSet<EmployeeSkill>> ManageEmployeeSkillsAsync(int id, ChangeSet<EmployeeSkill> model, bool isDeleted);

        Task<EmployeeCompetency> ManageEmployeeCompetencyAsync(int id, EmployeeCompetency model);

        //Task<ChangeSet<PersonBGMapping>> ManageEmployeeBGCAsync(int id, ChangeSet<PersonBGMapping> model);

        ChangeSet<EmployeeSkill> ManageEmployeeSkills(int id, ChangeSet<EmployeeSkill> model, bool isDeleted);

        EmployeeCompetency ManageEmployeeCompetency(int id, EmployeeCompetency model);

        EmployeeCompetency ManageEmployeeSkillsDescription(int id, EmployeeCompetency model);

        Task<ChangeSet<EmployeeProfileViewModel>> UpdateEmployeeJoiningDetail(int id, ChangeSet<EmployeeProfileViewModel> model);

        Task<ChangeSet<EmployeeOrganizaionDetails>> UpdateEmployeeOrgDetail(int personId,int id, ChangeSet<EmployeeOrganizaionDetails> model);

        Task<Dictionary<string, List<DropdownItems>>> GetDropdowns(string moduleName);

        Task<IEnumerable<EmployeeApproval>> GetApprovalsList();

        Task<EmployeeApproval> GetApprovalById(int empID);

        Task<EmployeeApprovalViewModel> ApproveEmployeeDetails(string currentUserEmail, string stageID, int moduleID, int empID, int statusID, string comments);
        Task<Boolean> ApproveBulkEmployeeDetails(string currentUserEmail, int statusID, string comments, params int[] empIDs);
        Task<IEnumerable<EmployeeMyTeam>> GetMyTeamList(int id);
        Task<IEnumerable<EmployeeMyTeam>> GetAllEmpList(int id);
        Task<IEnumerable<EmployeeMyTeam>> GetMyTeamListDeep(int id);

        Task<IEnumerable<EmployeeMyTeam>> GetMyTeamHistoryList(int id); 
        Task<EmployeeBasicProfile> SearchEmployeeForAdminTasks(string query, bool showInActive);

        Task<EmployeeAdminHistory>  GetEmployeeAdminHistory(string query,int location, bool showInActive);

        Task<ChangeSet<EmployeeSittingLocation>> UpdateEmployeeSittingLocationDetail(int id, ChangeSet<EmployeeSittingLocation> model);
        Task<IEnumerable<EmployeeDesignation>> GetDesignation();
        IEnumerable<EmployeeWorkLocation> GetEmpWorkLocation();
        Task<IEnumerable<EmployeeWorkLocation>> GetEmpOfficeLocation(); 
        Task<IEnumerable<EmploymentStatusList>> GetEmploymentStatus();
        string GetEmployeeName(int id);
        string GetCommaSeparatedEmployeeNames(string ids);
        Task<Pheonix.Models.ViewModels.VPBUApproverHandlerModel> checkIsBUApprover(Pheonix.Models.ViewModels.VPBUApproverHandlerModel vPBUApproverVM);
        EmployeePreviousExperience AddEmployeeExperience(EmployeePreviousExperience model);
        EmployeePreviousExperience GetEmployeePastExperience(int id);
        Task<IEnumerable<CorrectionEmployee>> GetAllActiveEmpList();
        Task<IEnumerable<EmployeeProfileAttachmentDetails>> GetAttachmentRecordByApprovalId(int approvalId);
    }
}
