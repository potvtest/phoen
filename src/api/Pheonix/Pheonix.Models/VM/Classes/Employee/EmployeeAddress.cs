namespace Pheonix.Models.VM
{
    public class EmployeeAddress : IEmployeeAddress
    {
        public string AddressLabel { get; set; }
        public string CurrentAddress { get; set; }
        public string CurrentAddressCountry { get; set; }
        public bool IsCurrent { get; set; }
        public int ID { get; set; }
        public int StageStatusID { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Pin { get; set; }
        public int SearchUserID { get; set; }
        public int ApprovalID { get; set; }
    }
}