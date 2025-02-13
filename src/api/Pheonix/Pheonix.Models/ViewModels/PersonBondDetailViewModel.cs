using System;

namespace Pheonix.Models
{
    public class PersonBondDetailViewModel
    {
        public int ID { get; set; }

        public long? BondAmount { get; set; }

        public int? BondType { get; set; }

        public bool? Status { get; set; }

        public DateTime? BondCompletionDate { get; set; }

        public int PersonID { get; set; }
    }
}