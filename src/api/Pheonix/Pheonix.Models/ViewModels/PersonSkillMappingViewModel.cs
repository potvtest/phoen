namespace Pheonix.Models
{
    public class PersonSkillMappingViewModel
    {
        public int ID { get; set; }

        public int SkillID { get; set; }

        public int PersonID { get; set; }

        public int ExperienceYears { get; set; }

        public int ExperienceMonths { get; set; }

        public bool HasCoreCompetency { get; set; }

        public int SkillRating { get; set; }

        public bool IsPrimary { get; set; }
    }
}