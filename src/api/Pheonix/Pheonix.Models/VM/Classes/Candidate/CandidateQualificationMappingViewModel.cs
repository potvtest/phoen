using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.VM.Classes.Candidate
{
    public class CandidateQualificationMappingViewModel
    {
        public int ID { get; set; }
        public int CandidateID { get; set; }
        public Nullable<int> QualificationID { get; set; }
        public string Specialization { get; set; }
        public string Institute { get; set; }
        public string University { get; set; }
        public Nullable<int> Year { get; set; }
        public string QualificationType { get; set; }
        public string Percentage { get; set; }
        public string GradeClass { get; set; }
        public Nullable<int> StatusId { get; set; }
        public bool IsDeleted { get; set; }
    }
}
