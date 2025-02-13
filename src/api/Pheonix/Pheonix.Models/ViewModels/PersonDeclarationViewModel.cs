using System;

namespace Pheonix.Models
{
    public class PersonDeclarationViewModel
    {
        public int ID { get; set; }

        public string Name { get; set; }

        public string RelationType { get; set; }

        public DateTime? BirthDate { get; set; }

        public int PersonID { get; set; }
    }
}