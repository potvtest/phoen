namespace Pheonix.Models.ViewModels
{
    public class KRACycleConfiguration
    {
        public int PersonId { get; set; }
        public int KRAInitiationId { get; set; }
        public bool IsFreezed { get; set; }
        public bool Q1 { get; set; } = false;
        public bool Q2 { get; set; } = false;
        public bool Q3 { get; set; } = false;
        public bool Q4 { get; set; } = false;
        public int Year { get; set; }
        public int ReviewerPersonId { get; set; }
        public string ReviewerPersonName { get; set; }
    }
}
