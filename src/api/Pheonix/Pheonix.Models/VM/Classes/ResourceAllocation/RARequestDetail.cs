using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.VM.Classes.ResourceAllocation
{
    public class RARequestDetail
    {
        public bool IsRmg { get; set; }
        public int EmpID { get; set; }
        public int CreatedBy { get; set; }
        public int ModifyBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifyDate { get; set; }
        public string Comments { get; set; }
        public string RMGComments { get; set; }
        public int Status { get; set; }
        public bool IsDeleted { get; set; }
    }
}
