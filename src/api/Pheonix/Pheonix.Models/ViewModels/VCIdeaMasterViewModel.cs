using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.ViewModels
{
    public class VCIdeaMasterViewModel
    {
        
        public long ID { get; set; }
        public string IdeaHeadline { get; set; }
        public string IdeaDescription { get; set; }
        public string IdeaBenefits { get; set; }
        public string RequiredEffort { get; set; }
        public string RequiredResources { get; set; }
        public string RequiredTechnologies { get; set; }
        public int SubmittedBy { get; set; }
        public Nullable<int> UpdatedBy { get; set; }
        public string ExecutionApproach { get; set; }
        public bool IsEmailReceiptRequired { get; set; }
        public short StatusID { get; set; }
        public string StatusName { get; set; }
        public short PriorityID { get; set; }
        public string PriorityName { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public System.DateTime LastUpdatedDate { get; set; }
        public bool IsDeleted { get; set; }
        public string SubmittedByName { get; set; }
        public string UniquenessQuotient { get; set; }
        public short BusinessUnit { get; set; }
        public string BusinessUnitName { get; set; }
        public float Target { get; set; }
        public double Benefit { get; set; }
        public string BenefitValue { get; set; }
        public double Cost { get; set; }
        public string CostDesc { get; set; }
        public int FinalScore { get; set; }
        public string UserComment { get; set; }
        public int ReviewerId { get; set; }
        public Pheonix.Models.VM.EmployeeProfileViewModel SubmittedByDetails { get; set; }
        public  ICollection<VPCommentsViewModel> VPComments { get; set; }
        public string TeammemberIds { get; set; }
        public string TeammemberNames { get; set; }
        //  public virtual ICollection<VPCommentsViewModel> VPComments { get; set; }
        public string IsLockedBy { get; set; }
        public short Stage { get; set; }
        public string BenefitFactor { get; set; }
        public int BenefitScope { get; set; }
        public string benefitScopeValue { get; set; }
    }
}
