using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models
{
    public class PersonQualificationMappingViewModel
    {
        public int ID { get; set; }

        public int PersonID { get; set; }

        public int QualificationID { get; set; }

        public string Specialization { get; set; }

        public string Institute { get; set; }

        public string University { get; set; }

        public int? Year { get; set; }

        public string QualificationType { get; set; }

        public string Percentage { get; set; }

        public string Grade_Class { get; set; }

        public int? StatusId { get; set; }
    }
}
