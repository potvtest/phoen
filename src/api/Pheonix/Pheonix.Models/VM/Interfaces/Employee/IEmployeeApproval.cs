using System.Collections.Generic;

namespace Pheonix.Models.VM
{
    public interface IEmployeeApproval
    {
        int ID { get; set; }
        string Name { get; set; }
        string OLText { get; set; }
        string Designation { get; set; }
        string OfficialEmail { get; set; }
        string ImagePath { get; set; }
        List<IEmployeeApprovalViewModel> EmployeeApprovalViewModelList { get; set; }
        List<int> ModuleIds { get; set; }
        List<int> Status { get; set; }
    }
}