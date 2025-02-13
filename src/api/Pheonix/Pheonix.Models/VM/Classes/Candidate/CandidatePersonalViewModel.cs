using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.VM.Classes.Candidate
{
    public class CandidatePersonalViewModel
    {
        public int ID { get; set; }
        public Nullable<int> HighestQualification { get; set; }
        public bool IsValidPassport { get; set; }
        public bool IsUSVisa { get; set; }
        public bool ReadytoRelocate { get; set; }
        public string MaritalStatus { get; set; }
        public Nullable<System.DateTime> WeddingDate { get; set; }
        public string SpouseName { get; set; }
        public string Phone { get; set; }
        public string Mobile { get; set; }
        public string AlternateContactNo { get; set; }
        public string PersonalEmail { get; set; }
        public string AlternateEmail { get; set; }
        public string Hobbies { get; set; }
        public int CandidateID { get; set; }
        public bool IsDeleted { get; set; }
    }
}
