using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.ViewModels
{
  public class VPAllMastersdataViewModel
    {
        public ICollection<VPPriorityViewModel> PriorityCollection { get; set; }
        public ICollection<VPStatusViewModel> StatusCollection { get; set; }
        public ICollection<VPBenefitViewModel> BenefitCollection { get; set; }
        public ICollection<VPCostViewModel> CostCollection { get; set; }
        public ICollection<VPBenefitScopeViewModel> ScopeCollection { get; set; }
    }
}
