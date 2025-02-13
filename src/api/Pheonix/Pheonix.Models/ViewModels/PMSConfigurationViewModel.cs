using Newtonsoft.Json;
using Pheonix.Models.ViewModels;
using System;


namespace Pheonix.Models
{
    public class PMSConfigurationViewModel
    {
        public int Id { get; set; }
        public Nullable<int> Project { get; set; }
        public Nullable<int> Role { get; set; }
        public Nullable<int> PersonID { get; set; }
    }

    public class ProjectSkillViewModel
    {
        public int ID { get; set; }
        public Nullable<int> ProjectID { get; set; }
        public Nullable<int> SkillID { get; set; }
    }
}
