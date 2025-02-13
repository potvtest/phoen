using System;
namespace Pheonix.Models.VM
{
    public interface IEmployeeBasicProfile
    {
        int ID { get; set; }
        string FirstName { get; set; }
        string MiddleName { get; set; }
        string LastName { get; set; }
        int? CurrentDesignationID { get; set; }
        string CurrentDesignation { get; set; }
        string PFNo { get; set; }
        string PANNo { get; set; }
        string Passport { get; set; }
        string ImagePath { get; }
        Nullable<System.DateTime> DateOfBirth { get; set; }

        int EmployeementStaus { get; set; }
        int GenderID { get; set; }

    }
}