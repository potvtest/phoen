using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.VM.Classes.Candidate
{
    public class CandidatePassportViewModel
    {
        public int ID { get; set; }
        public string PassportNumber { get; set; }
        public string NameAsInPassport { get; set; }
        public string FatherNameAsInPasssport { get; set; }
        public string MotherNameAsInPassport { get; set; }
        public string SpouseNameAsInPassport { get; set; }
        public Nullable<System.DateTime> DateOfIssue { get; set; }
        public Nullable<System.DateTime> DateOfExpiry { get; set; }
        public string PlaceIssued { get; set; }
        public Nullable<int> BlankPagesLeft { get; set; }
        public Nullable<int> RelationWithPPHolder { get; set; }
        public Nullable<int> PPHolderDependentID { get; set; }
        public int CandidateID { get; set; }
        public bool IsDeleted { get; set; }
        public string PassportFileURL { get; set; }
    }
}
