namespace Pheonix.Models.VM
{
    public class EmployeeQualification : IEmployeeQualification
    {
        public string University { get; set; }
        public int QualificationID { get; set; }
        public string QualificationName { get; set; }
        public string Specialization { get; set; }
        public int? PassingYear { get; set; }
        public string Institute { get; set; }
        public string QualificationType { get; set; }
        public string Grade_Class { get; set; }
        public int? StatusId { get; set; }
        public int ID { get; set; }
        public bool IsDeleted { get; set; }
        public int StageStatusID { get; set; }
        public int SearchUserID { get; set; }
        public int ApprovalID { get; set; }
    }
}