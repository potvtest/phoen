namespace Pheonix.Models
{
    public class PersonMedicalHistoryViewModel
    {
        public int ID { get; set; }

        public string Description { get; set; }

        public int PersonID { get; set; }

        public int? Year { get; set; }
    }
}