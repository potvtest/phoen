using System;

namespace Pheonix.Models
{
    public class PersonPassportViewModel
    {
        public int ID { get; set; }

        public string PassportNumber { get; set; }

        public string NameAsInPassport { get; set; }

        public string FatherNameAsInPasssport { get; set; }

        public string MotherNameAsInPassport { get; set; }

        public string SpouseNameAsInPassport { get; set; }

        public DateTime? DateOfIssue { get; set; }

        public DateTime? DateOfExpiry { get; set; }

        public string PlaceIssued { get; set; }

        public int? BlankPagesLeft { get; set; }

        public int? PassportHolderRelation { get; set; }

        public int PersonID { get; set; }

        public int? RelationWithPPHolder { get; set; }

        public int? PPHolderDependentID { get; set; }
    }
}