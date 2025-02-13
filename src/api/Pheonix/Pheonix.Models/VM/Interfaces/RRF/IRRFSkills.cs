using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.VM.Interfaces.RRF
{
    public interface IRRFSkills
    {
        int ID { get; set; }
        int SkillId { get; set; }
        int Rating { get; set; }
        int action { get; set; }
    }
}