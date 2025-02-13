using Newtonsoft.Json;
using Pheonix.Models.ViewModels;
using System;


namespace Pheonix.Models
{
    public class ProjectViewModel : IViewModel
    {
        public int ID { get; set; }
        public string ProjectCode { get; set; }
        public string ProjectName { get; set; }
        public string Description { get; set; }
        public Nullable<int> ProjectManager { get; set; }
        public Nullable<int> DeliveryManager { get; set; }
        public string ProjectManagerName { get; set; }
        public string DeliveryManagerName { get; set; }
        public System.DateTime ActualStartDate { get; set; }
        public System.DateTime ActualEndDate { get; set; }
        public string CustomerName { get; set; }
        public DateTime? CustomerStartDate { get; set; }
        public DateTime CustomerEndDate { get; set; }
        public int CustomerID { get; set; }
        public string Risks { get; set; }
        public int Active { get; set; }
        public Nullable<bool> Billable { get; set; }
        public Nullable<int> DeliveryUnit { get; set; }
        public Nullable<int> DeliveryTeam { get; set; }
        public Nullable<bool> IsExternal { get; set; }
        public Nullable<bool> IsOffshore { get; set; }
        public int ProjectType { get; set; }
        public int CreatedBy { get; set; }
        public Nullable<System.DateTime> CreatedOn { get; set; }
        public Nullable<bool> IsDeleted { get; set; }
        public Nullable<int> ParentProjId { get; set; }
        public string ProjectMethodology { get; set; }
        public string Process { get; set; }
        public string SprintDuration { get; set; }
        public bool IsChildPresent { get; set; }
        public string DelUnitName { get; set; }
        public string DelTeamName { get; set; }
    }


    public class ActionResult
    {
        public string message;
        public bool isActionPerformed = false;
        public int RecordID { get; set; }
    }
}
