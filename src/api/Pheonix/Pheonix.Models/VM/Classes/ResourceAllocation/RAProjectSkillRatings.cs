using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.VM.Classes.ResourceAllocation
{
    public class RAProjectSkillRatings
    {
        public int ID { get; set; }
        public Nullable<int> SkillId { get; set; }
        public Nullable<int> PersonID { get; set; }
        public Nullable<int> Rating { get; set; }
        public Nullable<bool> IsDeleted { get; set; }
        public int ReleaseRequestID { get; set; }
        public int AllocationRequestID { get; set; }
    }
}
