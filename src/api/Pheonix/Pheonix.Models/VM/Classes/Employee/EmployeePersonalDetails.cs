using System;
using System.Collections.Generic;

namespace Pheonix.Models.VM
{
    public class EmployeePersonalDetails : IEmployeePersonalDetails
    {
        public string UserName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public int Gender { get; set; }
        public string MaritalStatus { get; set; }
        public DateTime? WeddingDate { get; set; }
        public string SpouseName { get; set; }
        public DateTime? SpouseBirthDate { get; set; }
        public int NoofChildren { get; set; }
        public string Hobbies { get; set; }
        public string FirstChildName { get; set; }
        public DateTime? FirstChildDateOfBirth { get; set; }
        public string PersonalEmail { get; set; }
        public string Mobile { get; set; }
        public string ResidenceNumber { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string PANNo { get; set; }
        public string BloodGroup { get; set; }
        public string UID { get; set; }
        public IEnumerable<IEmployeeVisa> Visas { get; set; }
        public IEnumerable<IEmployeeQualification> Qualifications { get; set; }
        public IEnumerable<IEmployeeAddress> Addresses { get; set; }
        public IEnumerable<IEmployeeEmergencyContact> EmergencyContacts { get; set; }
        public IEnumerable<IEmployeeDependent> Dependents { get; set; }
        public IEnumerable<IEmployeeCertification> Certifications { get; set; }
        public IEnumerable<IEmployeeEmploymentHistory> EmployeeHistories { get; set; }
        public IEnumerable<IEmployeeMedicalHistory> EmployeeMedicalHistories { get; set; }
        public IEnumerable<IEmployeePassport> EmployeePassport { get; set; }

        public int SearchUserID { get; set; }
        public int ApprovalID { get; set; }
    }
}