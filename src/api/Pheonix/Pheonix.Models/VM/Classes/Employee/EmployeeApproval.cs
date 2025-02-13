using System.Collections.Generic;

namespace Pheonix.Models.VM
{
    public class EmployeeApproval : IEmployeeApproval
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string OLText { get; set; }
        public string Designation { get; set; }
        public string OfficialEmail { get; set; }
        public string ImagePath { get; set; }
        public List<IEmployeeApprovalViewModel> EmployeeApprovalViewModelList { get; set; }
        public List<int> ModuleIds { get; set; }
        public string SeatingLocation { get; set; }
        public string Extension { get; set; }
        public List<int> Status { get; set; }
    }
}