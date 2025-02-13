using Pheonix.DBContext;
using Pheonix.Models.VM.Interfaces.RRF;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.VM
{
    public class RRFReadOnlyViewModel
    {
        public int ID { get; set; }
        public string RRFNumber { get; set; }
        public DateTime? RequestedDate { get; set; }
        public int Positions { get; set; }
        public string SkillCategory { get; set; }
        public DateTime? ClosureDate { get; set; }
        public int Experience { get; set; }
        public string Designation { get; set; }
        public int EmploymentType { get; set; }
        public string RequestedBy { get; set; }
        public int RRFStatus { get; set; }
        public int RRFStage { get; set; }
        public string PrimarySkills { get; set; }
    }

    public class RRFViewModel
    {
        public int ID { get; set; }
        public string RRFNumber { get; set; }
        public DateTime? RequestedDate { get; set; }
        public int Positions { get; set; }
        public int SkillCategory { get; set; }
        public DateTime? ClosureDate { get; set; }
        public int Designation { get; set; }
        public string RequestedBy { get; set; }
        public int RequestedByID { get; set; }
        public int RRFStatus { get; set; }
    }


    public class AddEditRRFViewModel
    {
        public RRFViewModel RRF { get; set; }
        public RRFDetailsViewModels RRFDetail { get; set; }
        public List<RRFCommentViewModel> RRFComments { get; set; }
        public List<RRFSkillsViewModel> RRFSkill { get; set; }
    }
}