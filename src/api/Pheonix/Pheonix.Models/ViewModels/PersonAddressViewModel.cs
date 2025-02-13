namespace Pheonix.Models
{
    public class PersonAddressViewModel
    {
        public int ID { get; set; }

        public string Address { get; set; }

        public string City { get; set; }

        public string State { get; set; }

        public string Country { get; set; }

        public string Pin { get; set; }

        public bool? IsCurrent { get; set; }

        public bool? HouseOnRent { get; set; }

        public bool? ProofSubmitted { get; set; }

        public int? ProofLocation { get; set; }

        public int PersonID { get; set; }
    }
}