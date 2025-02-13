using System;

namespace Pheonix.Models
{
    public class PersonCertificationViewModel
    {
        public int ID { get; set; }

        public int? CertificationID { get; set; }

        public int PersonID { get; set; }

        public DateTime? CertificationDate { get; set; }

        public string Grade { get; set; }

        public int? StatusId { get; set; }
    }
}