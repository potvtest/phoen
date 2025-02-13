namespace Pheonix.Models.VM
{
    public interface IEmployeeSkill : IBaseModel
    {
        int SkillID { get; set; }
        string SkillName { get; set; }
        int SkillRating { get; set; }
        int ExperienceYears { get; set; }
        int ExperienceMonths { get; set; }
        bool HasCoreCompetency { get; set; }
        bool IsDeleted { get; set; }
    }
}