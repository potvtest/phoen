using System;

namespace Pheonix.Models
{
    public class PersonDependentViewModel
    {
        public int ID { get; set; }

        public string Name { get; set; }

        public int? Relation { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public int PersonID { get; set; }
    }
}