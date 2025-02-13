using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.Models.AdminConfig
{
    public class AdminConfigActionModel
    {
        public int ID { get; set; }
        public int HolidayYear { get; set; }
        public int Location { get; set; }
        public AdminConfigTaskType ActionType { get; set; }
        public List<HolidayListModel> Details { get; set; }
        public List<AdminLeaveConfigModel> leaves { get; set; }
        public List<LocationListModel> locations { get; set; }
        //public List<AdminBGCConfigModel> bgParameterList { get; set; }
        public List<VCFListModel> VCFDetails { get; set; }
        public List<VCFApproverModel> vcfApprover { get; set; }
        public List<DeliveryTeamModel> deliveryTeam { get; set; }
        public List<ResourcePoolModel> resourcePool { get; set; }
        public List<VCFGlobalApproverModel> vcfGlobalApproversList { get; set; }
        public List<SkillsModel> skills { get; set; }
        public List<TaskTypeModel> taskType { get; set; }
    }

    public class AdminLeaveConfigModel
    {
        public string ConfigKey { get; set; }
        public string ConfigValue { get; set; }
        public string Description { get; set; }
        public int Year { get; set; }
        public int Location { get; set; }
        public Nullable<bool> Active { get; set; }
        public bool IsDeleted { get; set; }
    }

    public class HolidayListModel
    {
        public DateTime Date { get; set; }
        public string Description { get; set; }
        public int HolidayType { get; set; }
    }

    public class LocationListModel
    {
        public Nullable<int> ID { get; set; }
        public string LocationName { get; set; }
        public Nullable<int> ParentLocation { get; set; }
        public bool IsDeleted { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get { return DateTime.Now; } set { value = DateTime.Now; } }
    }

    public class VCFApproverModel
    {
        public Nullable<int> id { get; set; }
        public long DeliveryUnitID { get; set; }
        public int ReviewerId { get; set; }
        public int IsDeleted { get; set; }
       
    }
    //public class AdminBGCConfigModel
    //{
    //    public Nullable<int> ID { get; set; }
    //    public string Name { get; set; }
    //    public string Description { get; set; }
    //    public bool Active { get; set; }
    //    public bool IsDeleted { get; set; }
    //}
    public class VCFListModel
    {
        public Nullable<int> ID { get; set; }
        public string Description { get; set; }
        public string Name { get; set; }
        public bool IsDeleted { get; set; }
    }
    public class DeliveryTeamModel
    {
        public int ID { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public Nullable<int> ResourceHead { get; set; }
        public Nullable<bool> IsActive { get; set; }
        public Nullable<bool> IsDeleted { get; set; }
    }
    public class ResourcePoolModel
    {
        public int ID { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsDeleted { get; set; }
    }
    public class VCFGlobalApproverModel
    {
        public Nullable<int> ID { get; set; }
        public int PersonID { get; set; }
        public int RoleID { get; set; }
        public bool IsDeleted { get; set; }
    }

    public class SkillsModel
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public bool Active { get; set; }
        public int SkillCategory { get; set; }
        public bool IsDeleted { get; set; }
    }
    
    public class TaskTypeModel
    {
        public int id { get; set; }
        public int parentTaskId { get; set; }
        public string typeName { get; set; }
    }

    public enum AdminConfigTaskType
    {
        HolidayList,
        Leaves,
        Locations,
        BGC,
        VCF,
        VCFApprover,
        DeliveryTeam,
        ResourcePool,
        VCFGlobalApprover,
        Skills,
        TaskType
    }
}
