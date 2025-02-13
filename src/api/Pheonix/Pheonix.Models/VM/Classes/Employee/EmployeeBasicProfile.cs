using System;
namespace Pheonix.Models.VM
{
    public class EmployeeBasicProfile : IEmployeeBasicProfile
    {
        public int ID { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public int? CurrentDesignationID { get; set; }
        public string CurrentDesignation { get; set; }
        public string PFNo { get; set; }
        public string PANNo { get; set; }
        public string Passport { get; set; }
        public string ImagePath { get; set; }
        public string Email { get; set; }
        public string PersonalEmail { get; set; }
        public string Mobile { get; set; }
        public int OL { get; set; }
        public string OLText { get; set; }
        public Nullable<int> OfficeLocation { get; set; }
        public string SeatingLocation { get; set; }
        public string ResidenceNumber { get; set; }
        public bool Active { get; set; }
        public Nullable<System.DateTime> DateOfBirth { get; set; }
        public int LeavesRemaining { get; set; }
        public string Extension { get; set; }
        public DateTime joiningDate { get; set; }
        public DateTime probationReviewDate { get; set; }
        public string SkillName { get; set; }
        public int SkillRating { get; set; }
        public string ResourcePoolName { get; set; }
        public string DeliveryUnitName { get; set; }
        public string DeliveryTeamName { get; set; }
        public DateTime? exitDate { get; set; }
        public int CasualLeavesRemaining { get; set; }
        public int SickLeavesRemaining { get; set; }

        public int EmployeementStaus { get; set; }
        public int GenderID { get; set; }
    }
}