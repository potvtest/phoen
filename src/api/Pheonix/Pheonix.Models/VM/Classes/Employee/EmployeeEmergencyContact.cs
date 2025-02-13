namespace Pheonix.Models.VM
{
    public class EmployeeEmergencyContact : IEmployeeEmergencyContact
    {
        public string ContactPersonName { get; set; }
        public int Relation { get; set; }
        public string EmergencyContactNo { get; set; }
        public string EmergencyEmail { get; set; }
        public int ID { get; set; }
        public bool IsDeleted { get; set; }
        public int StageStatusID { get; set; }
        public string ContactAddress { get; set; }
        public int SearchUserID { get; set; }
        public int ApprovalID { get; set; }
    }
}