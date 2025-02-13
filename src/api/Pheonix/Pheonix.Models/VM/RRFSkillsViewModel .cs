using Pheonix.Models.VM.Interfaces.RRF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.VM
{
    public class RRFSkillsViewModel : IRRFSkills
    {
        public int ID { get; set; }
        public int SkillId { get; set; }
        public int Rating { get; set; }
        public int action { get; set; }
    }
}
