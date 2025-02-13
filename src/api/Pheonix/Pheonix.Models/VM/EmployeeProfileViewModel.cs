using Newtonsoft.Json;
using Pheonix.DBContext;
using Pheonix.Models.VM.Interfaces.Employee;
using System;
using System.Collections.Generic;

namespace Pheonix.Models.VM
{
    public class EmployeeProfileViewModel : IEmployeeProfile
    {
        public int ID
        {
            get;
            set;
        }

        //[JsonProperty(PropertyName = "field_one")]
        public string FirstName
        {
            get;
            set;
        }

        public string MiddleName
        {
            get;
            set;
        }

        public string LastName
        {
            get;
            set;
        }

        public string Salutation
        {
            get;
            set;
        }

        public bool Active
        {
            get;
            set;
        }

        public int ActiveDdl
        {
            get;
            set;
        }

        public int? CurrentDesignationID
        {
            get;
            set;
        }

        public string CurrentDesignation
        {
            get;
            set;
        }

        public string PFNo
        {
            get;
            set;
        }

        public string PANNo
        {
            get;
            set;
        }

        public string Passport
        {
            get;
            set;
        }

        public string ImagePath
        {
            get;
            set;
        }

        public string UserName
        {
            get;
            set;
        }

        public DateTime? DateOfBirth
        {
            get;
            set;
        }

        public int Gender
        {
            get;
            set;
        }

        public string MaritalStatus
        {
            get;
            set;
        }

        public DateTime? WeddingDate
        {
            get;
            set;
        }

        public string SpouseName
        {
            get;
            set;
        }

        public DateTime? SpouseBirthDate
        {
            get;
            set;
        }

        public int NoofChildren
        {
            get;
            set;
        }

        public string Hobbies
        {
            get;
            set;
        }

        public string FirstChildName
        {
            get;
            set;
        }

        public DateTime? FirstChildDateOfBirth
        {
            get;
            set;
        }

        public string PersonalEmail
        {
            get;
            set;
        }

        public string Mobile
        {
            get;
            set;
        }

        public string ResidenceNumber
        {
            get;
            set;
        }

        public string RRFNumber
        {
            get;
            set;
        }

        public bool IsRRF
        {
            get;
            set;
        }

        public DateTime? JoiningDate
        {
            get;
            set;
        }

        public DateTime? ConfirmationDate
        {
            get;
            set;
        }

        public DateTime? ProbationReviewDate
        {
            get;
            set;
        }

        public DateTime? ExitDate
        {
            get;
            set;
        }

        public bool RejoinedWithinYear
        {
            get;
            set;
        }

        public string RejoinedWithinYearDdl
        {
            get;
            set;
        }

        public string ReportingTo
        {
            get;
            set;
        }

        public string CompetencyManager
        {
            get;
            set;
        }

        public string ExitProcessManager
        {
            get;
            set;
        }

        public string OrganizationEmail
        {
            get;
            set;
        }

        public int OL
        {
            get;
            set;
        }

        public string OLText
        {
            get;
            set;
        }

        public string EmployeeType
        {
            get;
            set;
        }

        public string SeatingLocation
        {
            get;
            set;
        }

        public string UID
        {
            get;
            set;
        }

        public int? CompetencyID
        {
            get;
            set;
        }

        public string SkillDescription
        {
            get;
            set;
        }

        public IEmployeeManagerViewModel R1
        {
            get;
            set;
        }

        public IEmployeeManagerViewModel R2
        {
            get;
            set;
        }

        public IEnumerable<IEmployeeVisa> Visas
        {
            get;
            set;
        }

        public IEnumerable<IEmployeeQualification> Qualifications
        {
            get;
            set;
        }

        public IEnumerable<IEmployeeAddress> Addresses
        {
            get;
            set;
        }

        public IEnumerable<IEmployeeEmergencyContact> EmergencyContacts
        {
            get;
            set;
        }

        public IEnumerable<IEmployeeDependent> Dependents
        {
            get;
            set;
        }

        public IEnumerable<IEmployeeSkill> Skills
        {
            get;
            set;
        }

        public IEnumerable<IEmployeeSkill> PrimarySkills
        {
            get;
            set;
        }

        public IEnumerable<IEmployeeSkill> SecondarySkills
        {
            get;
            set;
        }

        public IEnumerable<IEmployeeDeclaration> Declarations
        {
            get;
            set;
        }

        public IEnumerable<IEmployeeCertification> Certifications
        {
            get;
            set;
        }

        public IEnumerable<IEmployeeEmploymentHistory> EmployeeHistories
        {
            get;
            set;
        }

        public IEnumerable<IEmployeeMedicalHistory> EmployeeMedicalHistories
        {
            get;
            set;
        }

        public IEnumerable<IEmployeePassport> EmployeePassport
        {
            get;
            set;
        }

        public IEmployeeOrganizationDetails EmployeeOrganizationdetails
        {
            get;
            set;
        }

        public int[] Cards { get; set; }
        public int[] Approvals { get; set; }

        public List<int> SendForApprovals { get; set; }

        public IEnumerable<IEmployeeRole> Role { get; set; }

        public List<PersonBGMapping> PersonBGMapping { get; set; }

        public IEnumerable<IEmploymentHelpDeskCategories> HelpDeskCategories { get; set; }

        public string extension { get; set; }

        public string location { get; set; }

        public string employmentStatus { get; set; }

        public string Commitment { get; set; }


        public bool IsConfirmationHistoryPresent { get; set; }

        public string bloodGroup { get; set; }

        public int SearchUserID { get; set; }
        public int employmentStatusID { get; set; }

        public int EmployeementStaus { get; set; }
        public int GenderID { get; set; }
        public int grade { get; set; }
    }
}