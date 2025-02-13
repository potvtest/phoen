using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.VM.Classes.ResourceAllocation
{
    public class RARatings
    {
        public Nullable<int> Technical { get; set; }
        public Nullable<int> Process { get; set; }
        public Nullable<int> Discipline { get; set; }
        public Nullable<int> Communication { get; set; }
        public Nullable<int> Quality { get; set; }
        public Nullable<int> Timelines { get; set; }
        public int AllocationID { get; set; }
        public List<RAProjectSkillRatings> ProjectSkillRatings { get; set; }
    }
}
