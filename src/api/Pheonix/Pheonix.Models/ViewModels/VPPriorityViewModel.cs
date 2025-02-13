using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.ViewModels
{
    public class VPPriorityViewModel
    {
        public short ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }

      //  public ICollection<VPStatusViewModel> StatusCollection { get; set; }
    }
}
