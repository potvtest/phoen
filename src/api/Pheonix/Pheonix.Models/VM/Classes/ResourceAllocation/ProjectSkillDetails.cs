using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.VM.Classes.ResourceAllocation
{
    public class ProjectSkillDetails
    {
        public int ID { get; set; }
        public Nullable<int> ProjectID { get; set; }
        public Nullable<int> SkillID { get; set; }
        public string SkillName { get; set; }
        public Nullable<bool> IsDeleted { get; set; }
    }
}
