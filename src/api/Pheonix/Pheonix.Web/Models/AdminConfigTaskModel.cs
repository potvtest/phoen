using Pheonix.Models.Models.AdminConfig;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Pheonix.Web.Models
{
    public class AdminConfigTaskModel
    {
        public int ID { get; set; }
        public int HolidayYear { get; set; }
        public int Location { get; set; }
        public List<HolidayListModel> details { get; set; }
        public List<AdminLeaveConfigModel> leaves { get; set; }
        public List<LocationListModel> locations { get; set; }
        //public List<AdminBGCConfigModel> bgParameterList { get; set; }
        public List<VCFListModel> vCFDetails { get; set; }
        public List<VCFApproverModel> vcfApprover { get; set; }
        public List<DeliveryTeamModel> deliveryTeam { get; set; }
        public List<ResourcePoolModel> resourcePool { get; set; }
        public List<VCFGlobalApproverModel> vcfGlobalApproversList { get; set; }
        public List<SkillsModel> skills { get; set; }
        public List<TaskTypeModel> taskType { get; set; }
    }
}