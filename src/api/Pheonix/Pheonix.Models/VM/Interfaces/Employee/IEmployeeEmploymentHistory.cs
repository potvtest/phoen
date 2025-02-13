using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.VM
{
    public interface IEmployeeEmploymentHistory : IBaseModel
    {
        string OrganisationName { get; set; }
        string Location { get; set; }
        DateTime? JoiningDate { get; set; }
        DateTime? WorkedTill { get; set; }
        string LastDesignation { get; set; }
        string RoleDescription { get; set; }        
        bool IsDeleted { get; set; }
        string EmploymentType { get; set; }
    }
}
