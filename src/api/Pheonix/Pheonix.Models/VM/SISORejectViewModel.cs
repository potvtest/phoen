namespace Pheonix.Models.VM
{
    public class SISORejectViewModel
    {
        public string IDs { get; set; }
        public string RejectComment { get; set; }
        public bool IsBulkApprovals { get; set; } = false;
    }
}