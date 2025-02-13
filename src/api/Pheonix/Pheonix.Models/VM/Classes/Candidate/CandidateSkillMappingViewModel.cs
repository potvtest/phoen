using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.VM.Classes.Candidate
{
    public class CandidateSkillMappingViewModel
    {
        public int ID { get; set; }
        public int SkillID { get; set; }
        public int CandidateID { get; set; }
        public int ExperienceYears { get; set; }
        public Nullable<int> ExperienceMonths { get; set; }
        public bool HasCoreCompetency { get; set; }
        public int SkillRating { get; set; }
        public bool IsDeleted { get; set; }
    }
}
