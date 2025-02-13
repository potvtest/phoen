using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.VM
{
    public class EmployeeEmploymentHistory : IEmployeeEmploymentHistory
    {
        public int ID { get; set; }

        public string OrganisationName { get; set; }

        public string Location { get; set; }

        public DateTime? JoiningDate { get; set; }

        public DateTime? WorkedTill { get; set; }

        public string LastDesignation { get; set; }
        public string RoleDescription { get; set; }

        public bool IsDeleted { get; set; }

        public int StageStatusID { get; set; }

        public int SearchUserID { get; set; }
        public string EmploymentType { get; set; }
    }
}