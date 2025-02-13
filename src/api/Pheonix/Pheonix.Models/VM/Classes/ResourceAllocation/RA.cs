using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.VM.Classes.ResourceAllocation
{
    public class RA
    {
        public int RequestType { get; set; }
        public RAProject Project { get; set; }
        public List<RAResource> Resources { get; set; }
        public string Description { get; set; }       

    }
}
