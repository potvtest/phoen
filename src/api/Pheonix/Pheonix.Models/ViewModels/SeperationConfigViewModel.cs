using Newtonsoft.Json;
using Pheonix.Models.ViewModels;
using System;
using System.Collections.Generic;

namespace Pheonix.Models
{
    public class SeperationConfigViewModel : IViewModel
    {
        public int ID { get; set; }
        public string ChecklistItem { get; set; }
        public int RoleID { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public int? StatusID { get; set; }
        public bool ChecklistType { get; set; }
        public string DeptRemark { get; set; }
        public int Sequence { get; set; }
    }

    public class SeperationConfigProcessViewModel
    {
        public int ID { get; set; }
        public bool IsReadOnly { get; set; }
        public string Comments { get; set; }
        public int RoleID { get; set; }
        public List<SeperationConfigViewModel> Data { get; set; }
        public bool isPrint { get; set; }
        public int letterType { get; set; }
        public int StatusID { get; set; }
        public DateTime ExitDate { get; set; }
        public int assignedTo { get; set; }
        public Boolean isHRRole { get; set; }
        public bool isChkNotComplete { get; set; }
        public bool isContainNotRequired { get; set; }
    }
}
