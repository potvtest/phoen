using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pheonix.Models.VM
{
    public class ChangeSet<T>
    {
        public T OldModel { get; set; }
        public T NewModel { get; set; }
        public string ModuleCode { get; set; }
        public int ModuleId { get; set; }
        public bool SendForApproval { get; set; }
    }
}
