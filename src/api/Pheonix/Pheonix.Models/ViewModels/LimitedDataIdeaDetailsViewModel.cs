using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.ViewModels
{
    public class LimitedDataIdeaDetailsViewModel
    {
        public long ID { get; set; }
        public string IdeaHeadline { get; set; }
        public string IdeaDescription { get; set; }
        public string RequiredEffort { get; set; }
        //public string RequiredResources { get; set; }
        public string RequiredTechnologies { get; set; }
        public int SubmittedBy { get; set; }
        public string SubmittedByName { get; set; }
        public Pheonix.Models.VM.EmployeeProfileViewModel SubmittedByDetails { get; set; }
        public ICollection<VPCommentsViewModel> VPComments { get; set; }
        public string TeammemberIds { get; set; }
        public string TeammemberNames { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public bool IsDeleted { get; set; }
        public short StatusID { get; set; }
        public string StatusName { get; set; }
         public short BusinessUnit { get; set; }
        public string BenefitFactor { get; set; }
        public int BenefitScope { get; set; }
    }
}