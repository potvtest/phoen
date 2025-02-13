using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.ViewModels
{
   public class VPConfigurationViewModel
    {
        public short ID { get; set; }
        public string Name { get; set; }
        public Nullable<int> Value { get; set; }
        public string Description { get; set; }
        public Nullable<bool> IsActive { get; set; }
    }
}
