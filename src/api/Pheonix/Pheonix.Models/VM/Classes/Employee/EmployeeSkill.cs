namespace Pheonix.Models.VM
{
    public class EmployeeSkill : IEmployeeSkill
    {
        public int SkillID { get; set; }
        public string SkillName { get; set; }
        public int SkillRating { get; set; }
        public int ExperienceYears { get; set; }
        public int ExperienceMonths { get; set; }
        public bool HasCoreCompetency { get; set; }
        public int ID { get; set; }
        public bool IsDeleted { get; set; }
        public int StageStatusID { get; set; }
        public int SearchUserID { get; set; }
        public bool IsPrimary { get; set; }
    }
}