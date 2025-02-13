using System;
using System.Collections.Generic;

namespace Pheonix.Models.VM
{
    public interface IEmployeePersonalDetails
    {
        string UserName { get; set; }
        DateTime? DateOfBirth { get; set; }
        int Gender { get; set; }
        string MaritalStatus { get; set; }
        DateTime? WeddingDate { get; set; }
        string SpouseName { get; set; }
        DateTime? SpouseBirthDate { get; set; }
        int NoofChildren { get; set; }
        string Hobbies { get; set; }
        string FirstChildName { get; set; }
        DateTime? FirstChildDateOfBirth { get; set; }
        string PersonalEmail { get; set; }
        string Mobile { get; set; }
        string ResidenceNumber { get; set; }
        string FirstName { get; set; }
        string MiddleName { get; set; }
        string LastName { get; set; }

        IEnumerable<IEmployeeVisa> Visas { get; set; }
        IEnumerable<IEmployeeQualification> Qualifications { get; set; }
        IEnumerable<IEmployeeAddress> Addresses { get; set; }
        IEnumerable<IEmployeeEmergencyContact> EmergencyContacts { get; set; }
        IEnumerable<IEmployeeDependent> Dependents { get; set; }
        IEnumerable<IEmployeeCertification> Certifications { get; set; }
        IEnumerable<IEmployeeEmploymentHistory> EmployeeHistories { get; set; }
        IEnumerable<IEmployeeMedicalHistory> EmployeeMedicalHistories { get; set; }
        IEnumerable<IEmployeePassport> EmployeePassport { get; set; }
    }
}