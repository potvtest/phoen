namespace Pheonix.Models.ViewModels
{
    public class InvalidKRADetails
    {
        public int KRAId { get; set; }
        public bool IsValid { get; set; }
        public string Comments { get; set; }
        public int InvalidMarkedBy { get; set; }
        public int KRAGoalId { get; set; }
    }
}
