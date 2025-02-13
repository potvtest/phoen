using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.VM.Classes.Candidate
{
    public class CandidateCertificationViewModel
    {
        public int ID { get; set; }
        public Nullable<int> CertificationID { get; set; }
        public int CandidateID { get; set; }
        public Nullable<System.DateTime> CertificationDate { get; set; }
        public string CertificationNumber { get; set; }
        public string Grade { get; set; }
        public Nullable<int> StatusId { get; set; }
        public bool IsDeleted { get; set; }
    }
}
