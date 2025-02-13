namespace Pheonix.Models.ViewModels
{
    public class PersonKRAViewModel
    {
        public int PersonKRAInitiationId { get; set; }
        public int PersonId { get; set; }
        public int KRAYearId { get; set; }
        public bool Q1 { get; set; }
        public bool Q2 { get; set; }
        public bool Q3 { get; set; }
        public bool Q4 { get; set; }
        public int ReviewerPersonId { get; set; }
        public int FirstKRAInitiatedBy { get; set; }
        public int SecondKRAInitiatedBy { get; set; }
        public int ThirdKRAInitiatedBy { get; set; }
        public int FourthKRAInitiatedBy { get; set; }
    }
}
