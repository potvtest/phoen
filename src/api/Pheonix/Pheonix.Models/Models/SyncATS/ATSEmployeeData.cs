using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models
{
        public class ATSEmployeeData
        {
            public int ApplicantId { get; set; }
            public string ApplicantEmail { get; set; }
            public DateTime ApplicantCreatedDate { get; set; }
            public DateTime ApplicantUpdatedDate { get; set; }
            //public Applicantpifdetails ApplicantPIFDetails { get; set; }
            public string ApplicantFirstName { get; set; }
            public string ApplicantLastName { get; set; }
            public string ApplicantMobile1 { get; set; }
            public string ApplicantMobile2 { get; set; }
            public string ApplicantAddress { get; set; }
            public string ApplicantCurrentLocation { get; set; }
            public string ApplicantCurrentCompany { get; set; }
            public string ApplicantCurrentDesignation { get; set; }
            public DateTime ApplicantDob { get; set; }
            public string ApplicantMaritalStatus { get; set; }
            public string ApplicantGender { get; set; }
            public string ApplicantPermanentAddress { get; set; }
            public string ApplicantNationality { get; set; }
            public Applicanteducationdetail[] ApplicantEducationDetails { get; set; }
            public int ApplicantCVId { get; set; }
            public string ApplicantCVFileReference { get; set; }
            public float ApplicantTotalExperience { get; set; }
            public int ApplicationId { get; set; }
            public int ApplicationRequisitionId { get; set; }
            public int ApplicationStatusId { get; set; }
            public int ApplicationRecruiterEmployeeId { get; set; }
            public string ApplicationRecruiterEmployeeReference { get; set; }
            public string ApplicationRecruiterName { get; set; }
            public DateTime ApplicationCreatedDate { get; set; }
            public DateTime ApplicationUpdatedDate { get; set; }
            public int ApplicationStepId { get; set; }
            public string ApplicationJoiningDate { get; set; }
            public int ApplicationSourcerEmployeeId { get; set; }
            public object ApplicationSourcerEmployeeReference { get; set; }
            public string ApplicationSourcerName { get; set; }
            public bool ApplicationOnHold { get; set; }
            public float ApplicationJdMatchScore { get; set; }
            public int ApplicationSourceId { get; set; }
            public int ApplicationSourceCategoryId { get; set; }
            public string ApplicationSourceName { get; set; }
            public string ApplicationSourceCategory { get; set; }
            public DateTime ApplicationLastStatusChangeDate { get; set; }
            public string ApplicationOfferAcceptanceStatus { get; set; }
            public string ApplicationOfferAcceptanceComment { get; set; }
            public Nullable<DateTime> ApplicationOfferReleasedDate { get; set; }
            public object ApplicationOfferAcceptanceDate { get; set; }
            public Nullable<DateTime> ApplicationOfferedDate { get; set; }
            public object ApplicationRejectionCode { get; set; }
            public int ApplicationRejectionFromStepId { get; set; }
            public object ApplicationRejectionReasonCategory { get; set; }
            public Applicationscreeningquesansw ApplicationScreeningQuesAnsw { get; set; }
            public Nullable<decimal> ApplicationRelevantExp { get; set; }
            public int ApplicationNoticePeriod { get; set; }
            public float ApplicationCtcOffered { get; set; }
            public Nullable<decimal> ApplicationCurrentCtc { get; set; }
            public float ApplicationExpectedCtc { get; set; }
            public string ApplicationUUID { get; set; }
            public Applicationcustomofferfields ApplicationCustomOfferFields { get; set; }
            public float ApplicationFixedSalaryComponent { get; set; }
            public float ApplicationVariableSalaryComponent { get; set; }
            public string ApplicationPreferredLocation { get; set; }
            public object ApplicationScreeningCustomFields { get; set; }
            public int AgencyFee { get; set; }
            public string ApplicationScreeningComments { get; set; }
            public string RequisitionLocation { get; set; }
            public string PositionType { get; set; }
            public Requisitioncustomfields RequisitionCustomFields { get; set; }
            public string EmploymentType { get; set; }
            public string BuName { get; set; }
            public string BuHierarchy { get; set; }
            public int BuId { get; set; }
            public string BuReference { get; set; }
            public string RequisitionTitle { get; set; }
            public object RequisitionReference { get; set; }
            public int RequisitionId { get; set; }
            public string RequisitionDesignation { get; set; }
            public int HiringManagerEmployeeId { get; set; }
            public object HiringManagerEmployeeReference { get; set; }
            public string HiringManagerName { get; set; }
            public string HiringManagerEmail { get; set; }
        }

        public class Applicantpifdetails
        {
            public Personal Personal { get; set; }
            public object[] Family { get; set; }
            public Emergency[] Emergency { get; set; }
            public Contact Contact { get; set; }
            public Identity Identity { get; set; }
            public Education[] Education { get; set; }
            public Employment[] Employment { get; set; }
        }

        public class Personal
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Dob { get; set; }
            public string Gender { get; set; }
            public string MaritalStatus { get; set; }
            public string Nationality { get; set; }
            public string BloodGroup { get; set; }
        }

        public class Contact
        {
            public string CurrentAddress { get; set; }
            public string PermanentAddress { get; set; }
            public string Email1 { get; set; }
            public string Email2 { get; set; }
            public string Phone1 { get; set; }
            public string Phone2 { get; set; }
        }

        public class Identity
        {
            public Pan_Card Pan_card { get; set; }
            public Passport Passport { get; set; }
            public Driving_License Driving_license { get; set; }
            public Aadhaar_Card Aadhaar_card { get; set; }
        }

        public class Pan_Card
        {
            public string Id { get; set; }
            public Nullable<DateTime> Valid_until { get; set; }
        }

        public class Passport
        {
            public string Id { get; set; }
            public Nullable<DateTime> Valid_until { get; set; }
        }

        public class Driving_License
        {
            public string Id { get; set; }
            public Nullable<DateTime> Valid_until { get; set; }
        }

        public class Aadhaar_Card
        {
            public string Id { get; set; }
            public Nullable<DateTime> Valid_until { get; set; }
        }

        public class Emergency
        {
            public string Relation { get; set; }
            public string Name { get; set; }
            public string Contact { get; set; }
            public string Email { get; set; }
            public string Address { get; set; }
        }

        public class Education
        {
            public string Degree { get; set; }
            public string Institute { get; set; }
            public float Marks { get; set; }
            public int Year { get; set; }
        }

        public class Employment
        {
            public string Company { get; set; }
            public DateTime FromDate { get; set; }
            public DateTime ToDate { get; set; }
            public string Designation { get; set; }
            public bool CurrentEmployer { get; set; }
        }

        public class Applicationscreeningquesansw
        {
            public string[] Diduworkonangular2 { get; set; }
            public string[] HowmanyyearsinPHPdevelopment { get; set; }
        }

        public class Applicationcustomofferfields
        {
            public string Revised_ctc { get; set; }
            public string Hra { get; set; }
            public string Pf { get; set; }
            public string Conv_allow { get; set; }
            public string Basic_salary { get; set; }
            public string Education_allow { get; set; }
            public string Medical_allow { get; set; }
        }

        public class Requisitioncustomfields
        {
        }

        public class Applicanteducationdetail
        {
            public string Id { get; set; }
            public string Year { get; set; }
            public string Marks { get; set; }
            public string Institute { get; set; }
            public string Degree { get; set; }
            public string UniversityLocation { get; set; }
            public string UniversityName { get; set; }
            public object EducationMode { get; set; }
            public string Level { get; set; }
        }
}
