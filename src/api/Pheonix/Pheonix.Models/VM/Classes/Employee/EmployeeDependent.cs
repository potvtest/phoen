using System;

namespace Pheonix.Models.VM
{
    public class EmployeeDependent : IEmployeeDependent
    {
        public string DependentName
        {
            get;
            set;
        }

        public int RelationWithDependent
        {
            get;
            set;
        }

        public DateTime? DateOfBirthOfDependent
        {
            get;
            set;
        }

        public int Age
        {
            get;
            set;
        }

        public int ID
        {
            get;
            set;
        }

        public bool IsDeleted
        {
            get;
            set;
        }

        public int StageStatusID
        {
            get;
            set;
        }

        public int SearchUserID { get; set; }
    }
}