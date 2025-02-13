namespace Pheonix.Models.VM
{
    public class EmployeeMedicalHistory : IEmployeeMedicalHistory
    {
        public int ID { get; set; }
        public string Description { get; set; }
        public int Year { get; set; }
        public bool IsDeleted { get; set; }
        public int StageStatusID { get; set; }
        public int SearchUserID { get; set; }
        public int ApprovalID { get; set; }
    }
}