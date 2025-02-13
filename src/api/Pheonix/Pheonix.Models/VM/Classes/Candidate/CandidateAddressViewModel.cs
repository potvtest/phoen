using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.VM.Classes.Candidate
{
    public class CandidateAddressViewModel
    {
        public int ID { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string Pin { get; set; }
        public Nullable<bool> IsCurrent { get; set; }
        public int CandidateID { get; set; }
        public Nullable<bool> IsDeleted { get; set; }

    }
}
